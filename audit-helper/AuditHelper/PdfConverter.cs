using PDFtoImage;

namespace audit_helper;

public class PdfConverter
{
    public PdfConverter()
    {
    }

    public static string PdfToJpeg(string inputFileName, int pageNumber)
    {
        var outputFilePath = @$"{inputFileName}\converted_images";
        Directory.CreateDirectory(outputFilePath);

        var outputFileName = Path.ChangeExtension(outputFilePath, ".jpeg");

        using FileStream pdfStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
        using FileStream imageStream = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite);

        Conversion.SaveJpeg(imageStream, pdfStream, new Index(pageNumber - 1));

        return outputFileName;
    }
}