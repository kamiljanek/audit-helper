using Tesseract;

namespace audit_helper;

public class ImageInvoiceReader : IInvoiceReader
{
    public ImageInvoiceReader()
    {

    }

    public void SplitFile(TesseractEngine engine, string inputFileName)
    {
        var pageText = ImageReader.GetText(engine, inputFileName);

        var invoiceName = pageText.GetInvoiceName();

        try
        {
            var newFileName = DirectoryHelper.GenerateNewFileName(inputFileName, invoiceName, 1);
            File.Copy(inputFileName, $"{newFileName}", overwrite: false);

            Console.WriteLine($"Saved: {newFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fail: {ex.Message}");
        }
    }
}