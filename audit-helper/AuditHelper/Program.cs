using System.Text.RegularExpressions;

namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        string inputFileName = @"C:\repos\_PRIVATE\audit-helper\invoices\invoice_de.pdf";

        // UNDONE: add here reading all from directory
        var splitter = new InvoiceSplitter(inputFileName);
        splitter.Split();

        Console.WriteLine("Finnish.");
    }
}

