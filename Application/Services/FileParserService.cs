using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Docnet.Core;
using Docnet.Core.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

public class FileParserService : IFileParserService
{
    private readonly ILogger<FileParserService> _logger;
    private static readonly string TessDataPath;

    static FileParserService()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "tessdata"),
            Path.Combine(Directory.GetCurrentDirectory(), "tessdata"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
        };
        TessDataPath = candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
    }

    public FileParserService(ILogger<FileParserService> logger)
    {
        _logger = logger;
    }

    public string ParsePdf(Stream fileStream)
    {
        using var memStream = new MemoryStream();
        fileStream.CopyTo(memStream);
        var bytes = memStream.ToArray();

        var result = ParsePdfWithPdfPig(bytes);
        if (!string.IsNullOrWhiteSpace(result))
            return result;

        _logger.LogInformation("PdfPig returned no text, trying PDFium (Docnet) fallback");
        result = ParsePdfWithDocnet(bytes);
        if (!string.IsNullOrWhiteSpace(result))
            return result;

        _logger.LogInformation("Both text extractors failed, trying OCR (Tesseract) fallback for image-based PDF");
        result = ParsePdfWithOcr(bytes);
        if (!string.IsNullOrWhiteSpace(result))
        {
            result = CleanOcrText(result);
            return result;
        }

        _logger.LogWarning("All PDF extraction methods failed (PdfPig, PDFium, OCR). PDF content cannot be read.");
        return "";
    }

    private string ParsePdfWithPdfPig(byte[] bytes)
    {
        var sb = new StringBuilder();
        using var stream = new MemoryStream(bytes);
        using var document = PdfDocument.Open(stream);
        foreach (var page in document.GetPages())
        {
            var extractedText = ExtractTextWithSpatialOrder(page);
            if (!string.IsNullOrWhiteSpace(extractedText))
            {
                sb.AppendLine(extractedText);
                sb.AppendLine();
            }
            else
            {
                var text = page.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
        }
        _logger.LogInformation("PdfPig: {PageCount} pages, {CharCount} characters", document.NumberOfPages, sb.Length);
        return sb.ToString();
    }

    private string ParsePdfWithDocnet(byte[] bytes)
    {
        try
        {
            var sb = new StringBuilder();
            using var docReader = DocLib.Instance.GetDocReader(bytes, new PageDimensions(1080, 1920));
            var pageCount = docReader.GetPageCount();

            for (int i = 0; i < pageCount; i++)
            {
                using var pageReader = docReader.GetPageReader(i);
                var text = pageReader.GetText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }

            _logger.LogInformation("PDFium: {PageCount} pages, {CharCount} characters", pageCount, sb.Length);
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PDFium extraction failed, PDF may be corrupted");
            return "";
        }
    }

    private string ParsePdfWithOcr(byte[] bytes)
    {
        try
        {
            var engPath = Path.Combine(TessDataPath, "eng.traineddata");
            var chiPath = Path.Combine(TessDataPath, "chi_sim.traineddata");
            if (!File.Exists(engPath))
            {
                _logger.LogWarning("Tesseract data not found at {Path}. OCR skipped.", TessDataPath);
                return "";
            }

            var lang = File.Exists(chiPath) ? "chi_sim+eng" : "eng";
            _logger.LogInformation("Starting OCR with language={Lang}, tessdata={Path}", lang, TessDataPath);

            var sb = new StringBuilder();
            using var docReader = DocLib.Instance.GetDocReader(bytes, new PageDimensions(2160, 3840));
            var pageCount = docReader.GetPageCount();

            using var engine = new TesseractEngine(TessDataPath, lang, EngineMode.Default);

            for (int i = 0; i < pageCount; i++)
            {
                try
                {
                    using var pageReader = docReader.GetPageReader(i);
                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();
                    var rawBytes = pageReader.GetImage();

                    if (rawBytes == null || rawBytes.Length == 0 || width <= 0 || height <= 0)
                        continue;

                    using var pix = ConvertBgraToPixForOcr(rawBytes, width, height);
                    if (pix == null) continue;

                    using var page = engine.Process(pix);
                    var text = page.GetText();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text.Trim());
                        sb.AppendLine();
                    }

                    if ((i + 1) % 5 == 0 || i == pageCount - 1)
                        _logger.LogInformation("OCR progress: {Current}/{Total} pages", i + 1, pageCount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OCR failed on page {PageNumber}", i + 1);
                }
            }

            _logger.LogInformation("OCR: {PageCount} pages, {CharCount} characters", pageCount, sb.Length);
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OCR initialization failed");
            return "";
        }
    }

    private static Pix? ConvertBgraToPixForOcr(byte[] bgraData, int width, int height)
    {
        try
        {
            int rowStride = ((width * 3) + 3) & ~3;
            int pixelDataSize = rowStride * height;
            int fileSize = 14 + 40 + pixelDataSize;

            var bmp = new byte[fileSize];
            using var ms = new MemoryStream(bmp);
            using var bw = new BinaryWriter(ms);

            bw.Write((ushort)0x4D42);
            bw.Write(fileSize);
            bw.Write(0);
            bw.Write(14 + 40);

            bw.Write(40);
            bw.Write(width);
            bw.Write(height);
            bw.Write((ushort)1);
            bw.Write((ushort)24);
            bw.Write(0);
            bw.Write(pixelDataSize);
            bw.Write(2835);
            bw.Write(2835);
            bw.Write(0);
            bw.Write(0);

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcOffset = (y * width + x) * 4;
                    if (srcOffset + 2 < bgraData.Length)
                    {
                        bw.Write(bgraData[srcOffset]);
                        bw.Write(bgraData[srcOffset + 1]);
                        bw.Write(bgraData[srcOffset + 2]);
                    }
                    else
                    {
                        bw.Write((byte)255);
                        bw.Write((byte)255);
                        bw.Write((byte)255);
                    }
                }
                int padding = rowStride - (width * 3);
                for (int p = 0; p < padding; p++)
                    bw.Write((byte)0);
            }
            bw.Flush();

            return Pix.LoadFromMemory(bmp);
        }
        catch
        {
            return null;
        }
    }

    private string CleanOcrText(string text)
    {
        string prev;
        do
        {
            prev = text;
            text = Regex.Replace(text,
                @"([\u4e00-\u9fff\u3000-\u303f\uff00-\uffef]) ([\u4e00-\u9fff\u3000-\u303f\uff00-\uffef])",
                "$1$2");
        } while (text != prev);

        text = Regex.Replace(text, @"([\u4e00-\u9fff]) (?=[\d\(\)\[\]{}=<>+\-*/≤≥∈∉⊂⊄⊆∪∩∅])", "$1");
        text = Regex.Replace(text, @"(?<=[\d\(\)\[\]{}=<>+\-*/≤≥∈∉⊂⊄⊆∪∩∅]) ([\u4e00-\u9fff])", " $1");

        text = Regex.Replace(text, @"[ \t]{3,}", "  ");

        text = Regex.Replace(text, @"(?m)^\s*[\x00-\x1f]\s*$", "");

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        text = Regex.Replace(text, @"\n{4,}", "\n\n\n");

        _logger.LogInformation("OCR text cleaned: {CharCount} characters after cleanup", text.Length);
        return text;
    }

    private static string ExtractTextWithSpatialOrder(UglyToad.PdfPig.Content.Page page)
    {
        var words = page.GetWords().ToList();
        if (words.Count == 0) return "";

        var sortedWords = words.OrderByDescending(w => w.BoundingBox.Top)
                               .ThenBy(w => w.BoundingBox.Left)
                               .ToList();

        var lines = new List<List<Word>>();
        var currentLine = new List<Word> { sortedWords[0] };
        var currentLineY = sortedWords[0].BoundingBox.Top;

        var avgHeight = words.Average(w => w.BoundingBox.Height);
        var lineTolerance = Math.Max(avgHeight * 0.5, 2.0);

        for (int i = 1; i < sortedWords.Count; i++)
        {
            var word = sortedWords[i];
            if (Math.Abs(word.BoundingBox.Top - currentLineY) <= lineTolerance)
            {
                currentLine.Add(word);
            }
            else
            {
                lines.Add(currentLine);
                currentLine = new List<Word> { word };
                currentLineY = word.BoundingBox.Top;
            }
        }
        lines.Add(currentLine);

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var sortedLine = line.OrderBy(w => w.BoundingBox.Left).ToList();
            for (int i = 0; i < sortedLine.Count; i++)
            {
                sb.Append(sortedLine[i].Text);

                if (i < sortedLine.Count - 1)
                {
                    var gap = sortedLine[i + 1].BoundingBox.Left - sortedLine[i].BoundingBox.Right;
                    var charWidth = sortedLine[i].BoundingBox.Width /
                        Math.Max(1, sortedLine[i].Text.Length);

                    if (gap > charWidth * 0.3)
                        sb.Append(' ');
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string ParseDocx(Stream fileStream)
    {
        var sb = new StringBuilder();
        using var doc = WordprocessingDocument.Open(fileStream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body != null)
        {
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                var text = paragraph.InnerText;
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

        foreach (var row in usedRange.RowsUsed().Skip(1))
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = row.Cell(i + 1);
                dict[headers[i]] = cell.GetString().Trim();
            }

            if (dict.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                rows.Add(dict);
            }
        }

        _logger.LogInformation("Parsed Excel: {RowCount} data rows, {ColCount} columns", rows.Count, headers.Count);
        return rows;
    }
}
