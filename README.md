📄 AI Invoice Analyzer (C# Version)

A technical implementation of a Gmail-driven invoice extraction and processing workflow.
This project fetches invoice PDFs from Gmail, parses structured financial data, and generates summary reports.
It serves as the C# implementation, with a Python version planned for later.

⚙️ Technical Overview
✔️ Core Capabilities

Authenticates with Gmail API using OAuth 2.0

Queries and downloads invoice-related emails

Extracts PDF attachments and parses:

Vendor information

Invoice ID

Dates

Line items

Totals

Generates HTML reports (standard + IA-enhanced version)

Integrates optional local AI (via Ollama) or cloud AI (via OpenAI API)

Includes structured logging to /logs directory

📐 Architecture

```text
GmailInvoiceAnalyzer/
|-- Config/
|   |-- appsettings.json (runtime config)
|-- Models/
|   |-- InvoiceData.cs
|   |-- InvoiceItem.cs
|   |-- YearlyExpenseSummary.cs
|-- PDF/
|   |-- InvoiceParser.cs
|   |-- Helpers/
|-- Reports/
|   |-- ReportGenerator.cs
|-- Services/
|   |-- GmailServiceWrapper.cs
|   |-- AiAnalyzer.cs
|-- Program.cs

🧱 Layer Responsibilities

Services/
API integration, AI orchestration, Gmail operations.

PDF/
PDF extraction, text normalization, parsing helpers.

Reports/
HTML output generation.

Models/
Strongly-typed data structures used throughout the pipeline.

🚀 How It Works

Load Configuration
Reads Settings from appsettings.json.

Authenticate Gmail API
Uses OAuth token storage and refresh credentials.

Search for Invoice Emails
Filters incoming messages using Gmail query strings.

Download Attachments
Saves PDFs locally using a deterministic naming scheme.

Parse Invoice PDF
Uses a custom text extraction + pattern matching flow.

AI Enhancement (Optional)
Sends extracted text to AI for enrichment or error correction.

Generate Reports
Produces summary HTML pages under /Reports.

🛠️ Requirements

.NET 8 SDK or later

Gmail API credentials (credentials.json)

(Optional) Local AI engine via Ollama

(Optional) OpenAI API key

▶️ Running the Application
dotnet build
dotnet run --project GmailInvoiceAnalyzer


Configure Gmail and AI settings in:

/Config/appsettings.json

📝 License

MIT License.
