using Tesseract;

namespace audit_helper;

public class InvoiceSplitter
{
    private readonly string _inputFileName;
    private readonly IInvoiceReader _invoiceReader;

    public InvoiceSplitter(string inputFileName)
    {
        _inputFileName = inputFileName;
        var fileExtension = Path.GetExtension(inputFileName);
        _invoiceReader = string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase) ? new PdfInvoiceReader() : new ImageInvoiceReader();
    }

    public void Split()
    {
        var tessDataPath = @"C:\repos\_PRIVATE\audit-helper\invoices\tessdata";
        using var engine = new TesseractEngine(tessDataPath, "pol", EngineMode.Default);
        _invoiceReader.SplitFile(engine, _inputFileName);
    }
}