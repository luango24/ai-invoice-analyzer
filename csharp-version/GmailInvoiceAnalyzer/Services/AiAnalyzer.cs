using GmailInvoiceAnalyzer.Models;
using OllamaSharp;
using OllamaSharp.Models;
using Serilog;
using System.Text;
namespace GmailInvoiceAnalyzer.Services
{
    public class AiAnalyzer
    {
        private readonly OllamaApiClient _client;
        private readonly string _modelName;

        public AiAnalyzer(CommonSettings settings)
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(settings.RequestTimeoutMinutes),
                BaseAddress = new Uri(settings.BaseAddress!)
            };

            _client = new OllamaApiClient(httpClient);
            _modelName = settings.ModelName!;
        }

        public async Task<bool> IsOllamaRunning()
        {
            try
            {
                return await _client.IsRunningAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ModelExistsAsync()
        {
            try
            {
                var models = await _client.ListLocalModelsAsync();
                return models.Any(m => m.Name == _modelName);
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> CategorizeWithAIAsync(string description)
        {
            var prompt = $@"
Categorize this supermarket product into one category:
[Proteins, Vegetables, Fruits, Dairy, Dry Food, Snacks, Hygiene, Cleaning Supplies, Pet Supplies, Frozen, Drinks, Canned Goods, Bakery, Baby, Cereals, Sauces, Other]

Product: ""{description}""

ONLY return the category name.
";

            var req = new GenerateRequest
            {
                Model = _modelName,
                Prompt = prompt,
                Stream = false
            };

            var sb = new StringBuilder();

            try
            {
                await foreach (var part in _client.GenerateAsync(req))
                {
                    if (!string.IsNullOrEmpty(part!.Response))
                        sb.Append(part.Response.Trim());
                }

                return sb.ToString().Trim();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AI request failed");
                return $"ERROR: AI request failed: {ex.Message}";
            }
        }

        public async Task<string> SummarizeInvoiceAsync(string jsonMonthly,
                                                        string jsonCategories,
                                                        string jsonProviders)
        {

            string prompt = $@"
Here is the consolidated summary of all invoices:

Monthly Totals:
{jsonMonthly}

Category Totals:
{jsonCategories}

Provider Totals:
{jsonProviders}

Generate an executive summary.
Highlight spending trends, patterns, anomalies, and optimization opportunities.
Be clear, concise, and professional.
";

            var request = new GenerateRequest
            {
                Model = _modelName,
                Prompt = prompt,
                Stream = false
            };

            var sb = new StringBuilder();

            try
            {
                await foreach (var part in _client.GenerateAsync(request))
                {
                    if (!string.IsNullOrEmpty(part!.Response))
                        sb.Append(part.Response);
                }

                return sb.ToString().Trim();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AI request failed");
                return $"ERROR: AI request failed: {ex.Message}";
            }
        }
    }
}
