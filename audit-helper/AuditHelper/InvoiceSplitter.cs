namespace audit_helper;

public class InvoiceSplitter
{
    public InvoiceSplitter()
    {
    }

    public void Split(string inputFileName)
    {
        var fileExtension = Path.GetExtension(inputFileName);

        IInvoiceReader invoiceReader = string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase) ?
            new PdfInvoiceReader(inputFileName) : new ImageInvoiceReader(inputFileName);

        invoiceReader.SplitFile();
    }
}