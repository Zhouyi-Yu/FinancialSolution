using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.IO;

namespace PdfDebugger
{
    class Program
    {
        static void Main(string[] args)
        {
            var pdfPath = "/Users/jookenblue/Desktop/ZhouyiPortofolio/projects/FinancialSolution/data_samples/sample_bill.pdf";
            
            using var reader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(reader);
            
            Console.WriteLine($"Total Pages: {pdfDoc.GetNumberOfPages()}");
            Console.WriteLine("=".PadRight(80, '='));
            
            for (int i = 1; i <= Math.Min(pdfDoc.GetNumberOfPages(), 3); i++)
            {
                Console.WriteLine($"\n--- PAGE {i} ---\n");
                var page = pdfDoc.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                
                // Print first 2000 characters
                Console.WriteLine(text.Substring(0, Math.Min(text.Length, 2000)));
                Console.WriteLine("\n" + "=".PadRight(80, '='));
            }
        }
    }
}
