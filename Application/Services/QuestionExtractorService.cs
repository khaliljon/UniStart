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

    public List<ExtractedQuestion> ExtractFromText(string text)
    {
        var questions = new List<ExtractedQuestion>();

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var questionBlocks = SplitIntoQuestionBlocks(text);

        foreach (var block in questionBlocks)
        {
            var q = ParseQuestionBlock(block);
            if (q != null)
            {
                questions.Add(q);
            }
        }

        if (questions.Count < 3)
        {
            var optionAnchored = ExtractByOptionAnchors(text);
            foreach (var q in optionAnchored)
            {
                if (!questions.Any(existing =>
                    IsSimilarText(existing.QuestionText, q.QuestionText)))
                {
                    questions.Add(q);
                }
            }
        }

        var fillInQuestions = ExtractFillInBlankQuestions(questionBlocks, questions);
        questions.AddRange(fillInQuestions);

        _logger.LogInformation("Extracted {Count} questions from text ({Length} chars)", questions.Count, text.Length);
        return questions;
    }

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


    private List<string> SplitIntoQuestionBlocks(string text)
    {
        var blocks = new List<string>();

        var pattern = @"(?:^|\n)\s*(?:" +
            @"\d{1,4}\s*[\.\)\:\、](?:\s|(?=[\u4e00-\u9fff]))" +
            @"|(?:Question|Q|Вопрос|Задание|Задача|Упражнение|题目|问题)\s*\d+[:\.\)：、]\s*" +
            @"|Q\s*\d{1,4}(?=\s)" +
            @"|#\s*\d{1,4}[\.\:\s]" +
            @"|(?=[IVXLC]{2,6}[\.\)]\s)[IVXLC]{2,}[\.\)]\s" +
            @"|第\s*\d{1,4}\s*题[\.\:\、：]?\s*" +
            @"|（\d{1,4}）\s*" +
            @"|\d{1,4}\s*[\.\)\、]\s*(?=[\u4e00-\u9fff(\(（$])" +
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

    private List<ExtractedQuestion> ExtractByOptionAnchors(string text)
    {
        var results = new List<ExtractedQuestion>();
        var rawLines = text.Split('\n');
        var lines = rawLines.SelectMany(l => ExpandInlineOptions(l.Trim())).ToArray();

        int i = 0;
        while (i < lines.Length)
        {
            if (IsOptionLine(lines[i].Trim()))
            {
                var optionLines = new List<string>();
                var optStart = i;
                while (i < lines.Length && IsOptionLine(lines[i].Trim()))
                {
                    optionLines.Add(lines[i].Trim());
                    i++;
                }

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

                var questionLines = new List<string>();
                for (int j = optStart - 1; j >= Math.Max(0, optStart - 8); j--)
                {
                    var line = lines[j].Trim();
                    if (string.IsNullOrWhiteSpace(line)) break;
                    if (IsSectionHeader(line)) break;
                    questionLines.Insert(0, line);
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


    private ExtractedQuestion? ParseQuestionBlock(string block)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(l => l.Trim())
                         .Where(l => !string.IsNullOrWhiteSpace(l))
                         .ToList();

        if (lines.Count < 2) return null;

        lines = lines.SelectMany(ExpandInlineOptions).ToList();

        var questionLines = new List<string>();
        var optionLines = new List<string>();
        var answerLine = "";
        var explanationLine = "";
        string? metaDifficulty = null;
        bool inExplanation = false;

        bool inOptions = false;
        foreach (var line in lines)
        {
            if (!inOptions && TryParseMetaHeader(line, out var headerDifficulty))
            {
                if (headerDifficulty != null) metaDifficulty = headerDifficulty;
                continue;
            }

            if (IsSectionHeader(line) || IsDifficultyBanner(line))
            {
                inExplanation = false;
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
                explanationLine = explanationLine.Length == 0 ? line : explanationLine + " " + line;
            }
            else if (!inOptions)
            {
                var cleaned = StripLeadingNumber(line);
                if (!string.IsNullOrWhiteSpace(cleaned))
                    questionLines.Add(cleaned);
            }
            else
            {
                if (optionLines.Count > 0)
                    optionLines[optionLines.Count - 1] += " " + line;
            }
        }

        var questionText = string.Join(" ", questionLines).Trim();
        if (string.IsNullOrWhiteSpace(questionText))
            return null;

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


    private static bool IsOptionLine(string line)
    {
        return Regex.IsMatch(line,
            @"^(?:" +
            @"[A-Ea-e]\s*[\.\)\:\、]" +
            @"|[\(（][A-Ea-e][\)）]" +
            @"|[А-Га-г]\s*[\.\)\:\、]" +
            @"|[\(（][А-Га-г][\)）]" +
            @"|[甲乙丙丁]\s*[\.\)\:\、．）]" +
            @"|[\(（][甲乙丙丁][\)）]" +
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

    private static bool IsCjkQuestionStart(string line)
    {
        return Regex.IsMatch(line,
            @"^\d{1,4}\s*[\.\)\、．）]\s*[\u4e00-\u9fff(\(（$\\]");
    }

    private static bool IsSectionHeader(string line)
    {
        return Regex.IsMatch(line,
            @"^(?:Exercise[s]?|Self[- ]Test|Section|Chapter|Part|" +
            @"Раздел|Глава|Упражнени[ея]|Тест|Контрольн)" +
            @"\s*\d",
            RegexOptions.IgnoreCase)
            || Regex.IsMatch(line, @"^第[一二三四五六七八九十百\d]+[章节单元部分课]");
    }

    private static bool IsTextbookContent(string block)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var firstLine = lines.FirstOrDefault()?.Trim() ?? "";
        var stripped = StripLeadingNumber(firstLine);

        if (ContainsOptionSet(block)) return false;

        if (Regex.IsMatch(stripped, @"^(?:定义|例\s*\d|注\s*\d|性质|定理|推论|小结|证明|解[题答：:]|我们把|由所有|即)",
            RegexOptions.IgnoreCase))
            return true;

        if (Regex.IsMatch(stripped, @"第\s*\d+\s*章|第\s*\d+\s*节|空间\S+的\S+|平面向量|空间向量|导数|排列|组合|随机变量|概率分布",
            RegexOptions.None))
            return true;

        if (Regex.IsMatch(stripped, @"^(?:我们|由|即|例如|因为|所以|由此|综上|特别地|一般地|显然|关于|用符号|写出集合)",
            RegexOptions.None))
            return true;

        if (Regex.IsMatch(stripped, @"^(?:集合|元素|自然数|整数|有理数|实数|不等式|区间|向量)",
            RegexOptions.None) && !Regex.IsMatch(block, @"\(\s*\)", RegexOptions.None))
            return true;

        if (Regex.IsMatch(block, @"填空|填入", RegexOptions.None))
            return true;

        if (Regex.IsMatch(block, @"\(definition\)|\(subset\)|\(union set\)|\(empty set\)|\(equality\)|\(interval\)|\(inequality\)",
            RegexOptions.IgnoreCase))
            return true;

        if (Regex.IsMatch(stripped, @"^(?:Определение|Пример|Примечание|Свойство|Теорема|Доказательство|Следствие|Замечание)",
            RegexOptions.IgnoreCase))
            return true;

        if (block.Length > 800 && !ContainsOptionSet(block))
            return true;

        if (!Regex.IsMatch(block, @"[?？]|\(\s*\)|则下列|下列.*正确|等于|求|解不等式", RegexOptions.None)
            && !ContainsOptionSet(block)
            && block.Length > 100)
            return true;

        if (Regex.IsMatch(block, @"第\d+章.*第\d+章", RegexOptions.Singleline))
            return true;

        return false;
    }

    private static bool ContainsOptionSet(string text)
    {
        var hasA = Regex.IsMatch(text, @"(?:^|\n)\s*(?:[Aa][\.\)\:\、]|[\(（][Aa][\)）]|甲\s*[\.\)\:\、．）])(?:\s|[\u4e00-\u9fff])", RegexOptions.Multiline);
        var hasB = Regex.IsMatch(text, @"(?:^|\n|\s{2,})(?:[Bb][\.\)\:\、]|[\(（][Bb][\)）]|乙\s*[\.\)\:\、．）])(?:\s|[\u4e00-\u9fff])", RegexOptions.Multiline);
        return hasA && hasB;
    }

    private static IEnumerable<string> ExpandInlineOptions(string line)
    {
        if (!IsOptionLine(line))
        {
            yield return line;
            yield break;
        }

        var optPattern = @"(?:^|\s)([A-Ea-eА-Га-г])\s*[\.\)\:\、]\s";
        var optMatches = Regex.Matches(line, optPattern);

        if (optMatches.Count < 2)
        {
            yield return line;
            yield break;
        }

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

            if (lines.Any(l => IsOptionLine(l))) continue;

            var firstLine = StripLeadingNumber(lines[0]);

            if (alreadyExtracted.Any(q => IsSimilarText(q.QuestionText, firstLine)))
                continue;

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
                    instructionLines.Add(StripLeadingNumber(line));
                }
            }

            var instruction = string.Join(" ", instructionLines).Trim();

            if (subParts.Count > 0 && !string.IsNullOrWhiteSpace(instruction))
            {
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
                var fullText = string.Join(" ", lines.Select(StripLeadingNumber)).Trim();
                results.Add(new ExtractedQuestion(
                    fullText, new List<DraftOptionDto>(), null, null, null));
            }
        }

        return results;
    }


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

    private static bool HasCorrectMarker(string line) =>
        Regex.IsMatch(line, @"[✓✔☑✅]") ||
        Regex.IsMatch(line, @"\(\s*(?:correct|right|правильн\w*|верн\w*)\s*\)\s*$", RegexOptions.IgnoreCase);

    private static string StripCorrectMarker(string line) =>
        Regex.Replace(
            Regex.Replace(line, @"\s*[✓✔☑✅]\s*", " "),
            @"\s*\(\s*(?:correct|right|правильн\w*|верн\w*)\s*\)\s*$", "", RegexOptions.IgnoreCase).Trim();

    private static bool TryParseMetaHeader(string line, out string? difficulty)
    {
        difficulty = null;

        var isQId = Regex.IsMatch(line, @"^Q\s*\d{1,4}\b", RegexOptions.IgnoreCase);
        var isMetaKeyword = Regex.IsMatch(line,
            @"^(?:Difficulty|Source|Источник|Сложность)\s*[:：]", RegexOptions.IgnoreCase);
        var diffMatch = Regex.Match(line,
            @"\bDifficulty\s*[:：]\s*(Easy|Medium|Hard|Лёгк\w*|Легк\w*|Средн\w*|Сложн\w*)",
            RegexOptions.IgnoreCase);

        if (!isQId && !isMetaKeyword) return false;

        if (diffMatch.Success)
        {
            var d = diffMatch.Groups[1].Value.ToLowerInvariant();
            difficulty = (d.StartsWith("eas") || d.StartsWith("лёг") || d.StartsWith("лег")) ? "Easy"
                       : (d.StartsWith("har") || d.StartsWith("слож")) ? "Hard"
                       : "Medium";
        }

        return true;
    }

    private static bool IsDifficultyBanner(string line) =>
        line.Length < 60 &&
        Regex.IsMatch(line, @"^[\s\W]*(?:EASY|MEDIUM|HARD|NUET)[\sA-Za-z/\-–—]*QUESTIONS?\b",
            RegexOptions.IgnoreCase);

    private static string StripLeadingNumber(string line)
    {
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
        var match = Regex.Match(line,
            @"^(?:[\(（]?\s*([A-Ea-eА-Га-г甲乙丙丁])\s*[\.\)\:\、．）]?)");
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

    private static string? ExtractSolutionValue(string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        if (IsAnswerLine(line))
        {
            var letterOnly = Regex.Match(line,
                @"(?:Answer|Correct|Key|Ответ|Правильный|答案|正确答案|参考答案)[：:\s]+([A-Ea-eА-Га-г甲乙丙丁])\s*$",
                RegexOptions.IgnoreCase);
            if (letterOnly.Success) return null;
        }

        var value = Regex.Replace(line,
            @"^(?:Explanation|Explain|Solution|Решение|Объяснение|Пояснение|Rationale|Hint|Answer|Correct\s*(?:answer)?|Key|Ответ|Правильный(?:\s*ответ)?|答案|正确答案|参考答案|解析|解答|解题思路|详解|提示)[：:\s]*",
            "", RegexOptions.IgnoreCase).Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static List<DraftOptionDto> GenerateDistractors(string correctAnswer)
    {
        var options = new List<DraftOptionDto>();
        var rng = new Random(correctAnswer.GetHashCode());

        if (TryGenerateNumericDistractors(correctAnswer, rng, out var numericOptions))
        {
            options = numericOptions;
        }
        else if (TryGenerateSetDistractors(correctAnswer, rng, out var setOptions))
        {
            options = setOptions;
        }
        else
        {
            options = GenerateGenericDistractors(correctAnswer);
        }

        return options.OrderBy(o => rng.Next()).ToList();
    }

    private static bool TryGenerateNumericDistractors(string answer, Random rng, out List<DraftOptionDto> options)
    {
        options = new List<DraftOptionDto>();

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
                var factor = 1 + (rng.NextDouble() * 0.4 - 0.2);
                variation = strategy switch
                {
                    0 => value * factor,
                    1 => value + (value > 0 ? rng.Next(1, (int)Math.Max(2, Math.Abs(value) * 0.3)) : -rng.Next(1, (int)Math.Max(2, Math.Abs(value) * 0.3))),
                    2 => value * (strategy % 2 == 0 ? 2 : 0.5),
                    _ => -value
                };
            }

            string formatted;
            if (value == Math.Floor(value) && !answer.Contains(".") && !answer.Contains(","))
                formatted = ((int)Math.Round(variation)).ToString();
            else
                formatted = Math.Round(variation, 2).ToString(System.Globalization.CultureInfo.InvariantCulture);

            distractors.Add(formatted);
        }

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

        if (elements.Count > 2)
        {
            var reduced = elements.ToList();
            reduced.RemoveAt(rng.Next(reduced.Count));
            distractors.Add($"{bracket.open}{string.Join(",", reduced)}{bracket.close}");
        }

        if (elements.All(e => int.TryParse(e, out _)))
        {
            var nums = elements.Select(int.Parse).ToList();
            var extra = nums.Max() + rng.Next(1, 4);
            var expanded = nums.Append(extra).OrderBy(n => n).Select(n => n.ToString());
            distractors.Add($"{bracket.open}{string.Join(",", expanded)}{bracket.close}");
        }

        if (elements.All(e => int.TryParse(e, out _)))
        {
            var nums = elements.Select(int.Parse).ToList();
            var idx = rng.Next(nums.Count);
            nums[idx] = nums[idx] + rng.Next(1, 4);
            distractors.Add($"{bracket.open}{string.Join(",", nums.OrderBy(n => n))}{bracket.close}");
        }

        var shifted = elements.Skip(1).Concat(elements.Take(1));
        distractors.Add($"{bracket.open}{string.Join(",", shifted)}{bracket.close}");

        while (distractors.Count < 4 && elements.All(e => int.TryParse(e, out _)))
        {
            var nums = elements.Select(int.Parse).ToList();
            nums[rng.Next(nums.Count)] += rng.Next(-3, 4);
            distractors.Add($"{bracket.open}{string.Join(",", nums.OrderBy(n => n))}{bracket.close}");
        }

        if (distractors.Count < 4) return false;

        options = distractors.Take(4).Select(d => new DraftOptionDto(d, d == answer.Trim())).ToList();
        return true;
    }

    private static List<DraftOptionDto> GenerateGenericDistractors(string correctAnswer)
    {
        var options = new List<DraftOptionDto>
        {
            new(correctAnswer, true),
            new($"Not {correctAnswer}", false),
        };

        var words = correctAnswer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
        {
            options.Add(new DraftOptionDto(string.Join(" ", words.Reverse()), false));
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
        var na = Regex.Replace(a.ToLower(), @"\s+", " ").Trim();
        var nb = Regex.Replace(b.ToLower(), @"\s+", " ").Trim();
        if (na.Length < 10 || nb.Length < 10) return na == nb;
        return na.StartsWith(nb[..Math.Min(40, nb.Length)]) ||
               nb.StartsWith(na[..Math.Min(40, na.Length)]);
    }

    private static int ParseCorrectAnswer(string? answer, string[] optionTexts)
    {
        if (string.IsNullOrWhiteSpace(answer)) return -1;

        var trimmed = answer.Trim().ToUpper();

        if (trimmed.Length == 1 && trimmed[0] >= 'A' && trimmed[0] <= 'E')
            return trimmed[0] - 'A';

        if (trimmed.Length == 1)
        {
            var idx = "АБВГ".IndexOf(trimmed[0]);
            if (idx >= 0) return idx;
        }

        if (trimmed.Length == 1)
        {
            var idx = "甲乙丙丁".IndexOf(trimmed[0]);
            if (idx >= 0) return idx;
        }

        if (int.TryParse(trimmed, out var num) && num >= 1 && num <= 5)
            return num - 1;

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

    public Dictionary<int, string> ExtractAnswerKeys(string text)
    {
        var keys = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(text)) return keys;

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

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

    public List<(string Title, int StartQuestion, int EndQuestion)> DetectTopicSections(string text)
    {
        var sections = new List<(string Title, int LineIndex)>();
        if (string.IsNullOrWhiteSpace(text)) return new();

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = text.Split('\n');

        var headerPattern = new Regex(
            @"^\s*(?:" +
            @"(?:Chapter|Section|Topic|Unit|Part|Lesson)\s+[\d\.]+[:\.\-–—]\s*(.+)" +
            @"|(?:Глава|Раздел|Тема|Часть|Урок|Модуль)\s+[\d\.]+[:\.\-–—]\s*(.+)" +
            @"|第[\d一二三四五六七八九十百]+[章节单元部分课][:\.\-–—：]?\s*(.+)" +
            @")\s*$",
            RegexOptions.IgnoreCase);

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
                result.Add((sections[s].Title, 0, 0));
            }
        }

        _logger.LogInformation("Detected {Count} topic sections in text", result.Count);
        return result;
    }

    private static string NormalizeCyrillicAnswer(string letter)
    {
        return letter switch
        {
            "А" => "A",
            "Б" => "B",
            "В" => "C",
            "Г" => "D",
            "甲" => "A",
            "乙" => "B",
            "丙" => "C",
            "丁" => "D",
            _ => letter
        };
    }
}
