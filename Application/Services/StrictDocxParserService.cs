using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UniStart.Application.DTOs;

namespace UniStart.Application.Services;

/// <summary>
/// Deterministic (no-LLM) parser for questions authored in the fixed "strict template"
/// .docx layout: one table per question with rows for the Q-number/difficulty, question
/// text, four A–D options (✓ marks the correct one), and an EXPLANATION row.
/// The exam/section/topic are chosen in the UI, so the parser only extracts questions.
/// </summary>
public interface IStrictDocxParserService
{
    List<ExtractedQuestion> Parse(Stream docxStream);
}

public class StrictDocxParserService : IStrictDocxParserService
{
    // A question starts with "Q001", "Q1", etc. at the beginning of a line.
    private static readonly Regex QStart = new(@"^Q0*\d+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DiffRx = new(@"Difficulty:\s*(Easy|Medium|Hard)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    // Option lines look like "A) text", "B. text" (letters A–D).
    private static readonly Regex OptRx = new(@"^([A-Da-d])[\)\.]\s+(.+)$", RegexOptions.Compiled);

    private const char Check1 = '\u2713'; // ✓
    private const char Check2 = '\u2714'; // ✔

    public List<ExtractedQuestion> Parse(Stream docxStream)
    {
        var lines = FlattenToLines(docxStream);
        var result = new List<ExtractedQuestion>();

        Builder? cur = null;
        var started = false;

        void Finalize()
        {
            if (cur == null) return;
            var text = cur.Text.ToString().Trim();
            if (text.Length > 0 && cur.Options.Count > 0)
            {
                var expl = cur.Explanation.ToString().Trim();
                result.Add(new ExtractedQuestion(
                    text,
                    cur.Options,
                    expl.Length > 0 ? expl : null,
                    null,
                    cur.Difficulty));
            }
            cur = null;
        }

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            // Skip decorative group headers like "🟢 EASY QUESTIONS (Q001–Q025)".
            if (line.Contains("QUESTIONS", StringComparison.Ordinal) && !QStart.IsMatch(line)) continue;

            if (QStart.IsMatch(line))
            {
                Finalize();
                started = true;
                cur = new Builder();
                var dm = DiffRx.Match(line);
                cur.Difficulty = dm.Success ? Capitalize(dm.Groups[1].Value) : "Medium";
                cur.Stage = Stage.Text;
                continue;
            }

            // Ignore the file header (title / "Practice Questions — N Total" / "Easy: X ...")
            // that appears before the first question.
            if (!started || cur == null) continue;

            var om = OptRx.Match(line);
            if (om.Success)
            {
                var optText = om.Groups[2].Value;
                var correct = optText.IndexOf(Check1) >= 0 || optText.IndexOf(Check2) >= 0;
                optText = optText.Replace(Check1.ToString(), "").Replace(Check2.ToString(), "").Trim();
                cur.Options.Add(new DraftOptionDto(optText, correct));
                cur.Stage = Stage.Options;
                continue;
            }

            if (line.StartsWith("EXPLANATION", StringComparison.OrdinalIgnoreCase))
            {
                var expl = line.Substring("EXPLANATION".Length).TrimStart('\t', ' ', ':', '-', '\u2014').Trim();
                if (expl.Length > 0) cur.Explanation.Append(expl);
                cur.Stage = Stage.Explanation;
                continue;
            }

            // Continuation lines (multi-line question text or explanation).
            if (cur.Stage == Stage.Text)
            {
                if (cur.Text.Length > 0) cur.Text.Append(' ');
                cur.Text.Append(line);
            }
            else if (cur.Stage == Stage.Explanation)
            {
                if (cur.Explanation.Length > 0) cur.Explanation.Append(' ');
                cur.Explanation.Append(line);
            }
            // Non-option lines during the Options stage are ignored.
        }
        Finalize();
        return result;
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1).ToLowerInvariant();

    /// <summary>
    /// Flattens the document to lines in reading order. Paragraphs become one line each;
    /// each table row becomes one tab-joined line of its cells. OpenXml's InnerText already
    /// concatenates the many <w:r> runs Word splits text into.
    /// </summary>
    private static List<string> FlattenToLines(Stream stream)
    {
        var lines = new List<string>();
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return lines;

        foreach (var el in body.ChildElements)
        {
            switch (el)
            {
                case Paragraph p:
                    lines.Add(p.InnerText);
                    break;
                case Table t:
                    foreach (var row in t.Elements<TableRow>())
                    {
                        var cells = row.Elements<TableCell>().Select(c => c.InnerText.Trim()).ToList();
                        lines.Add(string.Join("\t", cells));
                    }
                    break;
            }
        }
        return lines;
    }

    private enum Stage { Text, Options, Explanation }

    private sealed class Builder
    {
        public readonly StringBuilder Text = new();
        public readonly List<DraftOptionDto> Options = new();
        public readonly StringBuilder Explanation = new();
        public string Difficulty = "Medium";
        public Stage Stage = Stage.Text;
    }
}
