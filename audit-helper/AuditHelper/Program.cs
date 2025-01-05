using System.Text.RegularExpressions;

namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"C:\repos\_PRIVATE\audit-helper\invoices\to work";

        var fileNames = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly).ToList();

        foreach (var fileName in fileNames)
        {
            var splitter = new InvoiceSplitter(fileName);
            splitter.Split();
        }

        Console.WriteLine("Finnish.");
    }
}

