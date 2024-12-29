    using PdfSharpCore.Pdf;
    using PdfSharpCore.Pdf.IO;

    namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");


        string inputFilePath = "file.pdf";
        string outputDirectory = "file_name";

        PdfDocument inputDocument = PdfReader.Open(inputFilePath, PdfDocumentOpenMode.Import);

        int totalPages = inputDocument.PageCount;
        Console.WriteLine($"Total page: {totalPages}");

        int currentPage = 0;
        while (currentPage < totalPages)
        {
            PdfDocument newDocument = new PdfDocument();

            int startPage = currentPage;
            int endPage = FindInvoiceEndPage(inputDocument, currentPage);

            for (int i = startPage; i <= endPage; i++)
            {
                newDocument.AddPage(inputDocument.Pages[i]);
            }

            string outputFilePath = Path.Combine(outputDirectory, $"Invoice_{startPage + 1}-{endPage + 1}.pdf");
            newDocument.Save(outputFilePath);

            Console.WriteLine($"Saved: {outputFilePath}");

            currentPage = endPage + 1;
        }

        Console.WriteLine("Finnished.");
    }

    static int FindInvoiceEndPage(PdfDocument document, int startPage)
    {
        return startPage;
    }
}

