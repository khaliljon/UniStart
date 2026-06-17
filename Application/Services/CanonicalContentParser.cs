using System.Text;
using System.Text.RegularExpressions;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

/// <summary>
/// Deterministic parser for the canonical study-pack Markdown format. No LLM, no tokens,
/// fully reproducible — the fast path for importing the already-structured 6000-question bank.
///
/// CANONICAL FORMAT
/// ----------------
/// <code>
/// # Skill Name                     (first H1 = the skill; a "Skill:" prefix is optional)
///
/// ## Topic Name                    (each H2 = ONE concept/topic; "Topic:" prefix optional)
///
/// ### Lesson                       (optional) markdown theory until the next ### / ## / #
/// Speed is distance over time...
///
/// ### Formulas                     (optional) one per line, pipe-separated:
/// - Speed | v = \frac{d}{t} | distance over time
///
/// ### Questions
/// 1. [Hard] What is the speed of a car covering 100 km in 2 h?
/// - 25 km/h
/// * 50 km/h                        (the correct option is marked with * or +, or - [x])
/// - 100 km/h
/// - 200 km/h
/// > 100 / 2 = 50 km/h.             (explanation, optional, starts with >)
/// ~ Distance divided by time.      (hint, optional, starts with ~ or "Hint:")
/// </code>
///
/// Tolerances: a "### Questions" header is optional (numbered lines are auto-detected); the
/// [difficulty] tag is optional (defaults to Medium); options accept -, *, +, or "- [x]/[ ]".
/// </summary>
public class CanonicalContentParser : ICanonicalContentParser
{
    private static readonly Regex H1 = new(@"^#\s+(.+?)\s*$", RegexOptions.Compiled);
    private static readonly Regex H2 = new(@"^##\s+(.+?)\s*$", RegexOptions.Compiled);
    private static readonly Regex H3 = new(@"^###\s+(.+?)\s*$", RegexOptions.Compiled);
    private static readonly Regex QuestionStart = new(@"^\s*\d+[.)]\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex DifficultyTag = new(@"^\[(easy|medium|hard)\]\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OptionLine = new(@"^\s*([-*+])\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex Checkbox = new(@"^\[(x| )\]\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public IngestContentDto Parse(string text, string examTypeCode, string examSectionName)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FormatException("Empty document.");

        var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").TrimStart('\uFEFF').Split('\n');

        string? skillName = null;
        var topics = new List<IngestTopicDto>();

        // Split the document into the skill title and H2 topic blocks.
        var topicBlocks = new List<(string name, List<string> body)>();
        List<string>? currentBody = null;
        foreach (var raw in lines)
        {
            var h1 = H1.Match(raw);
            if (h1.Success && !H2.IsMatch(raw) && !H3.IsMatch(raw))
            {
                skillName ??= StripPrefix(h1.Groups[1].Value, "skill");
                continue;
            }

            var h2 = H2.Match(raw);
            if (h2.Success && !H3.IsMatch(raw))
            {
                currentBody = new List<string>();
                topicBlocks.Add((StripPrefix(h2.Groups[1].Value, "topic"), currentBody));
                continue;
            }

            currentBody?.Add(raw);
        }

        if (string.IsNullOrWhiteSpace(skillName))
            throw new FormatException("No skill title found. The document must start with '# Skill Name'.");
        if (topicBlocks.Count == 0)
            throw new FormatException("No topics found. Each concept must be an H2 heading: '## Topic Name'.");

        for (var i = 0; i < topicBlocks.Count; i++)
        {
            var (name, body) = topicBlocks[i];
            topics.Add(ParseTopic(name, body, i + 1));
        }

        return new IngestContentDto(examTypeCode, examSectionName, skillName!.Trim(), topics);
    }

    private static IngestTopicDto ParseTopic(string name, List<string> body, int sortOrder)
    {
        // Split a topic body into H3 sections; lines before the first ### are an implicit lesson.
        var sections = new List<(string label, List<string> content)>();
        var lead = new List<string>();
        List<string>? cur = null;
        foreach (var raw in body)
        {
            var h3 = H3.Match(raw);
            if (h3.Success)
            {
                cur = new List<string>();
                sections.Add((h3.Groups[1].Value.Trim(), cur));
                continue;
            }
            (cur ?? lead).Add(raw);
        }

        string? lesson = null;
        var formulas = new List<IngestFormulaDto>();
        var questions = new List<IngestQuestionDto>();

        // Implicit lead text becomes lesson content. If it also contains a numbered question
        // list (no "### Questions" header), split it: lesson = lines before the first question.
        var firstQ = lead.FindIndex(l => QuestionStart.IsMatch(l));
        if (firstQ < 0)
        {
            var txt = JoinTrim(lead);
            if (!string.IsNullOrWhiteSpace(txt)) lesson = txt;
        }
        else
        {
            var leadLesson = JoinTrim(lead.Take(firstQ).ToList());
            if (!string.IsNullOrWhiteSpace(leadLesson)) lesson = leadLesson;
            questions.AddRange(ParseQuestions(lead.Skip(firstQ).ToList()));
        }

        foreach (var (label, content) in sections)
        {
            var key = label.ToLowerInvariant();
            if (key.StartsWith("lesson") || key.StartsWith("theory"))
            {
                var txt = JoinTrim(content);
                if (!string.IsNullOrWhiteSpace(txt)) lesson = txt;
            }
            else if (key.StartsWith("formula"))
            {
                formulas.AddRange(ParseFormulas(content));
            }
            else if (key.StartsWith("question") || key.StartsWith("practice") || content.Any(l => QuestionStart.IsMatch(l)))
            {
                questions.AddRange(ParseQuestions(content));
            }
            else
            {
                // Unknown H3 (e.g. "### Examples") — fold it into the lesson so nothing is lost.
                var txt = JoinTrim(content);
                if (!string.IsNullOrWhiteSpace(txt))
                    lesson = string.IsNullOrWhiteSpace(lesson) ? $"### {label}\n{txt}" : $"{lesson}\n\n### {label}\n{txt}";
            }
        }

        return new IngestTopicDto(
            name.Trim(),
            sortOrder,
            lesson,
            formulas.Count > 0 ? formulas : null,
            questions.Count > 0 ? questions : null);
    }

    private static IEnumerable<IngestFormulaDto> ParseFormulas(List<string> content)
    {
        var order = 0;
        foreach (var raw in content)
        {
            var line = raw.TrimStart();
            if (line.StartsWith("- ")) line = line[2..];
            else if (line.StartsWith("* ")) line = line[2..];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // "Title | KaTeX | description"
            var parts = line.Split('|');
            if (parts.Length < 2) continue;
            var title = parts[0].Trim();
            var formula = parts[1].Trim().Trim('$').Trim();
            var desc = parts.Length >= 3 ? parts[2].Trim() : null;
            if (title.Length == 0 || formula.Length == 0) continue;

            yield return new IngestFormulaDto(title, formula, string.IsNullOrWhiteSpace(desc) ? null : desc, order++);
        }
    }

    private static List<IngestQuestionDto> ParseQuestions(List<string> content)
    {
        var questions = new List<IngestQuestionDto>();

        string? qText = null;
        var difficulty = "Medium";
        var options = new List<IngestOptionDto>();
        string? explanation = null;
        string? hint = null;
        var order = 0;

        void Flush()
        {
            if (qText == null) return;
            questions.Add(new IngestQuestionDto(
                qText.Trim(),
                options.ToList(),
                string.IsNullOrWhiteSpace(explanation) ? null : explanation!.Trim(),
                string.IsNullOrWhiteSpace(hint) ? null : hint!.Trim(),
                difficulty,
                ++order));
            qText = null;
            difficulty = "Medium";
            options = new List<IngestOptionDto>();
            explanation = null;
            hint = null;
        }

        foreach (var raw in content)
        {
            var line = raw.TrimEnd();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var qm = QuestionStart.Match(line);
            if (qm.Success)
            {
                Flush();
                var rest = qm.Groups[1].Value.Trim();
                var dt = DifficultyTag.Match(rest);
                if (dt.Success)
                {
                    difficulty = Capitalize(dt.Groups[1].Value);
                    rest = rest[dt.Length..].Trim();
                }
                qText = rest;
                continue;
            }

            if (qText == null) continue; // skip stray lines before the first question

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith(">"))
            {
                var e = trimmed.TrimStart('>').Trim();
                explanation = string.IsNullOrWhiteSpace(explanation) ? e : $"{explanation} {e}";
                continue;
            }
            if (trimmed.StartsWith("~") || trimmed.StartsWith("Hint:", StringComparison.OrdinalIgnoreCase))
            {
                hint = trimmed.StartsWith("~") ? trimmed[1..].Trim() : trimmed[5..].Trim();
                continue;
            }

            var om = OptionLine.Match(line);
            if (om.Success)
            {
                var marker = om.Groups[1].Value;
                var body = om.Groups[2].Value.Trim();

                var isCorrect = marker is "*" or "+";
                var cb = Checkbox.Match(body);
                if (cb.Success)
                {
                    isCorrect = isCorrect || cb.Groups[1].Value.Equals("x", StringComparison.OrdinalIgnoreCase);
                    body = body[cb.Length..].Trim();
                }
                if (body.Length > 0)
                    options.Add(new IngestOptionDto(body, isCorrect));
                continue;
            }

            // Continuation line of the question stem.
            qText = $"{qText} {trimmed}".Trim();
        }

        Flush();
        return questions;
    }

    private static string StripPrefix(string value, string prefix)
    {
        var v = value.Trim();
        if (v.StartsWith($"{prefix}:", StringComparison.OrdinalIgnoreCase))
            return v[(prefix.Length + 1)..].Trim();
        return v;
    }

    private static string JoinTrim(List<string> lines)
    {
        var sb = new StringBuilder();
        foreach (var l in lines) sb.Append(l).Append('\n');
        return sb.ToString().Trim();
    }

    private static string Capitalize(string s)
        => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();
}
