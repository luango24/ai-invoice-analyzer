namespace GmailInvoiceAnalyzer.Models
{
    public class InvoiceData
    {
        public string InvoiceId { get; set; } = "";
        public string Supermarket { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string RawText { get; set; } = "";
        public List<InvoiceItem> Items { get; set; } = [];

    }

}
