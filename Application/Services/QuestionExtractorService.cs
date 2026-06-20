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
            var questionText = GetValue(row, "question", "questiontext", "q", "text", "вопрос", "题目", "问题", "试题");
            if (string.IsNullOrWhiteSpace(questionText))
                continue;

            var optionA = GetValue(row, "optiona", "a", "option_a", "option1", "вариант_а", "варианта", "选项a", "甲");
            var optionB = GetValue(row, "optionb", "b", "option_b", "option2", "вариант_б", "вариантб", "选项b", "乙");
            var optionC = GetValue(row, "optionc", "c", "option_c", "option3", "вариант_в", "вариантв", "选项c", "丙");
            var optionD = GetValue(row, "optiond", "d", "option_d", "option4", "вариант_г", "вариантг", "选项d", "丁");
            var answer = GetValue(row, "answer", "correctanswer", "correct", "ответ", "правильный", "答案", "正确答案");
            var explanation = GetValue(row, "explanation", "explain", "rationale", "объяснение", "пояснение", "解析", "详解");
            var hint = GetValue(row, "hint", "подсказка", "提示");
            var difficulty = GetValue(row, "difficulty", "level", "сложность", "уровень", "难度");

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
        //   2. "Question 1:" / "Q1:" / "Вопрос 1:" / "第1题"
        //   3. "#1" — hash numbering
        //   4. Roman numerals: "I." / "II."
        //   5. Chinese circled/parenthesized numbers: （1）
        // NOTE: (1)/(2) pattern intentionally excluded — it matches sub-parts,
        //       not top-level questions. Sub-parts are handled in fill-in parsing.
        var pattern = @"(?:^|\n)\s*(?:" +
            @"\d{1,4}\s*[\.\)\:\、](?:\s|(?=[\u4e00-\u9fff]))" +  // 1. / 1) / 1、(+CJK)
            @"|(?:Question|Q|Вопрос|Задание|Задача|Упражнение|题目|问题)\s*\d+[:\.\)：、]\s*" +
            @"|Q\s*\d{1,4}(?=\s)" +                         // Q001<TAB> / Q12  (template id, no delimiter)
            @"|#\s*\d{1,4}[\.\:\s]" +                       // #1.
            @"|(?=[IVXLC]{2,6}[\.\)]\s)[IVXLC]{2,}[\.\)]\s" +  // II. / III. / IV. (Roman, 2+ chars — single letters clash with MCQ options like "C)")
            @"|第\s*\d{1,4}\s*题[\.\:\、：]?\s*" +         // 第1题 / 第2题：
            @"|（\d{1,4}）\s*" +                        // （1）(fullwidth parenthesized)
            @"|\d{1,4}\s*[\.\)\、]\s*(?=[\u4e00-\u9fff(\(（$])" + // CJK: 1.设 / 1、若
            @")";

        var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

        for (int i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var block = text.Substring(start, end - start).Trim();
            if (block.Length > 10 && !IsTextbookContent(block))
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
        string? metaDifficulty = null;
        bool inExplanation = false;

        bool inOptions = false;
        foreach (var line in lines)
        {
            // Template meta header: "Q001  Difficulty: Easy  Source: Original" (or a bare "Difficulty: …" line).
            // Skip it from the question text but capture the difficulty if present.
            if (!inOptions && TryParseMetaHeader(line, out var headerDifficulty))
            {
                if (headerDifficulty != null) metaDifficulty = headerDifficulty;
                continue;
            }

            if (IsOptionLine(line))
            {
                inOptions = true;
                inExplanation = false;
                optionLines.Add(line);
            }
            else if (IsAnswerLine(line))
            {
                answerLine = line;
            }
            else if (IsExplanationLine(line))
            {
                inExplanation = true;
                var inlineExpl = CleanExplanationText(line);
                if (!string.IsNullOrWhiteSpace(inlineExpl))
                    explanationLine = explanationLine.Length == 0 ? inlineExpl : explanationLine + " " + inlineExpl;
            }
            else if (inExplanation)
            {
                // Lines after a standalone EXPLANATION keyword belong to the explanation.
                explanationLine = explanationLine.Length == 0 ? line : explanationLine + " " + line;
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
        if (string.IsNullOrWhiteSpace(questionText))
            return null;

        // If no MCQ options but we have a solution/answer line, generate distractors
        if (optionLines.Count < 2)
        {
            var solutionText = ExtractSolutionValue(answerLine) ?? ExtractSolutionValue(explanationLine);
            if (!string.IsNullOrEmpty(solutionText))
            {
                var generatedOptions = GenerateDistractors(solutionText);
                var solExplanation = CleanExplanationText(explanationLine);
                return new ExtractedQuestion(
                    questionText,
                    generatedOptions,
                    string.IsNullOrWhiteSpace(solExplanation) ? $"Solution: {solutionText}" : solExplanation,
                    null,
                    metaDifficulty
                );
            }
            return null;
        }

        var options = ParseOptionLines(optionLines, answerLine);
        if (options.Count < 2)
            return null;

        var explanation = CleanExplanationText(explanationLine);

        return new ExtractedQuestion(
            questionText,
            options,
            string.IsNullOrWhiteSpace(explanation) ? null : explanation,
            null,
            metaDifficulty
        );
    }

    // ══════════════════════════════════════════════════════════
    //  LINE CLASSIFICATION HELPERS
    // ══════════════════════════════════════════════════════════

    private static bool IsOptionLine(string line)
    {
        // Matches:  A. / A) / A: / A、/ (A) / А. (Cyrillic А/Б/В/Г) / 甲/乙/丙/丁 (Chinese)
        // Trailing: \s or CJK char (Chinese text has no space after punctuation)
        return Regex.IsMatch(line,
            @"^(?:" +
            @"[A-Ea-e]\s*[\.\)\:\、]" +          // A. / A) / A: / A、
            @"|[\(（][A-Ea-e][\)）]" +            // (A) / （A）
            @"|[А-Га-г]\s*[\.\)\:\、]" +          // А. / Б) (Cyrillic)
            @"|[\(（][А-Га-г][\)）]" +            // (А) (Cyrillic parenthesized)
            @"|[甲乙丙丁]\s*[\.\)\:\、．）]" +     // 甲. / 乙) / 丙、(Chinese traditional)
            @"|[\(（][甲乙丙丁][\)）]" +           // (甲) / （乙）
            @")(?:\s|(?=[\u4e00-\u9fff\uff00-\uffef]))",
            RegexOptions.None);
    }

    private static bool IsAnswerLine(string line)
    {
        return Regex.IsMatch(line,
            @"^(?:Answer|Correct\s*(?:answer)?|Key|Ответ|Правильный(?:\s*ответ)?|答案|正确答案|参考答案)[：:\s]",
            RegexOptions.IgnoreCase);
    }

    private static bool IsExplanationLine(string line)
    {
        return Regex.IsMatch(line,
            @"^(?:Explanation|Explain|Solution|Working|Решение|Объяснение|Пояснение|Rationale|Hint|解析|解答|解题思路|详解|提示)(?:[：:\s]|$)",
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Check if the line looks like a Chinese-format question number (e.g. "1.设集合..." or "3、若...")
    /// where there is no whitespace between the number/punctuation and the CJK text.
    /// </summary>
    private static bool IsCjkQuestionStart(string line)
    {
        return Regex.IsMatch(line,
            @"^\d{1,4}\s*[\.\)\、．）]\s*[\u4e00-\u9fff(\(（$\\]");
    }

    private static bool IsSectionHeader(string line)
    {
        // Matches: "Exercises 1.1", "Exercise 1:", "Self-Test 1", "Section 1.2",
        //          "Chapter 1", "Раздел 1.1", "Упражнения 1.1"
        //          "第一章", "第1节", "第三单元"
        return Regex.IsMatch(line,
            @"^(?:Exercise[s]?|Self[- ]Test|Section|Chapter|Part|" +
            @"Раздел|Глава|Упражнени[ея]|Тест|Контрольн)" +
            @"\s*\d",
            RegexOptions.IgnoreCase)
            || Regex.IsMatch(line, @"^第[一二三四五六七八九十百\d]+[章节单元部分课]");
    }

    /// <summary>
    /// Detect text blocks that are textbook content (definitions, examples, notes)
    /// rather than actual test questions. Used to filter out false positives from OCR text.
    /// </summary>
    private static bool IsTextbookContent(string block)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var firstLine = lines.FirstOrDefault()?.Trim() ?? "";
        var stripped = StripLeadingNumber(firstLine);

        // If block has answer options (A/B/C/D), it's likely a real question — keep it
        if (ContainsOptionSet(block)) return false;

        // ── Chinese textbook patterns ──

        // Definitions, examples, notes, theorems, proofs, summaries
        if (Regex.IsMatch(stripped, @"^(?:定义|例\s*\d|注\s*\d|性质|定理|推论|小结|证明|解[题答：:]|我们把|由所有|即)",
            RegexOptions.IgnoreCase))
            return true;

        // Chapter/section headers: "第X章", "第X节", chapter titles with numbers
        if (Regex.IsMatch(stripped, @"第\s*\d+\s*章|第\s*\d+\s*节|空间\S+的\S+|平面向量|空间向量|导数|排列|组合|随机变量|概率分布",
            RegexOptions.None))
            return true;

        // Content starting with explanation keywords
        if (Regex.IsMatch(stripped, @"^(?:我们|由|即|例如|因为|所以|由此|综上|特别地|一般地|显然|关于|用符号|写出集合)",
            RegexOptions.None))
            return true;

        // Textbook summary items: "集合的特点", "元素和集合之间的关系"
        if (Regex.IsMatch(stripped, @"^(?:集合|元素|自然数|整数|有理数|实数|不等式|区间|向量)",
            RegexOptions.None) && !Regex.IsMatch(block, @"\(\s*\)", RegexOptions.None))
            return true;

        // Fill-in-the-blank exercises from textbook (not test): "用符号…填空"
        if (Regex.IsMatch(block, @"填空|填入", RegexOptions.None))
            return true;

        // Block contains "definition" in English or Chinese
        if (Regex.IsMatch(block, @"\(definition\)|\(subset\)|\(union set\)|\(empty set\)|\(equality\)|\(interval\)|\(inequality\)",
            RegexOptions.IgnoreCase))
            return true;

        // ── Russian textbook patterns ──
        if (Regex.IsMatch(stripped, @"^(?:Определение|Пример|Примечание|Свойство|Теорема|Доказательство|Следствие|Замечание)",
            RegexOptions.IgnoreCase))
            return true;

        // ── Universal heuristics ──

        // Block is long (>800 chars) without answer options → likely textbook paragraph
        if (block.Length > 800 && !ContainsOptionSet(block))
            return true;

        // Block has no question-like pattern (no ( ) placeholder, no 则, no 求, no ?/？)
        // and is just a statement → textbook content
        if (!Regex.IsMatch(block, @"[?？]|\(\s*\)|则下列|下列.*正确|等于|求|解不等式", RegexOptions.None)
            && !ContainsOptionSet(block)
            && block.Length > 100)
            return true;

        // Table of contents: multiple chapter references
        if (Regex.IsMatch(block, @"第\d+章.*第\d+章", RegexOptions.Singleline))
            return true;

        return false;
    }

    private static bool ContainsOptionSet(string text)
    {
        // Check that text contains at least A and B options (on separate lines OR inline)
        // Allow CJK character to follow directly (no space needed)
        var hasA = Regex.IsMatch(text, @"(?:^|\n)\s*(?:[Aa][\.\)\:\、]|[\(（][Aa][\)）]|甲\s*[\.\)\:\、．）])(?:\s|[\u4e00-\u9fff])", RegexOptions.Multiline);
        var hasB = Regex.IsMatch(text, @"(?:^|\n|\s{2,})(?:[Bb][\.\)\:\、]|[\(（][Bb][\)）]|乙\s*[\.\)\:\、．）])(?:\s|[\u4e00-\u9fff])", RegexOptions.Multiline);
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

        // Split before each option letter (B, C, D, E or 乙, 丙, 丁) that follows whitespace
        var parts = Regex.Split(line, @"\s+(?=[B-Eb-eБ-Гб-г乙丙丁]\s*[\.\)\:\、．）])");
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
            var rawLine = optionLines[i];
            var inlineCorrect = HasCorrectMarker(rawLine);
            var optText = CleanOptionText(StripCorrectMarker(rawLine));
            var letter = GetOptionLetter(rawLine);
            var isCorrect = inlineCorrect ||
                            (!string.IsNullOrEmpty(correctAnswer) &&
                             letter.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(optText))
                options.Add(new DraftOptionDto(optText, isCorrect));
        }

        return options;
    }

    /// <summary>True if the option line carries an inline "correct answer" marker (✓ ✔ ☑ ✅ or "(correct)").</summary>
    private static bool HasCorrectMarker(string line) =>
        Regex.IsMatch(line, @"[✓✔☑✅]") ||
        Regex.IsMatch(line, @"\(\s*(?:correct|right|правильн\w*|верн\w*)\s*\)\s*$", RegexOptions.IgnoreCase);

    /// <summary>Remove inline correct-answer markers so they do not leak into the stored option text.</summary>
    private static string StripCorrectMarker(string line) =>
        Regex.Replace(
            Regex.Replace(line, @"\s*[✓✔☑✅]\s*", " "),
            @"\s*\(\s*(?:correct|right|правильн\w*|верн\w*)\s*\)\s*$", "", RegexOptions.IgnoreCase).Trim();

    /// <summary>
    /// Recognise a template meta/header line such as
    /// "Q001  Difficulty: Easy  Source: Original" or a standalone "Difficulty: Medium".
    /// Returns true when the line is metadata (and should not become question text);
    /// <paramref name="difficulty"/> is set to Easy/Medium/Hard when present.
    /// </summary>
    private static bool TryParseMetaHeader(string line, out string? difficulty)
    {
        difficulty = null;

        var isQId = Regex.IsMatch(line, @"^Q\s*\d{1,4}\b", RegexOptions.IgnoreCase);
        var diffMatch = Regex.Match(line,
            @"\bDifficulty\s*[:：]\s*(Easy|Medium|Hard|Лёгк\w*|Легк\w*|Средн\w*|Сложн\w*)",
            RegexOptions.IgnoreCase);

        if (!isQId && !diffMatch.Success) return false;

        if (diffMatch.Success)
        {
            var d = diffMatch.Groups[1].Value.ToLowerInvariant();
            difficulty = (d.StartsWith("eas") || d.StartsWith("лёг") || d.StartsWith("лег")) ? "Easy"
                       : (d.StartsWith("har") || d.StartsWith("слож")) ? "Hard"
                       : "Medium";
        }

        // A "Q123 …" id line is always a header. Otherwise only treat short
        // "Difficulty:/Source:" style lines as metadata to avoid eating real questions.
        if (isQId) return true;
        return Regex.IsMatch(line, @"^(?:Difficulty|Source|Источник|Сложность)\s*[:：]", RegexOptions.IgnoreCase);
    }

    private static string StripLeadingNumber(string line)
    {
        // Strip: "1. ", "1) ", "(1) ", "（1）", "#1 ", "Q1: ", "Question 1: ", "第1题"
        return Regex.Replace(line,
            @"^(?:" +
            @"\d{1,4}\s*[\.\)\:\、]\s*" +
            @"|[\(（]\s*\d{1,4}\s*[\)）]\s*" +
            @"|#\s*\d{1,4}[\.\:\s]\s*" +
            @"|(?:Question|Q|Вопрос|Задание|Задача|题目|问题)\s*\d+[:\.\)\:\、：]\s*" +
            @"|第\s*\d{1,4}\s*题[\.\:\、：]?\s*" +
            @")",
            "", RegexOptions.IgnoreCase).Trim();
    }

    private static string CleanOptionText(string line)
    {
        // Remove option letter prefix: A. / A) / (A) / А. / 甲. etc.
        return Regex.Replace(line,
            @"^(?:" +
            @"[A-Ea-e]\s*[\.\)\:\、]\s*" +
            @"|[\(（][A-Ea-e][\)）]\s*" +
            @"|[А-Га-г]\s*[\.\)\:\、]\s*" +
            @"|[\(（][А-Га-г][\)）]\s*" +
            @"|[甲乙丙丁]\s*[\.\)\:\、．）]\s*" +
            @"|[\(（][甲乙丙丁][\)）]\s*" +
            @")",
            "").Trim();
    }

    private static string GetOptionLetter(string line)
    {
        // Extract the letter from option line (Latin, Cyrillic, or Chinese)
        var match = Regex.Match(line,
            @"^(?:[\(（]?\s*([A-Ea-eА-Га-г甲乙丙丁])\s*[\.\)\:\、．）]?)");
        if (match.Success)
        {
            var letter = match.Groups[1].Value.ToUpper();
            // Normalize Cyrillic: А→A, Б→B, В→C, Г→D
            // Normalize Chinese: 甲→A, 乙→B, 丙→C, 丁→D
            return letter switch
            {
                "А" => "A", "Б" => "B", "В" => "C", "Г" => "D",
                "甲" => "A", "乙" => "B", "丙" => "C", "丁" => "D",
                _ => letter
            };
        }
        return "";
    }

    private static string ParseAnswerFromLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return "";
        var match = Regex.Match(line,
            @"(?:Answer|Correct|Key|Ответ|Правильный|答案|正确答案|参考答案)[：:\s]+([A-Ea-eА-Га-г甲乙丙丁])",
            RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var letter = match.Groups[1].Value.ToUpper();
            return letter switch
            {
                "А" => "A", "Б" => "B", "В" => "C", "Г" => "D",
                "甲" => "A", "乙" => "B", "丙" => "C", "丁" => "D",
                _ => letter
            };
        }
        return "";
    }

    private static string CleanExplanationText(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return "";
        return Regex.Replace(line,
            @"^(?:Explanation|Explain|Solution|Решение|Объяснение|Пояснение|Rationale|Hint|解析|解答|解题思路|详解|提示)[：:\s]*",
            "", RegexOptions.IgnoreCase).Trim();
    }

    /// <summary>
    /// Extract the value from a Solution/Explanation/Answer line.
    /// Returns the portion after the prefix keyword, or null if empty.
    /// </summary>
    private static string? ExtractSolutionValue(string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        // If the line is an answer line like "Answer: B", skip — that's a letter reference, not a value
        if (IsAnswerLine(line))
        {
            // But if the answer is more than just a letter, extract it as a value
            var letterOnly = Regex.Match(line,
                @"(?:Answer|Correct|Key|Ответ|Правильный|答案|正确答案|参考答案)[：:\s]+([A-Ea-eА-Га-г甲乙丙丁])\s*$",
                RegexOptions.IgnoreCase);
            if (letterOnly.Success) return null; // Just a letter like "Answer: B"
        }

        // Strip the prefix keyword to get the actual solution content
        var value = Regex.Replace(line,
            @"^(?:Explanation|Explain|Solution|Решение|Объяснение|Пояснение|Rationale|Hint|Answer|Correct\s*(?:answer)?|Key|Ответ|Правильный(?:\s*ответ)?|答案|正确答案|参考答案|解析|解答|解题思路|详解|提示)[：:\s]*",
            "", RegexOptions.IgnoreCase).Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Generate 3 plausible wrong answer options + the correct one (marked IsCorrect).
    /// Handles numeric answers with arithmetic variations and text answers with structural alterations.
    /// </summary>
    private static List<DraftOptionDto> GenerateDistractors(string correctAnswer)
    {
        var options = new List<DraftOptionDto>();
        var rng = new Random(correctAnswer.GetHashCode()); // deterministic seed for consistency

        // Try numeric distractor generation
        if (TryGenerateNumericDistractors(correctAnswer, rng, out var numericOptions))
        {
            options = numericOptions;
        }
        // Try set/collection distractor generation (e.g., "{1,2,3}")
        else if (TryGenerateSetDistractors(correctAnswer, rng, out var setOptions))
        {
            options = setOptions;
        }
        // Fallback: generic labeled distractors
        else
        {
            options = GenerateGenericDistractors(correctAnswer);
        }

        // Shuffle options deterministically
        return options.OrderBy(o => rng.Next()).ToList();
    }

    private static bool TryGenerateNumericDistractors(string answer, Random rng, out List<DraftOptionDto> options)
    {
        options = new List<DraftOptionDto>();

        // Check if the answer is a simple number (integer or decimal)
        var numMatch = Regex.Match(answer.Trim(), @"^[-−]?\s*(\d+(?:[.,]\d+)?)\s*$");
        if (!numMatch.Success) return false;

        var numStr = answer.Trim().Replace(",", ".").Replace("−", "-");
        if (!double.TryParse(numStr, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var value))
            return false;

        var distractors = new HashSet<string> { answer.Trim() };
        var attempts = 0;

        while (distractors.Count < 4 && attempts < 20)
        {
            attempts++;
            double variation;
            var strategy = rng.Next(4);

            if (value == 0)
            {
                // Special case for zero
                variation = strategy switch
                {
                    0 => rng.Next(1, 5),
                    1 => -rng.Next(1, 5),
                    2 => rng.Next(1, 10) * 0.5,
                    _ => -rng.Next(1, 10) * 0.5
                };
            }
            else if (Math.Abs(value) < 20 && value == Math.Floor(value))
            {
                // Small integers: ±1, ±2, ±3 variations
                variation = value + (strategy switch
                {
                    0 => rng.Next(1, 4),
                    1 => -rng.Next(1, 4),
                    2 => rng.Next(1, 3) * 2,
                    _ => -rng.Next(1, 3) * 2
                });
            }
            else
            {
                // Larger numbers or decimals: percentage-based variations
                var factor = 1 + (rng.NextDouble() * 0.4 - 0.2); // ±20%
                variation = strategy switch
                {
                    0 => value * factor,
                    1 => value + (value > 0 ? rng.Next(1, (int)Math.Max(2, Math.Abs(value) * 0.3)) : -rng.Next(1, (int)Math.Max(2, Math.Abs(value) * 0.3))),
                    2 => value * (strategy % 2 == 0 ? 2 : 0.5),
                    _ => -value
                };
            }

            // Format distractor the same way as the original
            string formatted;
            if (value == Math.Floor(value) && !answer.Contains(".") && !answer.Contains(","))
                formatted = ((int)Math.Round(variation)).ToString();
            else
                formatted = Math.Round(variation, 2).ToString(System.Globalization.CultureInfo.InvariantCulture);

            distractors.Add(formatted);
        }

        // If we couldn't generate enough unique distractors, pad with simple offsets
        var offset = 1;
        while (distractors.Count < 4)
        {
            if (value == Math.Floor(value))
                distractors.Add(((int)(value + offset * (distractors.Count % 2 == 0 ? 1 : -1))).ToString());
            else
                distractors.Add((value + offset * 0.5 * (distractors.Count % 2 == 0 ? 1 : -1))
                    .ToString(System.Globalization.CultureInfo.InvariantCulture));
            offset++;
        }

        options = distractors.Select(d => new DraftOptionDto(d, d == answer.Trim())).ToList();
        return true;
    }

    private static bool TryGenerateSetDistractors(string answer, Random rng, out List<DraftOptionDto> options)
    {
        options = new List<DraftOptionDto>();

        // Match set-like answers: {1,2,3}, {a,b,c}, (1,2,3)
        var setMatch = Regex.Match(answer.Trim(), @"^[\{\(\[](.+)[\}\)\]]$");
        if (!setMatch.Success) return false;

        var elements = setMatch.Groups[1].Value
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .ToList();

        if (elements.Count < 2) return false;

        var bracket = answer.Trim()[0] switch
        {
            '{' => (open: "{", close: "}"),
            '(' => (open: "(", close: ")"),
            '[' => (open: "[", close: "]"),
            _ => (open: "{", close: "}")
        };

        var distractors = new HashSet<string> { answer.Trim() };

        // Strategy 1: Remove one element
        if (elements.Count > 2)
        {
            var reduced = elements.ToList();
            reduced.RemoveAt(rng.Next(reduced.Count));
            distractors.Add($"{bracket.open}{string.Join(",", reduced)}{bracket.close}");
        }

        // Strategy 2: Add an extra element
        if (elements.All(e => int.TryParse(e, out _)))
        {
            var nums = elements.Select(int.Parse).ToList();
            var extra = nums.Max() + rng.Next(1, 4);
            var expanded = nums.Append(extra).OrderBy(n => n).Select(n => n.ToString());
            distractors.Add($"{bracket.open}{string.Join(",", expanded)}{bracket.close}");
        }

        // Strategy 3: Swap one element
        if (elements.All(e => int.TryParse(e, out _)))
        {
            var nums = elements.Select(int.Parse).ToList();
            var idx = rng.Next(nums.Count);
            nums[idx] = nums[idx] + rng.Next(1, 4);
            distractors.Add($"{bracket.open}{string.Join(",", nums.OrderBy(n => n))}{bracket.close}");
        }

        // Strategy 4: Reverse order or shift
        var shifted = elements.Skip(1).Concat(elements.Take(1));
        distractors.Add($"{bracket.open}{string.Join(",", shifted)}{bracket.close}");

        // Pad if needed
        while (distractors.Count < 4 && elements.All(e => int.TryParse(e, out _)))
        {
            var nums = elements.Select(int.Parse).ToList();
            nums[rng.Next(nums.Count)] += rng.Next(-3, 4);
            distractors.Add($"{bracket.open}{string.Join(",", nums.OrderBy(n => n))}{bracket.close}");
        }

        if (distractors.Count < 4) return false; // fallback to generic

        options = distractors.Take(4).Select(d => new DraftOptionDto(d, d == answer.Trim())).ToList();
        return true;
    }

    private static List<DraftOptionDto> GenerateGenericDistractors(string correctAnswer)
    {
        // For text answers, generate variations that look plausible
        var options = new List<DraftOptionDto>
        {
            new(correctAnswer, true),
            new($"Not {correctAnswer}", false),
        };

        // Try to create variations by word manipulation
        var words = correctAnswer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
        {
            // Reverse word order
            options.Add(new DraftOptionDto(string.Join(" ", words.Reverse()), false));
            // Drop last word
            options.Add(new DraftOptionDto(string.Join(" ", words.Take(words.Length - 1)), false));
        }
        else
        {
            options.Add(new DraftOptionDto($"{correctAnswer} (approx.)", false));
            options.Add(new DraftOptionDto($"None of the above", false));
        }

        return options.Take(4).ToList();
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

        // Chinese: 甲, 乙, 丙, 丁
        if (trimmed.Length == 1)
        {
            var idx = "甲乙丙丁".IndexOf(trimmed[0]);
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
            "easy" or "1" or "лёгкий" or "легкий" or "лёгко" or "легко" or "简单" or "容易" => "Easy",
            "medium" or "2" or "средний" or "средне" or "中等" or "一般" => "Medium",
            "hard" or "3" or "difficult" or "сложный" or "сложно" or "трудный" or "困难" or "难" => "Hard",
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
    ///   Russian: "1. В", "1) Б"  Chinese: "1. 甲", "1、丙"
    /// Returns dictionary: questionNumber → answerLetter (normalized to Latin A/B/C/D)
    /// </summary>
    public Dictionary<int, string> ExtractAnswerKeys(string text)
    {
        var keys = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(text)) return keys;

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Pattern: number followed by separator and a single letter answer
        // Matches: "1. C", "1) A", "1-B", "1: D", "1.C", "#1 C", "1、甲"
        var linePattern = new Regex(
            @"#?\s*(\d{1,4})\s*[\.\)\:\-–—、：]\s*([A-DА-Гa-dа-г甲乙丙丁])\b",
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
            var compactPattern = new Regex(@"\b(\d{1,4})([A-DА-Гa-dа-г甲乙丙丁])\b", RegexOptions.IgnoreCase);
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
            @"|第[\d一二三四五六七八九十百]+[章节单元部分课][:\.\-–—：]?\s*(.+)" +  // Chinese: 第X章/节
            @")\s*$",
            RegexOptions.IgnoreCase);

        // Also detect numbered chapter titles (e.g. "1. Sets and Logic", "第一章 集合")
        var numberedHeaderPattern = new Regex(
            @"^\s*(?:(?:Chapter|Глава|Раздел|Тема)\s+)?(\d+(?:\.\d+)?)\s*[:\.\-–—]\s*([A-ZА-Я\u4e00-\u9fff].{3,80})\s*$",
            RegexOptions.IgnoreCase);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var m = headerPattern.Match(line);
            if (m.Success)
            {
                var title = (m.Groups[1].Success ? m.Groups[1].Value
                    : m.Groups[2].Success ? m.Groups[2].Value
                    : m.Groups[3].Value).Trim();
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

    /// <summary>Convert Cyrillic/Chinese answer letters to Latin equivalents</summary>
    private static string NormalizeCyrillicAnswer(string letter)
    {
        return letter switch
        {
            "А" => "A",
            "Б" => "B",
            "В" => "C",  // В (Cyrillic) → C (3rd option)
            "Г" => "D",
            "甲" => "A",
            "乙" => "B",
            "丙" => "C",
            "丁" => "D",
            _ => letter
        };
    }
}
