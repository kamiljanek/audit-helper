using System.Text.RegularExpressions;

namespace audit_helper;

public static class PhraseFinderHelper
{
    private const string _regexInvoiceKeywordsPl = @"(?:(?:Faktura VAT nr|Faktura VAT|Faktura sprzedaży|Faktura korygująca|Faktura pro forma|Faktura zaliczkowa|Faktura końcowa|Faktura uproszczona|Faktura VAT marża|Dokument sprzedaży|Dokument księgowy|Korekta do faktury|Faktura numer|Faktura|FV|Nr faktury|Numer faktury|Faktura nr)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)";
    private const string _regexInvoiceKeywordsEn = @"(?:(?:Invoice No|Invoice number|Invoice ref|Invoice reference|VAT Invoice|Sales Invoice|Credit Invoice|Pro forma Invoice|Advance Invoice|Final Invoice|Simplified Invoice|VAT Margin Invoice|Sales Document|Accounting Document|Invoice correction|Bill No|Invoice|Tax Invoice|Ref No)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)";
    private const string _regexInvoiceKeywordsDe = @"(?:(?:Rechnungsnummer|Rechnung Nr|Rechnung De|Rechnungs-Nr|Rechnung|MwSt. Rechnung|Gutschrift|Proforma Rechnung|Vorausrechnung|Endrechnung|Umsatzsteuer Rechnung|Verkaufsdokument|Buchungsbeleg|Korrektur Rechnung|Rechnungsreferenz|Rechnungs-Nr)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)";
    private const string _regexInvoiceKeywordsCz = @"(?:(?:Číslo faktury|Faktura č|Číslo dokladu|Daňový doklad|Proforma faktura|Zálohová faktura|Konečná faktura|Zjednodušená faktura|Opravný daňový doklad|Účetní doklad|Korekce faktury|Faktura číslo|Faktura|FV|Číslo účtu|Číslo objednávky|Objednávka číslo)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)";
    private const string _regexInvoiceKeywordsDefault = @"(?:(?:Nr|No)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)";

    public static bool IsFirstPage(this string pageText)
    {
        string[] keywords = { "strona", "page", "site", "seite" };
        foreach (var keyword in keywords)
        {
            if (pageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                string keywordsPattern = @"\b(strona|page|site|seite)\b";
                string pattern = @"\b1\s*/\s*[1-5]\b|\b1\s*z\s*[1-5]\b|\b1\s*-\s*[1-5]\b";
                string combinedPattern = $@"{keywordsPattern}\s*{pattern}";

                return Regex.IsMatch(pageText, combinedPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }
        }

        return ContainsInvoiceKeyword(pageText);
    }

    public static bool IsLastPage(this string pageText)
    {
        string keywordsPattern = @"\b(strona|page|site|seite)\b";
        var pattern = @"\b([1-5])\s*/\s*\1\b|\b([1-5])\s*z\s*\1\b|\b([1-5])\s*-\s*\1\b";
        string combinedPattern = $@"{keywordsPattern}\s*{pattern}";

        bool containsKeywordAndPattern = Regex.IsMatch(pageText, combinedPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        return containsKeywordAndPattern;
    }

    public static string GetInvoiceName(this string pageText)
    {
        var invoiceName = Common.UnknownName;
        var patterns = new []
        {
            _regexInvoiceKeywordsPl,
            _regexInvoiceKeywordsEn,
            _regexInvoiceKeywordsDe,
            _regexInvoiceKeywordsCz,
            _regexInvoiceKeywordsDefault
        };

        foreach (var pattern in patterns)
        {
            MatchCollection matches = Regex.Matches(pageText, pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                invoiceName = matches[0].Groups[1].Value.ToUpper();
                Console.WriteLine($"Invoice name: {invoiceName}");
            }
        }

        return invoiceName.Replace('/', '_');
    }

    public static string GetInvoiceName(this List<string> pageTexts)
    {
        var invoiceName = Common.UnknownName;
        var patterns = new []
        {
            _regexInvoiceKeywordsPl,
            _regexInvoiceKeywordsEn,
            _regexInvoiceKeywordsDe,
            _regexInvoiceKeywordsCz
        };

        foreach (var pageText in pageTexts)
        {
            foreach (var pattern in patterns)
            {
                MatchCollection matches = Regex.Matches(pageText, pattern, RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    invoiceName = matches[0].Groups[1].Value.ToUpper();
                    Console.WriteLine($"Invoice name: {invoiceName}");
                    break;
                }
            }
        }

        return invoiceName.Replace('/', '_');
    }

    public static bool ContainsName(this string pageText, string fileName)
    {
        var rawPageText = RemoveSpecialCharacters(pageText);
        var rawFileName = RemoveSpecialCharacters(fileName);

        return rawPageText.Contains(rawFileName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsInvoiceKeyword(string pageText)
    {
        string[] invoiceKeywords = { "Faktura VAT", "Data wystawienia", "Invoice Date", "Invoice No", "Rechnung" };
        foreach (var keyword in invoiceKeywords)
        {
            if (pageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string RemoveSpecialCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        string pattern = "[^a-zA-Z0-9]";
        string result = Regex.Replace(text, pattern, "");

        return result;
    }
}