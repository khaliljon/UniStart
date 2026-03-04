using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

public class FileParserService : IFileParserService
{
    private readonly ILogger<FileParserService> _logger;

    public FileParserService(ILogger<FileParserService> logger)
    {
        _logger = logger;
    }

    public string ParsePdf(Stream fileStream)
    {
        var sb = new StringBuilder();
        using var document = PdfDocument.Open(fileStream);
        foreach (var page in document.GetPages())
        {
            var text = page.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
                sb.AppendLine(); // Blank line between pages
            }
        }
        _logger.LogInformation("Parsed PDF: {PageCount} pages, {CharCount} characters", document.NumberOfPages, sb.Length);
        return sb.ToString();
    }

    public string ParseDocx(Stream fileStream)
    {
        var sb = new StringBuilder();
        using var doc = WordprocessingDocument.Open(fileStream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body != null)
        {
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var text = paragraph.InnerText;
                // Always append line (even empty) to preserve paragraph structure
                // This ensures blank lines between sections are kept, which is
                // critical for question block detection in the extractor
                sb.AppendLine(text);
            }
        }
        _logger.LogInformation("Parsed DOCX: {CharCount} characters", sb.Length);
        return sb.ToString();
    }

    public List<Dictionary<string, string>> ParseExcel(Stream fileStream)
    {
        var rows = new List<Dictionary<string, string>>();
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.First();
        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
            return rows;

        var headerRow = usedRange.FirstRow();
        var headers = new List<string>();
        foreach (var cell in headerRow.CellsUsed())
        {
            headers.Add(cell.GetString().Trim());
        }

        foreach (var row in usedRange.RowsUsed().Skip(1)) // Skip header
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = row.Cell(i + 1);
                dict[headers[i]] = cell.GetString().Trim();
            }

            // Skip completely empty rows
            if (dict.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                rows.Add(dict);
            }
        }

        _logger.LogInformation("Parsed Excel: {RowCount} data rows, {ColCount} columns", rows.Count, headers.Count);
        return rows;
    }
}
