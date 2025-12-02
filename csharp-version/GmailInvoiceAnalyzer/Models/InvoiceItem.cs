namespace GmailInvoiceAnalyzer.Models
{
    public class InvoiceItem
    {
        public string? Number { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal ITBMS { get; set; }
        public decimal LinePrice { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
