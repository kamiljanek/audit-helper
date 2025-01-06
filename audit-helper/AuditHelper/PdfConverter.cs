using PDFtoImage;

namespace audit_helper;

public class PdfConverter
{
    private int _fileCounter = 0;
    public PdfConverter()
    {
    }

    public string PdfToJpeg(string inputFileName, int pageNumber)
    {
        var outputFilePath = Path.Combine(Path.GetDirectoryName(inputFileName)!, Common.ConvertedDirectoryName);
        Directory.CreateDirectory(outputFilePath);

        var fileName = Path.GetFileNameWithoutExtension(inputFileName);
        var changedFileName = Path.Combine(outputFilePath, $"{fileName}-{_fileCounter}");
        var newFileName = Path.ChangeExtension(changedFileName, ".jpeg");

        // UNDONE: use GenerateNewFileName

        using FileStream pdfStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
        using FileStream imageStream = new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.Write);

        Conversion.SaveJpeg(imageStream, pdfStream, new Index(pageNumber - 1));
        _fileCounter += 1;

        return newFileName;
    }
}