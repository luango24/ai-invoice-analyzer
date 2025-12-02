Gmail Invoice AI Analyzer

Processes supermarket invoices downloaded from Gmail, extracts product data, categorizes items using a local AI model (Ollama), and generates an HTML summary report.

Requirements

.NET 8

Ollama installed and running

Local model: llama3.2:latest

Gmail API enabled (OAuth)

Configuration

Copy appsettings.sample.json → appsettings.json.

Fill in:

Working folder path

Gmail API credentials

Ollama base URL and model name

Run
dotnet run


Pipeline:

(Optional) Download invoice PDFs from Gmail

Parse and extract invoice data

Categorize items using local AI

Generate summary.html + summary.IA.html

Project Structure

Manager.cs — Main workflow

Services/ — Gmail + Ollama integration

PDF/ — PDF parsing and item extraction

Models/ — Data structures

Reports/ — HTML report generator

License

MIT