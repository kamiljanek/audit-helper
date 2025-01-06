using UglyToad.PdfPig;
using UglyToad.PdfPig.Writer;
using Page = UglyToad.PdfPig.Content.Page;

namespace audit_helper;

public class PdfInvoiceReader : IInvoiceReader
{
    private readonly string _inputFileName;
    private readonly PdfConverter _pdfConverter;
    private string _currentConvertedFileName;
    private string _currentInvoiceName = "";
    private string _nextInvoiceName = "";
    private int _singleInvoiceMaxPages = 5;

    public PdfInvoiceReader(string inputFileName)
    {
        _inputFileName = inputFileName;
        _pdfConverter = new PdfConverter();

    }

    public void SplitFile()
    {
        var currentPage = 1;

        using var stream = new FileStream(_inputFileName, FileMode.Open, FileAccess.Read);
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
            var newFileName = DirectoryHelper.GenerateNewFileName(_inputFileName, _currentInvoiceName, numberOfPages);
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
        var totalPages = document.NumberOfPages;

        if (string.IsNullOrWhiteSpace(_currentInvoiceName))
        {
            var searchingName = Path.GetFileNameWithoutExtension(_inputFileName);
            if (totalPages <= _singleInvoiceMaxPages & TryFindInvoiceName(document, 1, out _currentInvoiceName, searchingName: searchingName, useImageRecognition: true))
            {
                return totalPages;
            }
        }

        for (var i = startPage; i <= totalPages; i++)
        {
            if (i + 1 > totalPages) continue;

            var pageText = ExtractTextFromPage(document, i + 1);
            if (pageText.IsFirstPage())
            {
                TryFindInvoiceName(document, i + 1, out _nextInvoiceName, useImageRecognition: true);
                return i;
            }

            if (pageText.IsLastPage())
            {
                TryFindInvoiceName(document, i + 2, out _nextInvoiceName, useImageRecognition: true);
                return i + 1;
            }
        }

        return totalPages;
    }

    private string ExtractTextFromPage(PdfDocument document, int pageNumber)
    {
        var pageText = ExtractTextFromPdfPage(document.GetPage(pageNumber));
        if (string.IsNullOrWhiteSpace(pageText))
        {
            _currentConvertedFileName = _pdfConverter.PdfToJpeg(_inputFileName, pageNumber);
            pageText = ImageReader.GetTextDefault(_currentConvertedFileName);
        }

        return pageText;
    }

    private static string ExtractTextFromPdfPage(Page page)
    {
        var pageText = string.Join(" ", page.GetWords()).ToLower();

        return pageText;
    }

    private bool TryFindInvoiceName(
        PdfDocument document,
        int pageNumber,
        out string invoiceName,
        string searchingName = null,
        bool useImageRecognition = false)
    {
        invoiceName = Common.UnknownName;
        var pageTexts = new List<string>();

        var pageText = ExtractTextFromPage(document, pageNumber);

        if (!string.IsNullOrEmpty(searchingName))
        {
            if (pageText.ContainsName(searchingName))
            {
                invoiceName = Path.GetFileName(searchingName);
                return true;
            }
        }

        var extractedInvoiceName = pageText.GetInvoiceName();
        if (extractedInvoiceName != Common.UnknownName)
        {
            invoiceName = extractedInvoiceName;
            return true;
        }

        if (useImageRecognition)
        {
            var bitmap = ImageReader.PreprocessImage(_currentConvertedFileName);
            var languages = new Func<string>[]
            {
                () => ImageReader.GetTextPl(bitmap),
                () => ImageReader.GetTextEn(bitmap),
                () => ImageReader.GetTextDe(bitmap),
                () => ImageReader.GetTextCz(bitmap)
            };

            foreach (var getText in languages)
            {
                var text = getText();
                pageTexts.Add(text);

                if (!string.IsNullOrEmpty(searchingName) && text.ContainsName(searchingName))
                {
                    invoiceName = Path.GetFileName(searchingName);
                    return true;
                }

                if (string.IsNullOrEmpty(searchingName))
                {
                    extractedInvoiceName = text.GetInvoiceName();
                    if (extractedInvoiceName != Common.UnknownName)
                    {
                        invoiceName = extractedInvoiceName;
                        return true;
                    }
                }
            }

            extractedInvoiceName = pageTexts.GetInvoiceName();
            if (!string.IsNullOrEmpty(extractedInvoiceName))
            {
                invoiceName = extractedInvoiceName;
                return true;
            }
        }

        return false;
    }
}