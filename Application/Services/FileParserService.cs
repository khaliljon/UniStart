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
        // Look for tessdata in several locations
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
        // Read stream into byte array so all strategies can use it
        using var memStream = new MemoryStream();
        fileStream.CopyTo(memStream);
        var bytes = memStream.ToArray();

        // Strategy 1: PdfPig with spatial word extraction
        var result = ParsePdfWithPdfPig(bytes);
        if (!string.IsNullOrWhiteSpace(result))
            return result;

        // Strategy 2: Docnet/PDFium — handles CJK encodings that PdfPig can't decode
        _logger.LogInformation("PdfPig returned no text, trying PDFium (Docnet) fallback");
        result = ParsePdfWithDocnet(bytes);
        if (!string.IsNullOrWhiteSpace(result))
            return result;

        // Strategy 3: OCR — render PDF pages to images, then Tesseract OCR
        _logger.LogInformation("Both text extractors failed, trying OCR (Tesseract) fallback for image-based PDF");
        result = ParsePdfWithOcr(bytes);
        if (!string.IsNullOrWhiteSpace(result))
        {
            // Post-process OCR text to fix common CJK artifacts
            result = CleanOcrText(result);
            return result;
        }

        _logger.LogWarning("All PDF extraction methods failed (PdfPig, PDFium, OCR). PDF content cannot be read.");
        return "";
    }

    /// <summary>
    /// Extract text using PdfPig with spatial word ordering.
    /// Works well for standard Latin/Cyrillic PDFs and some CJK PDFs with proper ToUnicode mappings.
    /// </summary>
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

    /// <summary>
    /// Extract text using Docnet.Core (Google PDFium engine).
    /// PDFium is Chrome's PDF renderer and handles virtually all CJK font encodings correctly.
    /// </summary>
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
                    sb.AppendLine(); // Blank line between pages
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

    /// <summary>
    /// OCR fallback: render each PDF page to an image using PDFium, then run Tesseract OCR.
    /// Handles scanned/image-based PDFs where no text layer exists.
    /// Supports Chinese (chi_sim) + English (eng) simultaneously.
    /// </summary>
    private string ParsePdfWithOcr(byte[] bytes)
    {
        try
        {
            // Check if tessdata exists
            var engPath = Path.Combine(TessDataPath, "eng.traineddata");
            var chiPath = Path.Combine(TessDataPath, "chi_sim.traineddata");
            if (!File.Exists(engPath))
            {
                _logger.LogWarning("Tesseract data not found at {Path}. OCR skipped.", TessDataPath);
                return "";
            }

            // Determine language: use chi_sim+eng if Chinese data available, else eng only
            var lang = File.Exists(chiPath) ? "chi_sim+eng" : "eng";
            _logger.LogInformation("Starting OCR with language={Lang}, tessdata={Path}", lang, TessDataPath);

            var sb = new StringBuilder();
            // Use higher resolution for better OCR quality
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

                    // Docnet returns BGRA raw pixel data
                    // Convert BGRA to Pix for Tesseract using Leptonica's pixCreate
                    using var pix = ConvertBgraToPixForOcr(rawBytes, width, height);
                    if (pix == null) continue;

                    using var page = engine.Process(pix);
                    var text = page.GetText();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text.Trim());
                        sb.AppendLine(); // Blank line between pages
                    }

                    // Log progress for long documents
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

    /// <summary>
    /// Convert BGRA raw pixel data from Docnet to a Tesseract-compatible Pix object.
    /// Creates a BMP in memory and loads it through Tesseract's Pix.LoadFromMemory.
    /// </summary>
    private static Pix? ConvertBgraToPixForOcr(byte[] bgraData, int width, int height)
    {
        try
        {
            // Build a BMP file in memory from BGRA data
            // BMP format: BITMAPFILEHEADER (14) + BITMAPINFOHEADER (40) + pixel data (BGR, 24bpp, padded to 4-byte rows)
            int rowStride = ((width * 3) + 3) & ~3; // 24bpp rows padded to 4-byte boundary
            int pixelDataSize = rowStride * height;
            int fileSize = 14 + 40 + pixelDataSize;

            var bmp = new byte[fileSize];
            using var ms = new MemoryStream(bmp);
            using var bw = new BinaryWriter(ms);

            // BITMAPFILEHEADER
            bw.Write((ushort)0x4D42); // 'BM'
            bw.Write(fileSize);       // File size
            bw.Write(0);              // Reserved
            bw.Write(14 + 40);       // Pixel data offset

            // BITMAPINFOHEADER
            bw.Write(40);            // Header size
            bw.Write(width);
            bw.Write(height);        // Positive = bottom-up
            bw.Write((ushort)1);     // Planes
            bw.Write((ushort)24);    // Bits per pixel
            bw.Write(0);             // Compression (BI_RGB)
            bw.Write(pixelDataSize);
            bw.Write(2835);          // X pixels per meter (~72 DPI)
            bw.Write(2835);          // Y pixels per meter
            bw.Write(0);             // Colors used
            bw.Write(0);             // Important colors

            // Pixel data: BMP is bottom-up, Docnet is top-down
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcOffset = (y * width + x) * 4; // BGRA
                    if (srcOffset + 2 < bgraData.Length)
                    {
                        bw.Write(bgraData[srcOffset]);     // B
                        bw.Write(bgraData[srcOffset + 1]); // G
                        bw.Write(bgraData[srcOffset + 2]); // R
                    }
                    else
                    {
                        bw.Write((byte)255);
                        bw.Write((byte)255);
                        bw.Write((byte)255);
                    }
                }
                // Pad row to 4-byte boundary
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

    /// <summary>
    /// Clean OCR output text to improve downstream question extraction quality.
    /// Fixes: CJK inter-character spaces, garbled symbols, excessive whitespace.
    /// </summary>
    private string CleanOcrText(string text)
    {
        // 1. Remove single spaces between CJK characters (OCR inserts spaces between every char)
        //    Example: "集 合 中 的 元 素" → "集合中的元素"
        //    Also handle CJK punctuation: ，。、；：？！（）
        string prev;
        do
        {
            prev = text;
            text = Regex.Replace(text,
                @"([\u4e00-\u9fff\u3000-\u303f\uff00-\uffef]) ([\u4e00-\u9fff\u3000-\u303f\uff00-\uffef])",
                "$1$2");
        } while (text != prev);

        // 2. Remove spaces between CJK and common symbols that should be adjacent
        //    e.g., "集合 4" → "集合4" (in Chinese math, numbers follow CJK directly)
        text = Regex.Replace(text, @"([\u4e00-\u9fff]) (?=[\d\(\)\[\]{}=<>+\-*/≤≥∈∉⊂⊄⊆∪∩∅])", "$1");
        text = Regex.Replace(text, @"(?<=[\d\(\)\[\]{}=<>+\-*/≤≥∈∉⊂⊄⊆∪∩∅]) ([\u4e00-\u9fff])", " $1");

        // 3. Fix common OCR misreadings in Chinese math context
        //    "4" misread as "A" in "集合A" context — can't reliably fix
        //    But we can normalize some patterns

        // 4. Collapse multiple spaces into one
        text = Regex.Replace(text, @"[ \t]{3,}", "  ");

        // 5. Remove orphan characters that are clearly OCR noise (single char on a line)
        text = Regex.Replace(text, @"(?m)^\s*[\x00-\x1f]\s*$", "");

        // 6. Normalize line endings
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // 7. Collapse 3+ blank lines into 2
        text = Regex.Replace(text, @"\n{4,}", "\n\n\n");

        _logger.LogInformation("OCR text cleaned: {CharCount} characters after cleanup", text.Length);
        return text;
    }

    /// <summary>
    /// Extract text from a PDF page using word positions for correct reading order.
    /// Groups words into lines by Y-coordinate proximity, then sorts left-to-right.
    /// This produces much better results for CJK content and math formulas.
    /// </summary>
    private static string ExtractTextWithSpatialOrder(UglyToad.PdfPig.Content.Page page)
    {
        var words = page.GetWords().ToList();
        if (words.Count == 0) return "";

        // Group words into lines based on Y-coordinate (top of bounding box).
        // PDF Y axis goes bottom-to-top, so higher Y = higher on page.
        // We process from top (highest Y) to bottom (lowest Y).
        var sortedWords = words.OrderByDescending(w => w.BoundingBox.Top)
                               .ThenBy(w => w.BoundingBox.Left)
                               .ToList();

        var lines = new List<List<Word>>();
        var currentLine = new List<Word> { sortedWords[0] };
        var currentLineY = sortedWords[0].BoundingBox.Top;

        // Tolerance for grouping words into the same line (in PDF points)
        // Average character height helps determine appropriate tolerance
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

        // Build text: sort each line left-to-right, join with smart spacing
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var sortedLine = line.OrderBy(w => w.BoundingBox.Left).ToList();
            for (int i = 0; i < sortedLine.Count; i++)
            {
                sb.Append(sortedLine[i].Text);

                if (i < sortedLine.Count - 1)
                {
                    // Calculate gap between this word and next
                    var gap = sortedLine[i + 1].BoundingBox.Left - sortedLine[i].BoundingBox.Right;
                    var charWidth = sortedLine[i].BoundingBox.Width /
                        Math.Max(1, sortedLine[i].Text.Length);

                    // Add space if gap is significant (> 30% of average char width)
                    // For CJK text, characters are usually close together without spaces
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
            // Use Descendants (not direct Elements) so that paragraphs nested inside
            // tables, text boxes and content controls are also captured. Many templates
            // lay their questions out in a table — reading only top-level paragraphs
            // would drop almost all of the content.
            foreach (var paragraph in body.Descendants<Paragraph>())
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
