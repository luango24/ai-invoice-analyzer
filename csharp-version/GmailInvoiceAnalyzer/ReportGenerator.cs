using GmailInvoiceAnalyzer.Models;
using System.Text;
using System.Text.Json;

namespace GmailInvoiceAnalyzer
{
    public class ReportGenerator
    {
        public ReportGenerator() { }

        public string GenerateHtmlReport(
    YearlyExpenseSummary summary,
    string aiSummary = "",
    string reportName = "summary.html")
        {
            string jsonSummary = JsonSerializer.Serialize(summary);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset='UTF-8'>");
            sb.AppendLine("  <title>Invoice Summary</title>");
            sb.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: Arial, sans-serif; padding: 20px; }");
            sb.AppendLine("    .grid-container { display: grid; grid-template-columns: 1fr 2fr; gap: 20px; }");
            sb.AppendLine("    .summary-card, .chart-card { padding: 10px; border: 1px solid #ccc; border-radius: 8px; background: #f9f9f9; }");
            sb.AppendLine("    .summary-card pre { white-space: pre-wrap; word-wrap: break-word; max-height: 600px; overflow: auto; }");
            sb.AppendLine("    .right-column { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class='grid-container'>");

            // Left column: AI Summary
            sb.AppendLine("    <div class='summary-card'>");
            sb.AppendLine("      <h2>Executive Summary (AI)</h2>");
            sb.AppendLine($"      <pre>{aiSummary}</pre>");
            sb.AppendLine("    </div>");

            // Right column: two charts side by side
            sb.AppendLine("    <div class='right-column'>");

            // Provider Totals
            sb.AppendLine("      <div class='chart-card'>");
            sb.AppendLine("        <h2>Provider Totals</h2>");
            sb.AppendLine("        <canvas id='providerChart'></canvas>");
            sb.AppendLine("      </div>");

            // Top 10 categories pie chart
            sb.AppendLine("      <div class='chart-card'>");
            sb.AppendLine("        <h2>Top 10 Expense Categories</h2>");
            sb.AppendLine("        <canvas id='topCategoryPieChart'></canvas>");
            sb.AppendLine("      </div>");

            sb.AppendLine("    </div>"); // end right-column
            sb.AppendLine("  </div>"); // end grid-container

            // Chart.js scripts
            sb.AppendLine("<script>");
            sb.AppendLine($"const summary = {jsonSummary};");

            // Provider Totals
            sb.AppendLine(@"
const ctxProvider = document.getElementById('providerChart').getContext('2d');
new Chart(ctxProvider, {
    type: 'bar',
    data: {
        labels: Object.keys(summary.ProviderTotals),
        datasets: [{
            label: 'Provider Totals',
            data: Object.values(summary.ProviderTotals),
            backgroundColor: 'rgba(75,192,192,0.6)'
        }]
    },
    options: { indexAxis: 'y', responsive: true }
});");

            // Top 10 categories pie chart
            sb.AppendLine(@"
const ctxTopCategory = document.getElementById('topCategoryPieChart').getContext('2d');
const sortedCategories = Object.entries(summary.CategoryTotals)
    .sort((a,b) => b[1]-a[1])
    .slice(0,10);
new Chart(ctxTopCategory, {
    type: 'pie',
    data: {
        labels: sortedCategories.map(c => c[0]),
        datasets: [{
            data: sortedCategories.map(c => c[1]),
            backgroundColor: [
                'rgba(255,99,132,0.6)','rgba(54,162,235,0.6)','rgba(255,206,86,0.6)',
                'rgba(75,192,192,0.6)','rgba(153,102,255,0.6)','rgba(255,159,64,0.6)',
                'rgba(199,199,199,0.6)','rgba(83,102,255,0.6)','rgba(255,102,178,0.6)','rgba(102,255,153,0.6)'
            ]
        }]
    },
    options: { responsive: true }
});");

            sb.AppendLine("</script>");
            sb.AppendLine("</body></html>");

            // Save file
            Directory.CreateDirectory("Reports");
            string htmlReport = Path.Combine("Reports", reportName);
            File.WriteAllText(htmlReport, sb.ToString());

            return htmlReport;
        }
    }
}
