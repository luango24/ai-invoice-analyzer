using GmailInvoiceAnalyzer.Models;
using UglyToad.PdfPig;

namespace GmailInvoiceAnalyzer.PDF
{
    public class InvoiceParser
    {
        public InvoiceData ParsePdf(string pdfFile)
        {
            var invoice = new InvoiceData();

            using var pdf = PdfDocument.Open(pdfFile);
            var text = string.Join("\n", pdf.GetPages().Select(p => p.Text));
            string pdfName = Path.GetFileNameWithoutExtension(pdfFile);
            invoice.InvoiceId = pdfName.Substring(pdfName.LastIndexOfAny(new char[] { '_'}) + 1);
            invoice.RawText = text;

            invoice.Supermarket = DetectSupermarket(text);
            invoice.Date = DetectDate(text);
            invoice.Total = DetectTotal(text);

            return invoice;
        }

        private string DetectSupermarket(string text)
        {
            if (text.Contains("Super 99", StringComparison.OrdinalIgnoreCase)
                || text.Contains("IMPORTADORA RICAMAR", StringComparison.OrdinalIgnoreCase))
                return "Super 99";
            if (text.Contains("SUPERMERCADOS XTRA", StringComparison.OrdinalIgnoreCase))
                return "XTRA";
            return "Uknonwn";
        }

        private DateTime DetectDate(string text)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\d{2}/\d{2}/\d{4}");
            if (matches.Count > 0)
                return DateTime.Parse(matches[0].Value);
            return DateTime.MinValue;
        }

        private decimal DetectTotal(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(text, @"Total\s*\$?(\d+(\.\d+)?)");
            if (match.Success)
                return decimal.Parse(match.Groups[1].Value);
            return 0;
        }
    }
}

