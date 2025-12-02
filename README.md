# 📄 AI Invoice Analyzer (C# Version)

A technical implementation of a Gmail-driven invoice extraction and processing workflow.  
This project fetches invoice PDFs from Gmail, parses structured financial data, and generates summary reports.

This is the **C# implementation**, built for modularity and integration with local LLMs (via **Ollama**).

---

## ⚙️ Technical Overview

### Core Capabilities
- **Email Automation:** Connects to Gmail API, queries inbox, downloads invoice PDFs.
- **Data Extraction:** Parses Vendor, Invoice ID, Dates, Line Items, and Totals.
- **AI Enhancement:** Optional LLM categorization + executive summaries via Ollama.
- **Reporting:** HTML summaries generated under `/Reports`.
- **Observability:** Logging via Serilog in `/logs`.

---

## 🚀 Execution Pipeline

The main workflow is orchestrated by **Manager.cs**:

1. Load configuration from *appsettings.json*  
2. Authenticate Gmail using OAuth  
3. (Optional) Download PDFs using Gmail query filters  
4. Parse invoice PDFs → `InvoiceData`  
5. (Optional) Categorize items + summarize via Local AI  
6. Generate HTML reports

---

## 🧱 Project Structure

### 📐 Directory Tree
```text
GmailInvoiceAnalyzer/
├── Config/
│   └── appsettings.json
├── Models/
│   ├── InvoiceData.cs
│   ├── InvoiceItem.cs
│   └── YearlyExpenseSummary.cs
├── PDF/
│   ├── InvoiceParser.cs
│   └── Helpers/
├── Reports/
│   └── ReportGenerator.cs
├── Services/
│   ├── GmailServiceWrapper.cs
│   └── AiAnalyzer.cs
└── Program.cs

## 🛠️ Requirements

.NET 8 SDK or later

Google Gmail API credentials (credentials.json)

(Optional) Ollama installed and running for local AI

(Optional) OpenAI API key

▶️ Running the Application
dotnet build
dotnet run --project GmailInvoiceAnalyzer

## 🔧 Configuration (appsettings.sample.json)

Copy the template:

GmailInvoiceAnalyzer/Config/appsettings.sample.json


Rename it to:

GmailInvoiceAnalyzer/Config/appsettings.json

### Configuration Fields
**Setting:**	Description
**SecretJsonFile:**	Full path to Google credentials JSON (never commit it).
**QueryString:**	Gmail query for filtering invoice emails.
**ModelName:**	Ollama model to run (e.g., llama3.2).
**BaseAddress:**	URL for local Ollama instance.
**RequestTimeoutMinutes:**	AI call timeout.
**WorkingFolder:**	Local folder for PDFs + output.
**TaskCounter:**	Parallelism for PDF parsing.
**PrompCounter:**	Parallelism for AI calls.
**DownloadInvoice:**	true = download from Gmail; false = use local PDFs.

## 🧰 Tech Stack Highlights
Area	Technology	Purpose
Language & Runtime	C# (.NET 8+)	Core services, parsing, logic
AI Integration	Ollama + Llama 3.2	Categorization + summarization
Google API	Gmail API (.NET SDK)	Read emails + download attachments
PDF Processing	Custom Parser	Extract invoice information
Logging	Serilog	Structured logs in /logs

✔️ Why Local AI?

Keeps data private

No cloud token cost

Very low latency

Ideal for categorization + summaries

Fully offline capability

## 📝 License

MIT License