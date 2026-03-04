using System.Text.RegularExpressions;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

public class QuestionExtractorService : IQuestionExtractorService
{
    private readonly ILogger<QuestionExtractorService> _logger;

    public QuestionExtractorService(ILogger<QuestionExtractorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract questions from free-form text (PDF/DOCX).
    /// Handles many common formats:
    ///   - "1. Question\nA) option\nB) option"
    ///   - "(1) Question\nA. option\nB. option"
    ///   - "Question 5: ...\nA: option"
    ///   - Inline MCQ: "question text ( )\nA. ...\nB. ..."
    ///   - Roman numerals, Chinese numbering, etc.
    /// </summary>
    public List<ExtractedQuestion> ExtractFromText(string text)
    {
        var questions = new List<ExtractedQuestion>();

        // Normalize line endings and collapse excessive whitespace
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Strategy 1: Split by question-start patterns and parse each block
        var questionBlocks = SplitIntoQuestionBlocks(text);

        foreach (var block in questionBlocks)
        {
            var q = ParseQuestionBlock(block);
            if (q != null)
            {
                questions.Add(q);
            }
        }

        // Strategy 2: If few questions found, look for option-anchored blocks
        // (find groups of A/B/C/D options and grab the preceding text as question)
        if (questions.Count < 3)
        {
            var optionAnchored = ExtractByOptionAnchors(text);
            // Deduplicate by question text similarity
            foreach (var q in optionAnchored)
            {
                if (!questions.Any(existing =>
                    IsSimilarText(existing.QuestionText, q.QuestionText)))
                {
                    questions.Add(q);
                }
            }
        }

        // Strategy 3: Extract fill-in-the-blank and open-ended questions from blocks
        // that were not captured as MCQ (they have blanks/underscores but no A/B/C/D options)
        var fillInQuestions = ExtractFillInBlankQuestions(questionBlocks, questions);
        questions.AddRange(fillInQuestions);

        _logger.LogInformation("Extracted {Count} questions from text ({Length} chars)", questions.Count, text.Length);
        return questions;
    }

    /// <summary>
    /// Extract questions from structured Excel rows.
    /// Expected columns: Question, OptionA, OptionB, OptionC, OptionD, Answer/CorrectAnswer, Explanation, Difficulty
    /// Column matching is case-insensitive and flexible.
    /// </summary>
    public List<ExtractedQuestion> ExtractFromRows(List<Dictionary<string, string>> rows)
    {
        var questions = new List<ExtractedQuestion>();

        foreach (var row in rows)
        {
            var questionText = GetValue(row, "question", "questiontext", "q", "text", "вопрос");
            if (string.IsNullOrWhiteSpace(questionText))
                continue;

            var optionA = GetValue(row, "optiona", "a", "option_a", "option1", "вариант_а", "варианта");
            var optionB = GetValue(row, "optionb", "b", "option_b", "option2", "вариант_б", "вариантб");
            var optionC = GetValue(row, "optionc", "c", "option_c", "option3", "вариант_в", "вариантв");
            var optionD = GetValue(row, "optiond", "d", "option_d", "option4", "вариант_г", "вариантг");
            var answer = GetValue(row, "answer", "correctanswer", "correct", "ответ", "правильный");
            var explanation = GetValue(row, "explanation", "explain", "rationale", "объяснение", "пояснение");
            var hint = GetValue(row, "hint", "подсказка");
            var difficulty = GetValue(row, "difficulty", "level", "сложность", "уровень");

            var options = new List<DraftOptionDto>();
            var optionTexts = new[] { optionA ?? "", optionB ?? "", optionC ?? "", optionD ?? "" };

            var correctIndex = ParseCorrectAnswer(answer, optionTexts);

            for (int i = 0; i < optionTexts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(optionTexts[i]))
                {
                    options.Add(new DraftOptionDto(optionTexts[i]!, i == correctIndex));
                }
            }

            if (options.Count >= 2 && options.Any(o => o.IsCorrect))
            {
                questions.Add(new ExtractedQuestion(
                    questionText,
                    options,
                    string.IsNullOrWhiteSpace(explanation) ? null : explanation,
                    string.IsNullOrWhiteSpace(hint) ? null : hint,
                    NormalizeDifficulty(difficulty)
                ));
            }
        }

        _logger.LogInformation("Extracted {Count} questions from {RowCount} Excel rows", questions.Count, rows.Count);
        return questions;
    }

    // ══════════════════════════════════════════════════════════
    //  QUESTION BLOCK SPLITTING
    // ══════════════════════════════════════════════════════════

    private List<string> SplitIntoQuestionBlocks(string text)
    {
        var blocks = new List<string>();

        // Broad pattern to detect question starts:
        //   1. "1." / "1)" / "1:" — standard numbering
        //   2. "Question 1:" / "Q1:" / "Вопрос 1:"
        //   3. "#1" — hash numbering
        //   4. Roman numerals: "I." / "II."
        // NOTE: (1)/(2) pattern intentionally excluded — it matches sub-parts,
        //       not top-level questions. Sub-parts are handled in fill-in parsing.
        var pattern = @"(?:^|\n)\s*(?:" +
            @"\d{1,4}\s*[\.\)\:]\s" +                       // 1. / 1) / 1:
            @"|(?:Question|Q|Вопрос|Задание|Задача|Упражнение)\s*\d+[:\.\)]\s*" +  // Question 1:
            @"|#\s*\d{1,4}[\.\:\s]" +                       // #1.
            @"|(?=[IVXLC]{1,6}[\.\)]\s)[IVXLC]+[\.\)]\s" +  // I. / II. (Roman)
            @")";

        var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

        for (int i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var block = text.Substring(start, end - start).Trim();
            if (block.Length > 10)
            {
                blocks.Add(block);
            }
        }

        // Fallback: if fewer than 3 numbered questions, try paragraph splitting
        if (blocks.Count < 3)
        {
            var paragraphs = text.Split(new[] { "\n\n\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in paragraphs)
            {
                var trimmed = p.Trim();
                if (trimmed.Length > 20 && ContainsOptionSet(trimmed) &&
                    !blocks.Any(b => IsSimilarText(b, trimmed)))
                {
                    blocks.Add(trimmed);
                }
            }
        }

        return blocks;
    }

    // ══════════════════════════════════════════════════════════
    //  OPTION-ANCHORED EXTRACTION (Strategy 2)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Find groups of consecutive option lines (A/B/C/D) and
    /// use the preceding non-option text as the question.
    /// This catches questions that don't have standard numbering.
    /// </summary>
    private List<ExtractedQuestion> ExtractByOptionAnchors(string text)
    {
        var results = new List<ExtractedQuestion>();
        // Expand inline options before scanning
        var rawLines = text.Split('\n');
        var lines = rawLines.SelectMany(l => ExpandInlineOptions(l.Trim())).ToArray();

        int i = 0;
        while (i < lines.Length)
        {
            // Look for start of option block (line starting with A/А)
            if (IsOptionLine(lines[i].Trim()))
            {
                // Collect all consecutive option lines
                var optionLines = new List<string>();
                var optStart = i;
                while (i < lines.Length && IsOptionLine(lines[i].Trim()))
                {
                    optionLines.Add(lines[i].Trim());
                    i++;
                }

                // Also grab answer/explanation lines that follow
                var answerLine = "";
                var explanationLine = "";
                while (i < lines.Length)
                {
                    var trimmed = lines[i].Trim();
                    if (IsAnswerLine(trimmed))
                    {
                        answerLine = trimmed;
                        i++;
                    }
                    else if (IsExplanationLine(trimmed))
                    {
                        explanationLine = trimmed;
                        i++;
                    }
                    else break;
                }

                if (optionLines.Count < 2) continue;

                // Walk backwards from optStart to find question text
                var questionLines = new List<string>();
                for (int j = optStart - 1; j >= Math.Max(0, optStart - 8); j--)
                {
                    var line = lines[j].Trim();
                    if (string.IsNullOrWhiteSpace(line)) break;
                    if (IsSectionHeader(line)) break;
                    questionLines.Insert(0, line);
                    // Stop if this line looks like it starts a question (has a number prefix)
                    if (Regex.IsMatch(line, @"^(?:\d{1,4}\s*[\.\)\:]|[\(（]\d{1,4}[\)）])"))
                        break;
                }

                var questionText = string.Join(" ", questionLines).Trim();
                questionText = StripLeadingNumber(questionText);

                if (string.IsNullOrWhiteSpace(questionText) || questionText.Length < 5)
                    continue;

                var options = ParseOptionLines(optionLines, answerLine);
                if (options.Count >= 2)
                {
                    results.Add(new ExtractedQuestion(
                        questionText, options,
                        string.IsNullOrWhiteSpace(explanationLine) ? null : CleanExplanationText(explanationLine),
                        null, null));
                }
            }
            else
            {
                i++;
            }
        }

        return results;
    }

    // ══════════════════════════════════════════════════════════
    //  QUESTION BLOCK PARSING
    // ══════════════════════════════════════════════════════════

    private ExtractedQuestion? ParseQuestionBlock(string block)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(l => l.Trim())
                         .Where(l => !string.IsNullOrWhiteSpace(l))
                         .ToList();

        if (lines.Count < 2) return null; // Need at least question + options line

        // Expand inline options: "A. x  B. y  C. z  D. w" → separate lines
        lines = lines.SelectMany(ExpandInlineOptions).ToList();

        // Separate lines into categories
        var questionLines = new List<string>();
        var optionLines = new List<string>();
        var answerLine = "";
        var explanationLine = "";

        bool inOptions = false;
        foreach (var line in lines)
        {
            if (IsOptionLine(line))
            {
                inOptions = true;
                optionLines.Add(line);
            }
            else if (IsAnswerLine(line))
            {
                answerLine = line;
            }
            else if (IsExplanationLine(line))
            {
                explanationLine = line;
            }
            else if (IsSectionHeader(line))
            {
                // Skip section headers like "Exercises 1.1", "Self-Test 1"
                continue;
            }
            else if (!inOptions)
            {
                var cleaned = StripLeadingNumber(line);
                if (!string.IsNullOrWhiteSpace(cleaned))
                    questionLines.Add(cleaned);
            }
            else
            {
                // Lines after options started that aren't options/answer/explanation
                // Could be continuation of last option or sub-question — append to last option
                if (optionLines.Count > 0)
                    optionLines[optionLines.Count - 1] += " " + line;
            }
        }

        var questionText = string.Join(" ", questionLines).Trim();
        if (string.IsNullOrWhiteSpace(questionText) || optionLines.Count < 2)
            return null;

        var options = ParseOptionLines(optionLines, answerLine);
        if (options.Count < 2)
            return null;

        var explanation = CleanExplanationText(explanationLine);

        return new ExtractedQuestion(
            questionText,
            options,
            string.IsNullOrWhiteSpace(explanation) ? null : explanation,
            null,
            null
        );
    }

    // ══════════════════════════════════════════════════════════
    //  LINE CLASSIFICATION HELPERS
    // ══════════════════════════════════════════════════════════

    private static bool IsOptionLine(string line)
    {
        // Matches:  A. / A) / A: / A、/ (A) / А. (Cyrillic А/Б/В/Г)
        return Regex.IsMatch(line,
            @"^(?:" +
            @"[A-Ea-e]\s*[\.\)\:\、]" +          // A. / A) / A: / A、
            @"|[\(（][A-Ea-e][\)）]" +            // (A) / （A）
            @"|[А-Га-г]\s*[\.\)\:\、]" +          // А. / Б) (Cyrillic)
            @"|[\(（][А-Га-г][\)）]" +            // (А) (Cyrillic parenthesized)
            @")\s",
            RegexOptions.None);
    }

    private static bool IsAnswerLine(string line)
    {
        return Regex.IsMatch(line,
            @"^(?:Answer|Correct\s*(?:answer)?|Key|Ответ|Правильный(?:\s*ответ)?)[:\s]",
            RegexOptions.IgnoreCase);
    }

    private static bool IsExplanationLine(string line)
    {
        return Regex.IsMatch(line,
            @"^(?:Explanation|Explain|Solution|Решение|Объяснение|Пояснение|Rationale|Hint)[:\s]",
            RegexOptions.IgnoreCase);
    }

    private static bool IsSectionHeader(string line)
    {
        // Matches: "Exercises 1.1", "Exercise 1:", "Self-Test 1", "Section 1.2",
        //          "Chapter 1", "Раздел 1.1", "Упражнения 1.1"
        return Regex.IsMatch(line,
            @"^(?:Exercise[s]?|Self[- ]Test|Section|Chapter|Part|" +
            @"Раздел|Глава|Упражнени[ея]|Тест|Контрольн)" +
            @"\s*\d",
            RegexOptions.IgnoreCase);
    }

    private static bool ContainsOptionSet(string text)
    {
        // Check that text contains at least A and B options (on separate lines OR inline)
        var hasA = Regex.IsMatch(text, @"(?:^|\n)\s*(?:[Aa][\.\)\:\、]|[\(（][Aa][\)）])\s", RegexOptions.Multiline);
        var hasB = Regex.IsMatch(text, @"(?:^|\n|\s{2,})(?:[Bb][\.\)\:\、]|[\(（][Bb][\)）])\s", RegexOptions.Multiline);
        return hasA && hasB;
    }

    // ══════════════════════════════════════════════════════════
    //  INLINE OPTION EXPANSION
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Split a line that contains multiple inline options into separate lines.
    /// e.g., "A. {6,8} B. {5,7} C. {4,6,7} D. {1,3,5,6,8}" → 4 lines.
    /// </summary>
    private static IEnumerable<string> ExpandInlineOptions(string line)
    {
        if (!IsOptionLine(line))
        {
            yield return line;
            yield break;
        }

        // Count how many option-letter patterns exist in this line
        var optPattern = @"(?:^|\s)([A-Ea-eА-Га-г])\s*[\.\)\:\、]\s";
        var optMatches = Regex.Matches(line, optPattern);

        if (optMatches.Count < 2)
        {
            yield return line;
            yield break;
        }

        // Split before each option letter (B, C, D, E) that follows whitespace
        var parts = Regex.Split(line, @"\s+(?=[B-Eb-eБ-Гб-г]\s*[\.\)\:\、])");
        if (parts.Length >= 2)
        {
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    yield return trimmed;
            }
        }
        else
        {
            yield return line;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  FILL-IN-THE-BLANK EXTRACTION (Strategy 3)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Extract fill-in-the-blank and open-ended questions from blocks
    /// that were not captured as MCQ. Handles:
    ///   - Single fill-in: "A ∪ B = ____, A ∩ B = ____."
    ///   - Compound exercises with sub-parts: (1), (2), (3)...
    ///   - Open-ended: "List all subsets of {1,3,5,7}."
    /// </summary>
    private List<ExtractedQuestion> ExtractFillInBlankQuestions(
        List<string> blocks, List<ExtractedQuestion> alreadyExtracted)
    {
        var results = new List<ExtractedQuestion>();

        foreach (var block in blocks)
        {
            var lines = block.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && !IsSectionHeader(l))
                .ToList();

            if (lines.Count == 0) continue;

            // Skip blocks that have MCQ option lines (already handled by Strategies 1 & 2)
            // Check BOTH original lines and expanded inline options
            if (lines.Any(l => IsOptionLine(l))) continue;

            // Build the full block text
            var firstLine = StripLeadingNumber(lines[0]);

            // Skip if this block was already extracted as MCQ
            if (alreadyExtracted.Any(q => IsSimilarText(q.QuestionText, firstLine)))
                continue;

            // Check for sub-parts: (1), (2), (3), ...
            var subParts = new List<(int Num, string Text)>();
            var instructionLines = new List<string>();

            foreach (var line in lines)
            {
                var subMatch = Regex.Match(line, @"^[\(（]\s*(\d+)\s*[\)）]\s*(.*)");
                if (subMatch.Success)
                {
                    subParts.Add((
                        int.Parse(subMatch.Groups[1].Value),
                        subMatch.Groups[2].Value.Trim()));
                }
                else if (subParts.Count == 0)
                {
                    // Lines before first sub-part are the parent instruction
                    instructionLines.Add(StripLeadingNumber(line));
                }
            }

            var instruction = string.Join(" ", instructionLines).Trim();

            if (subParts.Count > 0 && !string.IsNullOrWhiteSpace(instruction))
            {
                // Compound exercise: create one question per sub-part with parent context
                foreach (var (num, subText) in subParts)
                {
                    if (string.IsNullOrWhiteSpace(subText)) continue;
                    var questionText = $"{instruction} — ({num}) {subText}";
                    results.Add(new ExtractedQuestion(
                        questionText, new List<DraftOptionDto>(), null, null, null));
                }
            }
            else if (!string.IsNullOrWhiteSpace(instruction) && instruction.Length >= 10)
            {
                // Single fill-in or open-ended question
                var fullText = string.Join(" ", lines.Select(StripLeadingNumber)).Trim();
                results.Add(new ExtractedQuestion(
                    fullText, new List<DraftOptionDto>(), null, null, null));
            }
        }

        return results;
    }

    // ══════════════════════════════════════════════════════════
    //  PARSING HELPERS
    // ══════════════════════════════════════════════════════════

    private static List<DraftOptionDto> ParseOptionLines(List<string> optionLines, string answerLine)
    {
        var options = new List<DraftOptionDto>();
        var correctAnswer = ParseAnswerFromLine(answerLine);

        for (int i = 0; i < optionLines.Count; i++)
        {
            var optText = CleanOptionText(optionLines[i]);
            var letter = GetOptionLetter(optionLines[i]);
            var isCorrect = !string.IsNullOrEmpty(correctAnswer) &&
                            letter.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(optText))
                options.Add(new DraftOptionDto(optText, isCorrect));
        }

        return options;
    }

    private static string StripLeadingNumber(string line)
    {
        // Strip: "1. ", "1) ", "(1) ", "（1）", "#1 ", "Q1: ", "Question 1: "
        return Regex.Replace(line,
            @"^(?:" +
            @"\d{1,4}\s*[\.\)\:]\s*" +
            @"|[\(（]\s*\d{1,4}\s*[\)）]\s*" +
            @"|#\s*\d{1,4}[\.\:\s]\s*" +
            @"|(?:Question|Q|Вопрос|Задание|Задача)\s*\d+[:\.\)]\s*" +
            @")",
            "", RegexOptions.IgnoreCase).Trim();
    }

    private static string CleanOptionText(string line)
    {
        // Remove option letter prefix: A. / A) / (A) / А. etc.
        return Regex.Replace(line,
            @"^(?:" +
            @"[A-Ea-e]\s*[\.\)\:\、]\s*" +
            @"|[\(（][A-Ea-e][\)）]\s*" +
            @"|[А-Га-г]\s*[\.\)\:\、]\s*" +
            @"|[\(（][А-Га-г][\)）]\s*" +
            @")",
            "").Trim();
    }

    private static string GetOptionLetter(string line)
    {
        // Extract the letter from option line
        var match = Regex.Match(line,
            @"^(?:[\(（]?\s*([A-Ea-eА-Га-г])\s*[\.\)\:\、）]?)");
        if (match.Success)
        {
            var letter = match.Groups[1].Value.ToUpper();
            // Normalize Cyrillic: А→A, Б→B, В→C, Г→D
            return letter switch
            {
                "А" => "A", "Б" => "B", "В" => "C", "Г" => "D",
                _ => letter
            };
        }
        return "";
    }

    private static string ParseAnswerFromLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return "";
        var match = Regex.Match(line,
            @"(?:Answer|Correct|Key|Ответ|Правильный)[:\s]+([A-Ea-eА-Га-г])",
            RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var letter = match.Groups[1].Value.ToUpper();
            return letter switch
            {
                "А" => "A", "Б" => "B", "В" => "C", "Г" => "D",
                _ => letter
            };
        }
        return "";
    }

    private static string CleanExplanationText(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return "";
        return Regex.Replace(line,
            @"^(?:Explanation|Explain|Solution|Решение|Объяснение|Пояснение|Rationale|Hint)[:\s]*",
            "", RegexOptions.IgnoreCase).Trim();
    }

    private static bool IsSimilarText(string a, string b)
    {
        // Simple similarity: check if normalized forms share most content
        var na = Regex.Replace(a.ToLower(), @"\s+", " ").Trim();
        var nb = Regex.Replace(b.ToLower(), @"\s+", " ").Trim();
        if (na.Length < 10 || nb.Length < 10) return na == nb;
        // Check if one contains the other or starts the same
        return na.StartsWith(nb[..Math.Min(40, nb.Length)]) ||
               nb.StartsWith(na[..Math.Min(40, na.Length)]);
    }

    private static int ParseCorrectAnswer(string? answer, string[] optionTexts)
    {
        if (string.IsNullOrWhiteSpace(answer)) return -1;

        var trimmed = answer.Trim().ToUpper();

        // Single letter: A, B, C, D, E
        if (trimmed.Length == 1 && trimmed[0] >= 'A' && trimmed[0] <= 'E')
            return trimmed[0] - 'A';

        // Cyrillic: А, Б, В, Г
        if (trimmed.Length == 1)
        {
            var idx = "АБВГ".IndexOf(trimmed[0]);
            if (idx >= 0) return idx;
        }

        // Number: 1, 2, 3, 4
        if (int.TryParse(trimmed, out var num) && num >= 1 && num <= 5)
            return num - 1;

        // Match by text content
        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(optionTexts[i]) &&
                optionTexts[i].Trim().Equals(answer.Trim(), StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static string? NormalizeDifficulty(string? difficulty)
    {
        if (string.IsNullOrWhiteSpace(difficulty)) return null;
        var d = difficulty.Trim().ToLower();
        return d switch
        {
            "easy" or "1" or "лёгкий" or "легкий" or "лёгко" or "легко" => "Easy",
            "medium" or "2" or "средний" or "средне" => "Medium",
            "hard" or "3" or "difficult" or "сложный" or "сложно" or "трудный" => "Hard",
            _ => null
        };
    }

    private static string? GetValue(Dictionary<string, string> row, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
                return val;

            var match = row.FirstOrDefault(kvp =>
                kvp.Key.Replace(" ", "").Replace("_", "").Equals(key, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(kvp.Value));
            if (!string.IsNullOrEmpty(match.Value))
                return match.Value;
        }
        return null;
    }

    // ══════════════════════════════════════════════════════════
    //  ANSWER KEY EXTRACTION (for cross-matching with question files)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Extract answer keys from an answer-only file.
    /// Handles formats like:
    ///   "1. C", "1) A", "1 - B", "1.C", "#1: D"
    ///   "1-C 2-A 3-B" (inline), "1C 2A 3B" (compact)
    ///   Russian: "1. В", "1) Б"
    /// Returns dictionary: questionNumber → answerLetter (normalized to Latin A/B/C/D)
    /// </summary>
    public Dictionary<int, string> ExtractAnswerKeys(string text)
    {
        var keys = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(text)) return keys;

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Pattern: number followed by separator and a single letter answer
        // Matches: "1. C", "1) A", "1-B", "1: D", "1.C", "#1 C"
        var linePattern = new Regex(
            @"#?\s*(\d{1,4})\s*[\.\)\:\-–—]\s*([A-DА-Гa-dа-г])\b",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (Match m in linePattern.Matches(text))
        {
            if (int.TryParse(m.Groups[1].Value, out var num) && num > 0)
            {
                var letter = NormalizeCyrillicAnswer(m.Groups[2].Value.Trim().ToUpper());
                keys.TryAdd(num, letter);
            }
        }

        // Also try compact format: "1C 2A 3B" or "1C, 2A, 3B"
        if (keys.Count < 3)
        {
            var compactPattern = new Regex(@"\b(\d{1,4})([A-DА-Гa-dа-г])\b", RegexOptions.IgnoreCase);
            foreach (Match m in compactPattern.Matches(text))
            {
                if (int.TryParse(m.Groups[1].Value, out var num) && num > 0)
                {
                    var letter = NormalizeCyrillicAnswer(m.Groups[2].Value.Trim().ToUpper());
                    keys.TryAdd(num, letter);
                }
            }
        }

        _logger.LogInformation("Extracted {Count} answer keys from text", keys.Count);
        return keys;
    }

    /// <summary>
    /// Detect topic/chapter sections in text.
    /// Looks for patterns like:
    ///   "Chapter 1: Sets", "Глава 2. Неравенства", "Topic 3 - Algebra"
    ///   "Section 1.1:", "Раздел 4:", "Тема 5."
    /// Returns list of (Title, StartQuestion, EndQuestion)
    /// </summary>
    public List<(string Title, int StartQuestion, int EndQuestion)> DetectTopicSections(string text)
    {
        var sections = new List<(string Title, int LineIndex)>();
        if (string.IsNullOrWhiteSpace(text)) return new();

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = text.Split('\n');

        // Pattern for section/chapter headers
        var headerPattern = new Regex(
            @"^\s*(?:" +
            @"(?:Chapter|Section|Topic|Unit|Part|Lesson)\s+[\d\.]+[:\.\-–—]\s*(.+)" +  // English
            @"|(?:Глава|Раздел|Тема|Часть|Урок|Модуль)\s+[\d\.]+[:\.\-–—]\s*(.+)" +   // Russian
            @")\s*$",
            RegexOptions.IgnoreCase);

        // Also detect numbered chapter titles (e.g. "1. Sets and Logic")
        var numberedHeaderPattern = new Regex(
            @"^\s*(?:(?:Chapter|Глава|Раздел|Тема)\s+)?(\d+(?:\.\d+)?)\s*[:\.\-–—]\s*([A-ZА-Я].{3,80})\s*$",
            RegexOptions.IgnoreCase);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var m = headerPattern.Match(line);
            if (m.Success)
            {
                var title = (m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value).Trim();
                sections.Add((title, i));
                continue;
            }

            var nm = numberedHeaderPattern.Match(line);
            if (nm.Success)
            {
                sections.Add((nm.Groups[2].Value.Trim(), i));
            }
        }

        if (sections.Count == 0) return new();

        // Now find the question number ranges for each section
        var questionNumberPattern = new Regex(@"(?:^|\n)\s*(\d{1,4})\s*[\.\)\:]");
        var result = new List<(string Title, int StartQuestion, int EndQuestion)>();

        for (int s = 0; s < sections.Count; s++)
        {
            var startLine = sections[s].LineIndex;
            var endLine = s < sections.Count - 1 ? sections[s + 1].LineIndex : lines.Length;

            var sectionText = string.Join("\n", lines[startLine..endLine]);
            var questionNumbers = questionNumberPattern.Matches(sectionText)
                .Select(m => int.TryParse(m.Groups[1].Value, out var n) ? n : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            if (questionNumbers.Count > 0)
            {
                result.Add((sections[s].Title, questionNumbers.First(), questionNumbers.Last()));
            }
            else
            {
                // Section with no detected questions — use approximate numbering
                result.Add((sections[s].Title, 0, 0));
            }
        }

        _logger.LogInformation("Detected {Count} topic sections in text", result.Count);
        return result;
    }

    /// <summary>Convert Cyrillic answer letters to Latin equivalents</summary>
    private static string NormalizeCyrillicAnswer(string letter)
    {
        return letter switch
        {
            "А" => "A",
            "Б" => "B",
            "В" => "C",  // В (Cyrillic) → C (3rd option)
            "Г" => "D",
            _ => letter
        };
    }
}
