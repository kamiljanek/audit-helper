using Tesseract;

namespace audit_helper;

public class InvoiceSplitter
{
    private readonly string _inputFileName;
    private readonly IFileSplitter _fileSplitter;

    public InvoiceSplitter(string inputFileName)
    {
        _inputFileName = inputFileName;

        var fileExtension = Path.GetExtension(inputFileName);
        _fileSplitter = string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase) ? new PdfSplitter(_inputFileName) : new ImageSplitter(_inputFileName);
    }

    public void Split()
    {
        _fileSplitter.SplitFile();
    }

    private static string ExtractTextFromImage(string imagePath)
    {
        var tessDataPath = @"C:\repos\_PRIVATE\audit-helper\invoices\tessdata";
        using var engine = new TesseractEngine(tessDataPath, "pol", EngineMode.Default);

        using var img = Pix.LoadFromFile(imagePath);
        using var page = engine.Process(img);

        return page.GetText();
    }
}