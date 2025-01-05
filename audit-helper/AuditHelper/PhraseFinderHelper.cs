using System.Text.RegularExpressions;

namespace audit_helper;

public static class PhraseFinderHelper
{
    public static bool IsContainInvoiceHeader(this string pageText)
    {
        string[] invoiceKeywords = { "Faktura VAT", "Data wystawienia", "Invoice Date", "Invoice No", "Rechnung" };
        // UNDONE: check page number (e.g. site 1/3)
        foreach (var keyword in invoiceKeywords)
        {
            if (pageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static string GetInvoiceName(this string pageText)
    {
        var invoiceName = "unknown";
        var patterns = new string[]
        {
            @"(?:(?:Faktura VAT nr|Faktura VAT|Faktura sprzedaży|Faktura korygująca|Faktura pro forma|Faktura zaliczkowa|Faktura końcowa|Faktura uproszczona|Faktura VAT marża|Dokument sprzedaży|Dokument księgowy|Korekta do faktury|Faktura numer|Faktura|FV|Nr faktury|Numer faktury|Faktura nr)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)",
            @"(?:(?:Invoice No|Invoice number|Invoice ref|Invoice reference|VAT Invoice|Sales Invoice|Credit Invoice|Pro forma Invoice|Advance Invoice|Final Invoice|Simplified Invoice|VAT Margin Invoice|Sales Document|Accounting Document|Invoice correction|Bill No|Invoice|Tax Invoice|Ref No)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)",
            @"(?:(?:Rechnungsnummer|Rechnung Nr|Rechnungs-Nr|Rechnung|MwSt. Rechnung|Gutschrift|Proforma Rechnung|Vorausrechnung|Endrechnung|Umsatzsteuer Rechnung|Verkaufsdokument|Buchungsbeleg|Korrektur Rechnung|Rechnungsreferenz|Rechnungs-Nr)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)",
            @"(?:(?:Nr|No)\s*[:/]*\s*)([A-Za-z0-9/.-]*\d{2,}[A-Za-z0-9/.-]*)"
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
}