using GmailInvoiceAnalyzer.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

namespace GmailInvoiceAnalyzer.Services
{
    public class GmailServiceWrapper
    {
        private readonly GmailService _service;
        private readonly CommonSettings _settings;

        public GmailServiceWrapper(CommonSettings settings)
        {
            _settings = settings;
            using var stream = new FileStream(_settings.SecretJsonFile!, FileMode.Open, FileAccess.Read);

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                [GmailService.Scope.GmailReadonly],
                "user",
                CancellationToken.None
            ).Result;

            _service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail Invoice Analyzer",
            });
        }

        public IList<Message> SearchInvoices()
        {
            var request = _service.Users.Messages.List("me");
            request.Q = _settings.QueryString;
            return request.Execute().Messages ?? [];
        }

        public async Task<byte[]?> GetPdfAttachmentAsync(string messageId)
        {
            var message = await _service.Users.Messages.Get("me", messageId).ExecuteAsync();

            var pdfPart = FindPdfPart(message.Payload);

            if (pdfPart == null)
                return null;

            var attachment = await _service.Users.Messages.Attachments
                .Get("me", messageId, pdfPart.Body.AttachmentId)
                .ExecuteAsync();
            var base64 = attachment.Data.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(base64);

        }

        private static MessagePart? FindPdfPart(MessagePart part)
        {
            if (!string.IsNullOrEmpty(part.Filename) &&
                part.Filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return part;
            }

            if (part.Parts != null)
            {
                foreach (var sub in part.Parts)
                {
                    var found = FindPdfPart(sub);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }
    }
}

