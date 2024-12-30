using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace audit_helper;

public class PdfSplitter : IFileSplitter
{
    private readonly string _fileName;
    private string _currentInvoiceName = "";
    private string _nextInvoiceName = "";

    public PdfSplitter(string fileName)
    {
        _fileName = fileName;
    }

    public void SplitFile()
    {
        // UNDONE: zrobic mniejszy plik z invoice_multiple i przetestować
        var currentPage = 1;

        using var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read);
        using var inputDocument = PdfDocument.Open(stream);

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

            var outputFileName = GetOutputFileName(_fileName);
            File.WriteAllBytes(outputFileName, fileBytes);

            Console.WriteLine($"Saved: {outputFileName}");

            currentPage = endPage + 1;
            if (!string.IsNullOrWhiteSpace(_nextInvoiceName))
            {
                _currentInvoiceName = _nextInvoiceName;
            }
        }
    }

    private string GetOutputFileName(string fileName)
    {
        var outputFilePath = @$"{fileName}\splitted";
        Directory.CreateDirectory(outputFilePath);
        var outputFileName = Path.Combine(outputFilePath, $"{_currentInvoiceName}.pdf");

        return outputFileName;
    }

    private int FindInvoiceEndPage(PdfDocument document, int startPage)
    {
        if (string.IsNullOrWhiteSpace(_currentInvoiceName))
        {
            var pageText = ExtractTextFromPage(document, 1);
            _currentInvoiceName = ExtractInvoiceName(pageText);
        }

        var totalPages = document.NumberOfPages;
        for (var i = startPage; i <= totalPages; i++)
        {
            if (i + 1 > totalPages) continue;

            var pageText = ExtractTextFromPage(document, i + 1);

            if (!ContainsInvoiceHeader(pageText)) continue;

            _nextInvoiceName = ExtractInvoiceName(pageText);

            return i;
        }

        return totalPages;
    }

    private string ExtractTextFromPage(PdfDocument document, int pageNumber)
    {
        var pageText = ExtractTextFromPageOld(document.GetPage(pageNumber));
        if (string.IsNullOrWhiteSpace(pageText))
        {
            var convertedFileName = PdfConverter.PdfToJpeg(_fileName, pageNumber);
            pageText = ImageReader.GetText(convertedFileName);
        }

        return pageText;
    }

    private static string ExtractTextFromPageOld(Page page)
    {
        var pageText = string.Join(" ", page.GetWords()).ToLower();

        return pageText;
    }

    static bool ContainsInvoiceHeader(string pageText)
    {
        // string[] invoiceKeywords = { "Faktura VAT", "Invoice No", "Nr faktury", "Data wystawienia" };
        string[] invoiceKeywords = { "Data wystawienia", "Invoice Date", "Invoice No" };
        foreach (var keyword in invoiceKeywords)
        {
            if (pageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ExtractInvoiceName(string pageText)
    {
        string pattern = @"\b\S*/2023\S*\b";

        Match match = Regex.Match(pageText, pattern);

        if (match.Success)
        {
            var result = match.Value.Replace('/', '_');
            return result;
        }

        return "";
    }
}