using System.Diagnostics;

namespace audit_helper;

class Program
{
    static async Task Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();
        string directoryPath = @"C:\repos\_PRIVATE\audit-helper\invoices\to work";

        var fileNames = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly).ToList();

        var tasks = new List<Task>();

        foreach (var fileName in fileNames)
        {
            var splitter = new InvoiceSplitter();
            tasks.Add(Task.Run(() => splitter.Split(fileName)));
        }

        await Task.WhenAll(tasks);

        Console.WriteLine("Finnish.");
        Console.WriteLine(sw.ElapsedMilliseconds);
    }
}

