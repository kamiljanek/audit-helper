using Tesseract;

namespace audit_helper;

public class ImageReader
{
    public static string GetText(TesseractEngine engine, string fileName)
    {
        using var img = Pix.LoadFromFile(fileName);
        using var page = engine.Process(img);

        return page.GetText();
    }
}