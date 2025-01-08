using System.Diagnostics;

namespace audit_helper;

class Program
{
    static async Task Main(string[] args)
    {
        var sw = new Stopwatch();
        // string directoryPath = @"C:\repos\_PRIVATE\audit-helper\invoices\to work";
        Console.WriteLine("Write path to folder with files to separate:");
        string directoryPath = Console.ReadLine()!;
        sw.Start();

        var fileNames = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly).ToList();

        var tasks = new List<Task>();

        foreach (var fileName in fileNames)
        {
            var splitter = new InvoiceSplitter();
            tasks.Add(Task.Run(() => splitter.Split(fileName)));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.ReadKey();
            throw;
        }

        Console.WriteLine("Finnish.");
        Console.WriteLine(sw.ElapsedMilliseconds);
        Console.ReadKey();
    }
}

