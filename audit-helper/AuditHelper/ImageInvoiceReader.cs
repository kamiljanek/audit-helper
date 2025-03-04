﻿namespace audit_helper;

public class ImageInvoiceReader : IInvoiceReader
{
    private readonly string _inputFileName;
    private readonly ImageReader _imageReader;

    public ImageInvoiceReader(string inputFileName)
    {
        _inputFileName = inputFileName;
        _imageReader = new ImageReader();
    }

    public void SplitFile()
    {
        var pageText = _imageReader.GetTextDefault(_inputFileName);

        var fileName = Path.GetFileNameWithoutExtension(_inputFileName);
        var invoiceName = pageText.ContainsName(fileName) ? fileName : pageText.GetInvoiceName();

        try
        {
            var newFileName = DirectoryHelper.GenerateNewFileName(_inputFileName, invoiceName, 1);
            File.Copy(_inputFileName, $"{newFileName}", overwrite: false);

            Console.WriteLine($"Saved: {newFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fail: {ex.Message}");
        }
    }
}