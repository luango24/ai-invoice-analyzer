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


🔧 Configuration (appsettings.sample.json)

Before running the console application, create a local appsettings.json based on the included template:

GmailInvoiceAnalyzer/Config/appsettings.sample.json


Copy it to:

GmailInvoiceAnalyzer/Config/appsettings.json


and update the values according to your environment.

Configuration fields explained:

Setting: Description
SecretJsonFile: Full path to your Google API credentials JSON. This file never goes into Git.
QueryString: "Gmail search query used to locate invoice emails (e.g., filter by date, attachments, or sender)."
ModelName: Name of the model to use in Ollama (must match ollama pull <model>).
BaseAddress: URL where Ollama is running locally.
RequestTimeoutMinutes: Timeout for AI categorization and summarization.
WorkingFolder: Folder used to store downloaded PDFs and intermediate files.
TaskCounter: Parallelism level for PDF parsing.
PrompCounter: Parallelism level for AI calls (SemaphoreSlim).
DownloadInvoice: true = download PDFs from Gmail; false = process only local PDFs.

🧰 Tech Stack

This project combines Gmail automation, PDF parsing, and Local LLM processing using a clean and modular C# architecture.

Core Technologies
Area	Technology	Purpose
Language & Runtime	C# (.NET 8+)	Main application logic, services, PDF parsing, reporting.
AI / LLM Integration	Ollama + Llama 3.2	Local inference for categorization and executive summaries.
Google API	Google Gmail API (.NET SDK)	Read inbox messages, filter invoice emails, download attachments.
PDF Processing	iText 7 (via PdfReader) or your implemented parser	Extract invoice text, items, totals, dates.
Logging	Serilog	Structured logs stored in local /logs folder.
HTML Reports	Custom HTML generator	Creates summary and AI-assisted reports in /Reports.
Project Structure Highlights

The application follows a clean separation of concerns:

Services

GmailServiceWrapper: Gmail access + attachment retrieval

AiAnalyzer: Local LLM communication (categorization & summarization)

PDF Layer

InvoiceParser: Extracts provider, invoice number, date, and line items

Core Logic

Manager.cs: Main workflow (download → parse → analyze → summarize → report)

Models
Represent invoices, items, yearly totals, and settings.

AI Integration

Uses OllamaSharp to communicate with a local Ollama server

Automatically checks:

whether Ollama is running

whether the required model exists

Supports parallel prompts using SemaphoreSlim

Compatible with any local model that follows simple text prompt structure

Why Local AI?

✔️ No cloud dependencies
✔️ Keeps data private
✔️ Fast iterative development
✔️ No cost per token
✔️ Ideal for invoice categorization and summarization workflows

📝 License

MIT License.
