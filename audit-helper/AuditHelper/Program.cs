using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        string inputFilePath = @"C:\repos\_PRIVATE\audit-helper\invoices\invoice_fuzion.pdf";
        string outputDirectory = @"C:\repos\_PRIVATE\audit-helper\invoices";

        int currentPage = 1;
        using (var stream = File.OpenRead(inputFilePath))
        using (PdfDocument inputDocument = PdfDocument.Open(stream))
        {
            int totalPages = inputDocument.NumberOfPages;
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

    private static int FindInvoiceEndPage(PdfDocument document, int startPage)
    {
        var totalPages = document.NumberOfPages;
        for (var i = startPage; i <= totalPages; i++)
        {
            if (i + 1 > totalPages) continue;

            var pageText = ExtractTextFromPage(document.GetPage(i + 1));

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

    private static string ExtractInvoiceName(Page page)
    {
        var invoiceKeyword = "/2024";
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

