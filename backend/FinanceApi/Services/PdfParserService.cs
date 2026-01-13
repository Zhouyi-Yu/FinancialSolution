using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using FinanceApi.Models.Entities;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FinanceApi.Services
{
    public class ParsedTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string DeduplicationHash { get; set; } = string.Empty;
    }

    public class PdfParseResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<ParsedTransaction> Transactions { get; set; } = new();
        public int TotalParsed { get; set; }
        public string BankDetected { get; set; } = "Unknown";
    }

    public class PdfParserService
    {
        public async Task<PdfParseResult> ParseBankStatementAsync(Stream pdfStream)
        {
            var result = new PdfParseResult();
            
            try
            {
                using var reader = new PdfReader(pdfStream);
                
                // AC 6: Check if PDF is encrypted
                if (reader.IsEncrypted())
                {
                    result.ErrorMessage = "PDF is password-protected. Please decrypt it first.";
                    return result;
                }

                using var pdfDoc = new PdfDocument(reader);
                var fullText = string.Empty;

                // Extract all text from PDF
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    fullText += PdfTextExtractor.GetTextFromPage(page, strategy) + "\n";
                }

                // Debug: Log first 500 chars to help diagnose issues
                Console.WriteLine($"[PDF Parser] Extracted {fullText.Length} characters");
                Console.WriteLine($"[PDF Parser] First 500 chars: {fullText.Substring(0, Math.Min(500, fullText.Length))}");

                // Detect bank type
                result.BankDetected = DetectBankType(fullText);

                // AC 2: Extract transactions based on bank format
                result.Transactions = result.BankDetected switch
                {
                    "CIBC" => ParseCIBCFormat(fullText),
                    _ => ParseGenericFormat(fullText)
                };

                result.TotalParsed = result.Transactions.Count;
                result.Success = result.TotalParsed > 0;

                if (!result.Success && result.ErrorMessage == null)
                {
                    result.ErrorMessage = $"No transactions found in the PDF. Detected bank: {result.BankDetected}. " +
                                        $"The format may not be supported. Please ensure it's a transaction statement (not a summary page).";
                }

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error parsing PDF: {ex.Message}";
                return result;
            }
        }

        private string DetectBankType(string text)
        {
            if (text.Contains("CIBC", StringComparison.OrdinalIgnoreCase))
                return "CIBC";
            if (text.Contains("TD Canada Trust", StringComparison.OrdinalIgnoreCase))
                return "TD";
            if (text.Contains("RBC", StringComparison.OrdinalIgnoreCase))
                return "RBC";
            if (text.Contains("Scotiabank", StringComparison.OrdinalIgnoreCase))
                return "Scotiabank";
            
            return "Generic";
        }

        private List<ParsedTransaction> ParseCIBCFormat(string text)
        {
            var transactions = new List<ParsedTransaction>();
            
            // CIBC pattern: "MMM DD  DESCRIPTION  AMOUNT" or "MMM DD YYYY  DESCRIPTION  AMOUNT"
            // Example: "DEC 15  STARBUCKS  -45.67" or "DEC 15 2025  SALARY DEPOSIT  +2500.00"
            var pattern = @"([A-Z]{3})\s+(\d{1,2})(?:\s+(\d{4}))?\s+(.+?)\s+([-+]?\$?\s*[\d,]+\.\d{2})";
            var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                try
                {
                    var monthStr = match.Groups[1].Value;
                    var dayStr = match.Groups[2].Value;
                    var yearStr = match.Groups[3].Success ? match.Groups[3].Value : DateTime.Now.Year.ToString();
                    var description = match.Groups[4].Value.Trim();
                    var amountStr = match.Groups[5].Value.Replace("$", "").Replace(",", "").Replace(" ", "").Trim();

                    // Parse date
                    var dateString = $"{dayStr} {monthStr} {yearStr}";
                    if (!DateTime.TryParseExact(dateString, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionDate))
                    {
                        continue; // Skip if date parsing fails
                    }

                    // Parse amount
                    if (!decimal.TryParse(amountStr, out var amount))
                    {
                        continue;
                    }

                    var transaction = new ParsedTransaction
                    {
                        Date = transactionDate,
                        Description = CleanDescription(description),
                        Amount = Math.Abs(amount),
                        Type = amount < 0 ? TransactionType.Expense : TransactionType.Income,
                        // AC 4: Create deduplication hash
                        DeduplicationHash = GenerateHash(transactionDate, description, amount)
                    };

                    transactions.Add(transaction);
                }
                catch (Exception)
                {
                    // Skip malformed entries
                    continue;
                }
            }

            // AC 3: Sort chronologically
            return transactions.OrderBy(t => t.Date).ToList();
        }

        private List<ParsedTransaction> ParseGenericFormat(string text)
        {
            var transactions = new List<ParsedTransaction>();
            
            // Generic pattern: try to find date, description, amount patterns
            var pattern = @"(\d{4}[-/]\d{2}[-/]\d{2}|\d{2}[-/]\d{2}[-/]\d{4})\s+(.+?)\s+([-+]?\$?\s*[\d,]+\.\d{2})";
            var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                try
                {
                    var dateStr = match.Groups[1].Value;
                    var description = match.Groups[2].Value.Trim();
                    var amountStr = match.Groups[3].Value.Replace("$", "").Replace(",", "").Replace(" ", "").Trim();

                    if (!DateTime.TryParse(dateStr, out var transactionDate))
                    {
                        continue;
                    }

                    if (!decimal.TryParse(amountStr, out var amount))
                    {
                        continue;
                    }

                    transactions.Add(new ParsedTransaction
                    {
                        Date = transactionDate,
                        Description = CleanDescription(description),
                        Amount = Math.Abs(amount),
                        Type = amount < 0 ? TransactionType.Expense : TransactionType.Income,
                        DeduplicationHash = GenerateHash(transactionDate, description, amount)
                    });
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return transactions.OrderBy(t => t.Date).ToList();
        }

        private string CleanDescription(string description)
        {
            // Remove excessive whitespace and special characters
            description = Regex.Replace(description, @"\s+", " ");
            description = Regex.Replace(description, @"[^\w\s\-&'.()]", "");
            return description.Trim();
        }

        private string GenerateHash(DateTime date, string description, decimal amount)
        {
            // AC 4: Create a unique hash for duplicate detection
            var combined = $"{date:yyyy-MM-dd}|{description.ToLower()}|{amount:F2}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
