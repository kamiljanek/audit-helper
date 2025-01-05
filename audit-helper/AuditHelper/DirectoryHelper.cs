using System.Text.RegularExpressions;

namespace audit_helper;

public class DirectoryHelper
{
    public static string GenerateNewFileName(string originalFileName, string newFileName, int numberOfPages)
    {
        var prefix = $"({numberOfPages})-";
        var fileExtension = Path.GetExtension(originalFileName);
        var outputFilePath = Path.Combine(Path.GetDirectoryName(originalFileName)!, Common.SeparatedDirectoryName);
        Directory.CreateDirectory(outputFilePath);
        var outputFileName = Path.Combine(outputFilePath, $"{prefix}{newFileName}{fileExtension}");

        EnsureUniqueName(outputFileName);

        return outputFileName;
    }

    private static void EnsureUniqueName(string inputFileName)
    {
        if (File.Exists(inputFileName))
        {
            UpdateName(ref inputFileName);
        }
    }

    private static void UpdateName(ref string inputFileName)
    {
        var directoryPath = Path.GetDirectoryName(inputFileName);
        var fileName = Path.GetFileNameWithoutExtension(inputFileName);
        var fileExtension = Path.GetExtension(inputFileName);

        string pattern = @"^\(\d+\)-|(-copy\(\d+\)$)";
        string searchPhrase = Regex.Replace(fileName, pattern, "");

        var fileNames = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
            .Where(f => Path.GetFileName(f).Contains(searchPhrase, StringComparison.OrdinalIgnoreCase))
            .ToList();

        string numberPattern = @"\((\d+)\)$";

        var maxFoundedNumber = fileNames
            .Select(name => Regex.Match(name, numberPattern).Success ? int.Parse(Regex.Match(name, pattern).Groups[1].Value) : 0)
            .Max();

        string patternForNewFile = @"-copy\(\d+\)$";

        string newFileName = Regex.Replace(fileName, patternForNewFile, "");

        inputFileName = Path.Combine(directoryPath, $"{newFileName}-copy({maxFoundedNumber + 1}){fileExtension}");
    }
}