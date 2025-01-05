using Tesseract;

namespace audit_helper;

public interface IInvoiceReader
{
    public void SplitFile(TesseractEngine engine, string inputFileName);
}