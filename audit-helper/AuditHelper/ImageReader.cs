using Tesseract;

namespace audit_helper;

public class ImageReader
{
    public static string GetText(string fileName)
    {
        var tessDataPath = @"C:\repos\_PRIVATE\audit-helper\invoices\tessdata";
        using var engine = new TesseractEngine(tessDataPath, "pol", EngineMode.Default);

        using var img = Pix.LoadFromFile(fileName);
        using var page = engine.Process(img);

        return page.GetText();
    }
}