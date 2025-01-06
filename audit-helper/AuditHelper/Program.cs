namespace audit_helper;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"C:\repos\_PRIVATE\audit-helper\invoices\to work";

        var fileNames = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly).ToList();
        var splitter = new InvoiceSplitter();

        foreach (var fileName in fileNames)
        {
            splitter.Split(fileName);
        }

        Console.WriteLine("Finnish.");
    }
}

