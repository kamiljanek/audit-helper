namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        string inputFileName = @"C:\repos\_PRIVATE\audit-helper\invoices\invoice_scan.pdf";

        var splitter = new InvoiceSplitter(inputFileName);
        splitter.Split();

       Console.WriteLine("Finnish.");
    }
}

