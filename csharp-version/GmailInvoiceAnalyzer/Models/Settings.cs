namespace GmailInvoiceAnalyzer.Models
{
    public class CommonSettings
    {
        public string? SecretJsonFile { get; set; }
        public string? QueryString { get; set; }
        public string? ModelName { get; set; } = "llama3.2";
        public string? BaseAddress { get; set; } = "http://localhost:11434";
        public int RequestTimeoutMinutes { get; set; } = 5;
        public string? WorkingFolder { get; set; }
        public int? TaskCounter { get; set; } = 5;
        public int? PrompCounter { get; set; } = 3;
        public bool? DownloadInvoice { get; set; } = true;
    }
}
