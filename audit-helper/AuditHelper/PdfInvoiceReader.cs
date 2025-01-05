using System.Text.RegularExpressions;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Writer;
using Page = UglyToad.PdfPig.Content.Page;

namespace audit_helper;

public class PdfInvoiceReader : IInvoiceReader
{

    private TesseractEngine _engine;
    private string _inputFileName = "";
    private string _currentInvoiceName = "";
    private string _nextInvoiceName = "";

    public PdfInvoiceReader()
    {
    }

    public void SplitFile(TesseractEngine engine, string inputFileName)
    {
        _engine = engine;
        _inputFileName = inputFileName;
        var currentPage = 1;

        using var stream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
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

            var numberOfPages = endPage - startPage + 1;
            var newFileName = DirectoryHelper.GenerateNewFileName(inputFileName, _currentInvoiceName, numberOfPages);
            File.WriteAllBytes(newFileName, fileBytes);

            Console.WriteLine($"Saved: {newFileName}");

            currentPage = endPage + 1;
            if (!string.IsNullOrWhiteSpace(_nextInvoiceName))
            {
                _currentInvoiceName = _nextInvoiceName;
            }
        }
    }

    private int FindInvoiceEndPage(PdfDocument document, int startPage)
    {
        if (string.IsNullOrWhiteSpace(_currentInvoiceName))
        {
            var pageText = ExtractTextFromPage(document, 1);
            _currentInvoiceName = pageText.GetInvoiceName();
        }

        var totalPages = document.NumberOfPages;
        for (var i = startPage; i <= totalPages; i++)
        {
            if (i + 1 > totalPages) continue;

            var pageText = ExtractTextFromPage(document, i + 1);
            if (!pageText.IsContainInvoiceHeader()) continue;

            _nextInvoiceName = pageText.GetInvoiceName();

            return i;
        }

        return totalPages;
    }

    private string ExtractTextFromPage(PdfDocument document, int pageNumber)
    {
        var pageText = ExtractTextFromPdfPage(document.GetPage(pageNumber));
        if (string.IsNullOrWhiteSpace(pageText))
        {
            var convertedFileName = PdfConverter.PdfToJpeg(_inputFileName, pageNumber);
            pageText = ImageReader.GetText(_engine, convertedFileName);
        }

        return pageText;
    }

    private static string ExtractTextFromPdfPage(Page page)
    {
        var pageText = string.Join(" ", page.GetWords()).ToLower();

        return pageText;
    }
}