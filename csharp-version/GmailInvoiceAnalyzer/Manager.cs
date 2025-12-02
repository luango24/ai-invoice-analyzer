using GmailInvoiceAnalyzer.Models;
using GmailInvoiceAnalyzer.PDF;
using GmailInvoiceAnalyzer.PDF.Helpers;
using GmailInvoiceAnalyzer.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace GmailInvoiceAnalyzer
{
    public class Manager
    {
        private readonly CommonSettings _settings;
        private readonly AiAnalyzer _aiAnalyzer;
        private readonly InvoiceHelper _categorizationHelper;
        private readonly InvoiceParser _invoiceParser;

        public Manager(IConfigurationSection configuration)
        {
            _settings = configuration.Get<CommonSettings>()!;
            _aiAnalyzer = new AiAnalyzer(_settings);
            _categorizationHelper = new InvoiceHelper(_aiAnalyzer);
            _invoiceParser = new InvoiceParser();
        }
        public async Task Execute()
        {
            if (!Directory.Exists(_settings.WorkingFolder))
            {
                Directory.CreateDirectory(_settings.WorkingFolder!);
            }

            if (! await _aiAnalyzer.IsOllamaRunning())
            {
                Log.Error("Ollama AI service is not running. Please start Ollama and try again.");
                return;
            }

            if(! await _aiAnalyzer.ModelExistsAsync())
            {
                Log.Error($"Ollama model {_settings.ModelName} not found. Please download the model using ollama pull {_settings.ModelName} and try again.");
                return;
            }

            Log.Information($"Ollama is running and model {_settings.ModelName} is available.");


            //Allow skip the download step for testing with local files
            if (_settings.DownloadInvoice!.Value)
            {
                await GetPdfInvoices();

            }
            else
            {
                Log.Information("Skipping PDF download as per configuration.");
            }

            var pdfFiles = Directory.GetFiles(_settings.WorkingFolder!, "*.pdf");

            if (pdfFiles.Length == 0)
            {
                Log.Warning("No PDF files found to process. Exiting.");
                return;
            }

            // Step 2: Process the PDFs
            var (_, monthlyTotals, categoryTotals, providerTotals) =
                await ProcessPdfInvoices(pdfFiles);

            // Step 3: Summarize
            var (aiSummary, fullSummary) = await GenerateSummary(
                 monthlyTotals, categoryTotals, providerTotals);

            fullSummary.MonthlyTotals = fullSummary.MonthlyTotals
            .OrderBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);


            //Step 4: Report
            string reportName = "summary.IA.html";
            var reportGenerator = new ReportGenerator();
            string htmlReport = reportGenerator.GenerateHtmlReport(fullSummary, aiSummary, reportName);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = htmlReport,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Log.Warning($"Could not open the report automatically: {ex.Message}");
            }

            Log.Information("Invoice Analysis Process Complete.");
        }

        // --- Step 2 Download Pdf Invoices ---
        private async Task<int> GetPdfInvoices()
        {
            Log.Information("Reading invoices from Gmail...");
            int pdfCount = 0;
            var gmail = new GmailServiceWrapper(_settings);
            var invoices = gmail.SearchInvoices();
            string folderPath = _settings.WorkingFolder!;

            foreach (var msg in invoices)
            {
                var pdfBytes = await gmail.GetPdfAttachmentAsync(msg.Id);

                if (pdfBytes != null)
                {
                    string filePath = Path.Combine(folderPath, $"invoice_{msg.Id}.pdf");
                    File.WriteAllBytes(filePath, pdfBytes);
                    pdfCount++;
                }
            }

            Log.Information($"Total PDF invoices downloaded: {pdfCount}");
            return pdfCount;
        }

        // --- Step 2: Processing ---
        private async Task<(ConcurrentBag<InvoiceData> AllInvoices,
                          ConcurrentDictionary<string, decimal> MonthlyTotals,
                          ConcurrentDictionary<string, decimal> CategoryTotals,
                          ConcurrentDictionary<string, decimal> ProviderTotals)>
            ProcessPdfInvoices(string[] pdfFiles)
        {
            Log.Information($"Starting parallel processing of {pdfFiles.Length} invoices...");

            var allInvoices = new ConcurrentBag<InvoiceData>();
            var monthlyTotals = new ConcurrentDictionary<string, decimal>();
            var categoryTotals = new ConcurrentDictionary<string, decimal>();
            var providerTotals = new ConcurrentDictionary<string, decimal>();

            var parallelOptions = new ParallelOptions
            {
                // Use the configured TaskCounter, defaulting to 5
                MaxDegreeOfParallelism = _settings.TaskCounter ?? 5
            };

            // Use the configured PrompCounter, defaulting to 3
            var aiSemaphore = new SemaphoreSlim(_settings.PrompCounter ?? 3);

            await Parallel.ForEachAsync(pdfFiles, parallelOptions, async (file, ct) =>
            {
                Log.Information($"Parsing {Path.GetFileName(file)}...");

                try
                {
                    var invoice = _invoiceParser.ParsePdf(file);
                    invoice.Items = InvoiceHelper.ExtractItems(invoice.RawText);

                    // Categorization by AI inside SemaphoreSlim
                    foreach (var item in invoice.Items)
                    {
                        await aiSemaphore.WaitAsync(ct);
                        try
                        {
                            item.Category = await _categorizationHelper.CategorizeItem(item.Description!);
                        }
                        finally
                        {
                            aiSemaphore.Release();
                        }
                    }

                    // THREAD-SAFE accumulation of totals
                    var month = invoice.Date.ToString("yyyy-MM");
                    monthlyTotals.AddOrUpdate(month, invoice.Total, (_, old) => old + invoice.Total);

                    foreach (var item in invoice.Items)
                    {
                        var cat = item.Category ?? "Uncategorized";
                        categoryTotals.AddOrUpdate(cat, item.ItemPrice, (_, old) => old + item.ItemPrice);
                    }

                    var provider = invoice.Supermarket ?? "Unknown";
                    providerTotals.AddOrUpdate(provider, invoice.Total, (_, old) => old + invoice.Total);

                    allInvoices.Add(invoice);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error processing file {file}");
                }
            });

            return (allInvoices, monthlyTotals, categoryTotals, providerTotals);
        }

        // --- Step 3: Summarizing ---
        private async Task<(string AiSummary, YearlyExpenseSummary FullSummary)> GenerateSummary(
            ConcurrentDictionary<string, decimal> monthlyTotals,
            ConcurrentDictionary<string, decimal> categoryTotals,
            ConcurrentDictionary<string, decimal> providerTotals)
        {
            Log.Information("Generating final summaries and reports...");

            var summary = new YearlyExpenseSummary
            {
                MonthlyTotals = monthlyTotals.ToDictionary(k => k.Key, v => v.Value),
                CategoryTotals = categoryTotals.ToDictionary(k => k.Key, v => v.Value),
                ProviderTotals = providerTotals.ToDictionary(k => k.Key, v => v.Value)
            };

            // Serialize for AI summarization
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonMonthly = JsonSerializer.Serialize(summary.MonthlyTotals, options);
            var jsonCategories = JsonSerializer.Serialize(summary.CategoryTotals, options);
            var jsonProviders = JsonSerializer.Serialize(summary.ProviderTotals, options);

            var finalSummaryString = await _aiAnalyzer.SummarizeInvoiceAsync(jsonMonthly, jsonCategories, jsonProviders);

            Log.Information("=== FINAL AI SUMMARY ===");

            return (finalSummaryString, summary);
        }
    }
}
