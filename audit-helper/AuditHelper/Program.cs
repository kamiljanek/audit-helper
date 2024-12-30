using PDFtoImage;
using Tesseract;
using UglyToad.PdfPig.Writer;
using Page = UglyToad.PdfPig.Content.Page;
using PdfDocument = UglyToad.PdfPig.PdfDocument;

namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        string inputFilePath = @"C:\repos\_PRIVATE\audit-helper\invoices\invoice_scan.pdf";
        string outputDirectory = @"C:\repos\_PRIVATE\audit-helper\invoices\splitted";

        inputFilePath = ConvertPdfScanToJpeg(inputFilePath);

        var pt = ExtractTextFromImage(inputFilePath);


        var currentPage = 1;
        using (var stream = File.OpenRead(inputFilePath))
        using (var inputDocument = PdfDocument.Open(stream))
        {
            var totalPages = inputDocument.NumberOfPages;
            Console.WriteLine($"Total page: {totalPages}");

            while (currentPage <= totalPages)
            {
                var startPage = currentPage;
                var endPage = FindInvoiceEndPage(inputDocument, currentPage);

                var builder = new PdfDocumentBuilder();

                for (var i = startPage; i <= endPage; i++)
                {
                    builder.AddPage(inputDocument, i);
                }

                byte[] fileBytes = builder.Build();

                var invoiceName = ExtractInvoiceName(inputDocument.GetPage(startPage));
                if (string.IsNullOrWhiteSpace(invoiceName))
                {
                    invoiceName = $"Invoice_{startPage}-{endPage}";
                }


                var outputFilePath = Path.Combine(outputDirectory, $"{invoiceName}.pdf");
                File.WriteAllBytes(outputFilePath, fileBytes);

                Console.WriteLine($"Saved: {outputFilePath}");

                currentPage = endPage + 1;
            }
        }

        Console.WriteLine("Finnished.");
    }

    private static string ConvertPdfScanToJpeg(string inputFilePath)
    {
        using FileStream pdfStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
        var outputFilePath = Path.ChangeExtension(inputFilePath, ".jpeg");
        using FileStream imageStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite);
        var a = new Index(0);
        Conversion.SaveJpeg(imageStream, pdfStream, a);
        return outputFilePath;
    }

    private static int FindInvoiceEndPage(PdfDocument document, int startPage)
    {
        var totalPages = document.NumberOfPages;
        for (var i = startPage; i <= totalPages; i++)
        {
            var pageText = ExtractTextFromPage(document.GetPage(i));

            // if (i + 1 > totalPages) continue;

            // var pageText = ExtractTextFromPage(document.GetPage(i + 1));

            if (ContainsInvoiceHeader(pageText))
            {
                return i;
            }
        }

        return totalPages;
    }

    private static string ExtractTextFromPage(Page page)
    {
        var pageText = string.Join(" ", page.GetWords()).ToLower();

        return pageText;
    }

    private static string ExtractTextFromImage(string imagePath)
    {
        var tessDataPath = @"C:\repos\_PRIVATE\audit-helper\invoices\tessdata";
        using var engine = new TesseractEngine(tessDataPath, "pol", EngineMode.Default);

        using var img = Pix.LoadFromFile(imagePath);
        using var page = engine.Process(img);

        return page.GetText();
    }

    private static string ExtractInvoiceName(Page page)
    {
        var invoiceKeyword = "/2023";
        foreach (var word in page.GetWords().ToArray())
        {
            if (word.Text.Contains(invoiceKeyword, StringComparison.OrdinalIgnoreCase))
            {
                var result = word.Text.Replace('/', '_');
                return result;
            }
        }

        return "";
    }

    static bool ContainsInvoiceHeader(string pageText)
    {
        // string[] invoiceKeywords = { "Faktura VAT", "Invoice No", "Nr faktury", "Data wystawienia" };
        string[] invoiceKeywords = { "Data wystawienia" };
        foreach (var keyword in invoiceKeywords)
        {
            if (pageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

