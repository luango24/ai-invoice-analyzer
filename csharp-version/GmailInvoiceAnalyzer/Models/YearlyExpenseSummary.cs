namespace GmailInvoiceAnalyzer.Models
{
    public class YearlyExpenseSummary
    {
        public Dictionary<string, decimal> MonthlyTotals { get; set; } = [];
        public Dictionary<string, decimal> CategoryTotals { get; set; } = [];
        public Dictionary<string, decimal> ProviderTotals { get; set; } = [];
    }
}
