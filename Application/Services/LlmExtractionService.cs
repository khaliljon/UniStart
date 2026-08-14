using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

public class LlmExtractionService : ILlmExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LlmExtractionService> _logger;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public LlmExtractionService(IConfiguration config, ILogger<LlmExtractionService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

        var section = config.GetSection("LlmExtraction");
        _apiKey = section["ApiKey"];
        _model = section["Model"] ?? "deepseek-chat";
        _baseUrl = (section["BaseUrl"] ?? "https://api.deepseek.com").TrimEnd('/');
    }

    public async Task<List<ExtractedQuestion>> ExtractQuestionsAsync(string text, string? instructions = null, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("LLM extraction not configured (no API key). Skipping.");
            return new List<ExtractedQuestion>();
        }

        try
        {
            const int maxChars = 300_000;
            if (text.Length > maxChars)
            {
                _logger.LogWarning("Text too long for LLM ({Length} chars), truncating to {Max}", text.Length, maxChars);
                text = text[..maxChars];
            }

            _logger.LogInformation("Starting LLM extraction with {Model}, text length: {Length} chars", _model, text.Length);

            var (expectedCount, expectedOptions) = ParseExtractionHints(instructions);
            if (expectedCount.HasValue)
                _logger.LogInformation("Admin context: expecting {Count} questions", expectedCount.Value);
            if (expectedOptions.HasValue)
                _logger.LogInformation("Admin context: expecting {Options} answer options per question", expectedOptions.Value);

            const int batchSize = 15;
            const int maxBatches = 12;
            var allQuestions = new List<ExtractedQuestion>();

            for (int batch = 1; batch <= maxBatches; batch++)
            {
                int remaining = expectedCount.HasValue
                    ? Math.Max(0, expectedCount.Value - allQuestions.Count)
                    : batchSize;
                if (remaining == 0)
                {
                    _logger.LogInformation("Reached expected question count ({Count}) — stopping", expectedCount!.Value);
                    break;
                }
                int thisBatch = Math.Min(batchSize, remaining);

                var alreadyExtracted = allQuestions.Count > 0 ? BuildAlreadyExtractedSummary(allQuestions) : null;
                var result = await CallLlmApiAsync(text, thisBatch, allQuestions.Count, expectedCount, expectedOptions, alreadyExtracted, instructions, ct);
                if (result.Count == 0)
                {
                    _logger.LogInformation("LLM batch {Batch} returned no questions — stopping", batch);
                    break;
                }

                int before = allQuestions.Count;
                allQuestions.AddRange(result);
                allQuestions = DeduplicateQuestions(allQuestions);
                int newlyAdded = allQuestions.Count - before;
                _logger.LogInformation("LLM batch {Batch}: {Raw} returned, {New} new (total {Total})",
                    batch, result.Count, newlyAdded, allQuestions.Count);

                if (newlyAdded == 0) break;
                if (result.Count < thisBatch) break;
            }

            if (expectedCount.HasValue && allQuestions.Count > expectedCount.Value)
            {
                _logger.LogInformation("Trimming {From} → {To} questions to match admin-stated count",
                    allQuestions.Count, expectedCount.Value);
                allQuestions = allQuestions.Take(expectedCount.Value).ToList();
            }

            _logger.LogInformation("LLM extracted {Count} questions total (after dedup)", allQuestions.Count);
            return allQuestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM extraction failed");
            return new List<ExtractedQuestion>();
        }
    }

    private static string BuildAlreadyExtractedSummary(List<ExtractedQuestion> questions)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < questions.Count; i++)
        {
            var qText = questions[i].QuestionText;
            var summary = qText.Length > 80 ? qText[..80] + "…" : qText;
            sb.AppendLine($"  #{i + 1}: {summary}");
        }
        return sb.ToString();
    }

    private static (int? expectedCount, int? expectedOptions) ParseExtractionHints(string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
            return (null, null);

        int? count = null;
        int? options = null;

        var qMatch = Regex.Match(
            instructions,
            @"(\d{1,3})\s*(?:вопрос\w*|сұрақ\w*|задани\w*|задач\w*|question[s]?|item[s]?)",
            RegexOptions.IgnoreCase);
        if (qMatch.Success && int.TryParse(qMatch.Groups[1].Value, out var qn) && qn is > 0 and <= 500)
            count = qn;

        var oMatch = Regex.Match(
            instructions,
            @"(\d{1,2})\s*(?:вариант\w*|ответ\w*|жауап\w*|option[s]?|answer[s]?|choice[s]?)",
            RegexOptions.IgnoreCase);
        if (oMatch.Success && int.TryParse(oMatch.Groups[1].Value, out var on) && on is >= 2 and <= 10)
            options = on;

        return (count, options);
    }

    private List<ExtractedQuestion> DeduplicateQuestions(List<ExtractedQuestion> questions)
    {
        if (questions.Count <= 1) return questions;

        var result = new List<ExtractedQuestion>();

        foreach (var q in questions)
        {
            var normalized = NormalizeForComparison(q.QuestionText);
            bool isDuplicate = false;

            foreach (var existing in result)
            {
                var existingNorm = NormalizeForComparison(existing.QuestionText);

                if (normalized == existingNorm)
                {
                    isDuplicate = true;
                    break;
                }

                var similarity = TrigramSimilarity(normalized, existingNorm);
                if (similarity >= 0.70)
                {
                    _logger.LogDebug("Duplicate detected (similarity={Sim:F2}): '{New}' ≈ '{Existing}'",
                        similarity,
                        q.QuestionText[..Math.Min(60, q.QuestionText.Length)],
                        existing.QuestionText[..Math.Min(60, existing.QuestionText.Length)]);
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
                result.Add(q);
        }

        return result;
    }

    private static string NormalizeForComparison(string text)
    {
        text = Regex.Replace(text, @"^[\s#]*\d+[\.\)\:]?\s*", "");
        text = Regex.Replace(text, @"\s+", " ").Trim().ToLowerInvariant();
        text = Regex.Replace(text, @"[""''«»\(\)]", "");
        return text;
    }

    private static double TrigramSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

        var trigramsA = GetTrigrams(a);
        var trigramsB = GetTrigrams(b);

        if (trigramsA.Count == 0 || trigramsB.Count == 0) return 0;

        int intersection = 0;
        foreach (var t in trigramsA)
        {
            if (trigramsB.Contains(t))
                intersection++;
        }

        int union = trigramsA.Count + trigramsB.Count - intersection;
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static HashSet<string> GetTrigrams(string text)
    {
        var set = new HashSet<string>();
        for (int i = 0; i <= text.Length - 3; i++)
            set.Add(text.Substring(i, 3));
        return set;
    }


    public async Task<ExtractedTheory> ExtractTheoryAsync(string text, string? instructions = null, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("LLM extraction not configured (no API key). Skipping theory extraction.");
            return new ExtractedTheory(new(), new(), new(), new());
        }

        const int maxChars = 200_000;
        if (text.Length > maxChars)
        {
            _logger.LogWarning("Theory text too long for LLM ({Length} chars), truncating to {Max}", text.Length, maxChars);
            text = text[..maxChars];
        }

        _logger.LogInformation("Starting LLM theory extraction with {Model}, text length: {Length} chars", _model, text.Length);

        var lessons = await ExtractLessonsAsync(text, instructions, ct);
        var formulas = await ExtractFormulasAsync(text, instructions, ct);
        var flashcards = await ExtractFlashcardsAsync(text, instructions, ct);
        var strategies = await ExtractStrategiesAsync(text, instructions, ct);

        _logger.LogInformation("Theory extraction done: {L} lessons, {F} formulas, {C} flashcards, {S} strategies",
            lessons.Count, formulas.Count, flashcards.Count, strategies.Count);

        return new ExtractedTheory(lessons, formulas, flashcards, strategies);
    }

    private static string AdminBlock(string? instructions) =>
        string.IsNullOrWhiteSpace(instructions)
            ? ""
            : "\n\n⭐ ADMIN INSTRUCTIONS (highest priority — follow these closely):\n" + instructions.Trim();

    private async Task<List<ExtractedLesson>> ExtractLessonsAsync(string text, string? instructions, CancellationToken ct)
    {
        const string system = @"You extract LESSON content from a theory document for an educational platform.
Return the lesson(s) suitable for a ""Lessons"" section. Preserve the author's structure and ALL substantive content (definitions, explanations, tables, worked steps) as clean GitHub-flavored Markdown.
Rules:
- Usually there is ONE lesson per document — return a single lesson unless the document is clearly split into separate lessons.
- Do NOT include the formulas reference list, flashcards, or exam strategies sections — those are extracted separately.
- Keep math as readable Unicode/Markdown. Do not wrap the whole lesson in code fences.
JSON format: {""lessons"":[{""title"":""..."",""content"":""markdown...""}]}
Return ONLY valid JSON.";
        var user = "Extract the lesson content from the document below." + AdminBlock(instructions) + "\n\nDOCUMENT TEXT:\n" + text;
        var raw = await CallChatAsync(system, user, ct);
        var items = ParseJsonArray<LlmLesson>(raw);
        return items
            .Where(l => !string.IsNullOrWhiteSpace(l.Content))
            .Select(l => new ExtractedLesson(
                string.IsNullOrWhiteSpace(l.Title) ? "Lesson" : l.Title!.Trim(),
                l.Content!.Trim()))
            .ToList();
    }

    private async Task<List<ExtractedFormula>> ExtractFormulasAsync(string text, string? instructions, CancellationToken ct)
    {
        const string system = @"You extract FORMULAS from a theory document for an educational platform's formula reference.
For each formula output a title, the expression as **KaTeX** (no surrounding $ or $$ delimiters), and an optional description (units, variable meanings, common pitfalls).
Rules:
- Convert plain-text math to valid KaTeX. Examples: 'v = d / t' → 'v = \\frac{d}{t}';  'ρ = m / V' → '\\rho = \\frac{m}{V}';  'a^2' → 'a^2';  '×' → '\\times'.
- One entry per distinct formula. Do NOT include lesson prose, flashcards, or strategies.
JSON format: {""formulas"":[{""title"":""..."",""formula"":""KaTeX"",""description"":""...""}]}
Return ONLY valid JSON.";
        var user = "Extract every formula from the document below as KaTeX." + AdminBlock(instructions) + "\n\nDOCUMENT TEXT:\n" + text;
        var raw = await CallChatAsync(system, user, ct);
        var items = ParseJsonArray<LlmFormula>(raw);
        return items
            .Where(f => !string.IsNullOrWhiteSpace(f.Title) && !string.IsNullOrWhiteSpace(f.Formula))
            .Select(f => new ExtractedFormula(
                f.Title!.Trim(),
                f.Formula!.Trim(),
                string.IsNullOrWhiteSpace(f.Description) ? null : f.Description!.Trim()))
            .ToList();
    }

    private async Task<List<ExtractedFlashcard>> ExtractFlashcardsAsync(string text, string? instructions, CancellationToken ct)
    {
        const string system = @"You extract FLASHCARDS from a theory document for an educational platform.
Each flashcard has a FRONT (prompt/question) and BACK (answer). Use the author's existing front/back pairs where present; otherwise create concise recall cards from key facts.
Rules:
- Keep front and back short. Markdown allowed. Do NOT include lesson prose, formulas list, or strategies.
JSON format: {""flashcards"":[{""front"":""..."",""back"":""...""}]}
Return ONLY valid JSON.";
        var user = "Extract the flashcards from the document below." + AdminBlock(instructions) + "\n\nDOCUMENT TEXT:\n" + text;
        var raw = await CallChatAsync(system, user, ct);
        var items = ParseJsonArray<LlmFlashcard>(raw);
        return items
            .Where(c => !string.IsNullOrWhiteSpace(c.Front) && !string.IsNullOrWhiteSpace(c.Back))
            .Select(c => new ExtractedFlashcard(c.Front!.Trim(), c.Back!.Trim()))
            .ToList();
    }

    private async Task<List<ExtractedStrategy>> ExtractStrategiesAsync(string text, string? instructions, CancellationToken ct)
    {
        const string system = @"You extract EXAM STRATEGIES from a theory document for an educational platform.
Each strategy has a title, a one-sentence summary, the full content (markdown), and a category.
Rules:
- category MUST be exactly one of: test-taking, time-management, section-specific, mental.
- Do NOT include lesson prose, formulas, or flashcards.
JSON format: {""strategies"":[{""title"":""..."",""summary"":""one sentence"",""content"":""markdown"",""category"":""test-taking""}]}
Return ONLY valid JSON.";
        var user = "Extract the exam strategies from the document below." + AdminBlock(instructions) + "\n\nDOCUMENT TEXT:\n" + text;
        var raw = await CallChatAsync(system, user, ct);
        var items = ParseJsonArray<LlmStrategy>(raw);
        var allowed = new HashSet<string> { "test-taking", "time-management", "section-specific", "mental" };
        return items
            .Where(s => !string.IsNullOrWhiteSpace(s.Title) && !string.IsNullOrWhiteSpace(s.Content))
            .Select(s => new ExtractedStrategy(
                s.Title!.Trim(),
                string.IsNullOrWhiteSpace(s.Summary) ? s.Title!.Trim() : s.Summary!.Trim(),
                s.Content!.Trim(),
                allowed.Contains((s.Category ?? "").Trim().ToLowerInvariant()) ? s.Category!.Trim().ToLowerInvariant() : "test-taking"))
            .ToList();
    }

    private async Task<string> CallChatAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.1,
            max_tokens = 8192,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/chat/completions") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("LLM theory API returned {StatusCode}: {Body}", response.StatusCode, responseBody[..Math.Min(500, responseBody.Length)]);
            return "";
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody);
        return completion?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
    }

    private List<T> ParseJsonArray<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return new();
        var trimmed = content.Trim();
        try
        {
            if (trimmed.StartsWith("{"))
            {
                var wrapper = JsonSerializer.Deserialize<JsonElement>(trimmed);
                foreach (var prop in wrapper.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                        return JsonSerializer.Deserialize<List<T>>(prop.Value.GetRawText(), _jsonOptions) ?? new();
                }
            }
            if (trimmed.StartsWith("["))
                return JsonSerializer.Deserialize<List<T>>(trimmed, _jsonOptions) ?? new();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse theory JSON array. Raw: {Raw}", trimmed[..Math.Min(300, trimmed.Length)]);
        }
        return new();
    }

    private class LlmLesson
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("content")] public string? Content { get; set; }
    }

    private class LlmFormula
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("formula")] public string? Formula { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }

    private class LlmFlashcard
    {
        [JsonPropertyName("front")] public string? Front { get; set; }
        [JsonPropertyName("back")] public string? Back { get; set; }
    }

    private class LlmStrategy
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("summary")] public string? Summary { get; set; }
        [JsonPropertyName("content")] public string? Content { get; set; }
        [JsonPropertyName("category")] public string? Category { get; set; }
    }

    private async Task<List<ExtractedQuestion>> CallLlmApiAsync(string text, int batchSize, int alreadyCount, int? expectedCount, int? expectedOptions, string? alreadyExtracted, string? instructions, CancellationToken ct)
    {
        var systemPrompt = @"You are an exam-question extraction engine for an educational platform.

INPUT: Raw text extracted from an exam file (PDF / DOCX / OCR). It may contain numbered exam or test questions, sometimes mixed with theory, examples, or instructions.

YOUR TASK: Extract the REAL exam questions exactly as they appear in the source, and output each as a multiple-choice question in JSON.

CORE RULES (apply these UNLESS the ADMIN INSTRUCTIONS below override them):
1. Faithfulness first. Do NOT invent, duplicate, split, or merge questions. One numbered question in the source = exactly one question in the output. If a question has labelled sub-parts (a)/(b) or (1)(2)(3) that belong to the same item, keep it as ONE question.
2. Preserve the original number of answer options EXACTLY as written. If the source question has 5 options (A–E), output 5 options; if it has 4, output 4; if 2, output 2. Only create options when the source question genuinely has none.
3. Mark exactly ONE option as correct. If the source marks the correct answer, use it; otherwise solve the problem to determine it.
4. Keep the original wording and language of each question and its options. Translate ONLY if the ADMIN INSTRUCTIONS ask you to.
5. Fix only obvious OCR/encoding artifacts. Use proper math Unicode where appropriate: ∈ ∉ ⊂ ⊆ ∪ ∩ ∁ ∅ ≤ ≥ ≠ √ π ℤ ℕ ℚ ℝ
6. Skip non-questions: theory, definitions, examples, notes, properties, chapter headers, tables of contents.
7. Keep explanations to one short sentence (or leave empty).
8. Do NOT repeat any question listed as already extracted.

The ADMIN INSTRUCTIONS (provided in the user message, if any) have ABSOLUTE priority over the core rules above. If they state the number of questions, the number of options per question, the language, difficulty, or formatting — follow them precisely.

JSON format (the number of option objects must match the source):
{""questions"":[{""q"":""..."",""options"":[{""text"":""..."",""correct"":false},{""text"":""..."",""correct"":true}],""explanation"":""...""}]}

Return ONLY valid JSON.";

        var userPromptSb = new StringBuilder();
        userPromptSb.AppendLine($"Extract exam questions from the document text below. Return up to {batchSize} questions in this response.");
        userPromptSb.AppendLine("Preserve each question exactly as written, including its original number of answer options. Do NOT split, merge, or invent questions.");
        userPromptSb.AppendLine("Skip theory, definitions, examples and headers.");

        if (expectedCount.HasValue)
        {
            int remaining = Math.Max(0, expectedCount.Value - alreadyCount);
            userPromptSb.AppendLine();
            userPromptSb.AppendLine($"📌 The document is stated to contain EXACTLY {expectedCount.Value} questions. " +
                $"You have already extracted {alreadyCount}. Extract only the next {remaining} (at most {batchSize} now). " +
                "If you cannot find that many GENUINE questions in the source, return fewer — NEVER invent, reword, or duplicate questions to reach a number.");
        }
        if (expectedOptions.HasValue)
        {
            userPromptSb.AppendLine();
            userPromptSb.AppendLine($"📌 Each question is stated to have EXACTLY {expectedOptions.Value} answer options. " +
                $"Output exactly {expectedOptions.Value} options per question, with exactly one marked correct. " +
                "If the source text for a question shows a different number, re-read it carefully — the source almost certainly has the stated count.");
        }

        if (!string.IsNullOrWhiteSpace(instructions))
        {
            userPromptSb.AppendLine();
            userPromptSb.AppendLine("⭐ ADMIN INSTRUCTIONS (highest priority — follow these closely):");
            userPromptSb.AppendLine(instructions.Trim());
        }

        if (!string.IsNullOrWhiteSpace(alreadyExtracted))
        {
            userPromptSb.AppendLine();
            userPromptSb.AppendLine("⚠️ IMPORTANT: The following questions were ALREADY extracted in previous batches. DO NOT include them again:");
            userPromptSb.AppendLine(alreadyExtracted);
            userPromptSb.AppendLine("Only extract NEW questions not in the list above.");
        }

        userPromptSb.AppendLine();
        userPromptSb.AppendLine("DOCUMENT TEXT:");
        userPromptSb.AppendLine(text);

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPromptSb.ToString() }
            },
            temperature = 0.05,
            max_tokens = 8192,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/chat/completions")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("LLM API returned {StatusCode}: {Body}", response.StatusCode, responseBody[..Math.Min(500, responseBody.Length)]);
            return new List<ExtractedQuestion>();
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody);
        var messageContent = completion?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(messageContent))
        {
            _logger.LogWarning("LLM returned empty content");
            return new List<ExtractedQuestion>();
        }

        _logger.LogInformation("LLM raw response ({Length} chars): {Response}", 
            messageContent.Length, messageContent[..Math.Min(300, messageContent.Length)]);

        return ParseLlmResponse(messageContent);
    }

    private List<ExtractedQuestion> ParseLlmResponse(string responseContent)
    {
        var results = new List<ExtractedQuestion>();

        try
        {
            var trimmed = responseContent.Trim();

            List<LlmQuestion>? questions = null;

            if (trimmed.StartsWith("{"))
            {
                var wrapper = JsonSerializer.Deserialize<JsonElement>(trimmed);
                foreach (var prop in wrapper.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        questions = JsonSerializer.Deserialize<List<LlmQuestion>>(prop.Value.GetRawText(), _jsonOptions);
                        break;
                    }
                }
            }

            if (questions == null && trimmed.StartsWith("["))
            {
                questions = JsonSerializer.Deserialize<List<LlmQuestion>>(trimmed, _jsonOptions);
            }

            if (questions == null || questions.Count == 0)
            {
                _logger.LogWarning("Could not parse LLM response into questions. Raw: {Raw}", 
                    trimmed[..Math.Min(300, trimmed.Length)]);
                return results;
            }

            foreach (var q in questions)
            {
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                    continue;

                var options = new List<DraftOptionDto>();
                if (q.Options != null)
                {
                    foreach (var opt in q.Options)
                    {
                        if (!string.IsNullOrWhiteSpace(opt.Text))
                            options.Add(new DraftOptionDto(opt.Text, opt.IsCorrect));
                    }
                }

                results.Add(new ExtractedQuestion(
                    q.QuestionText,
                    options,
                    string.IsNullOrWhiteSpace(q.Explanation) ? null : q.Explanation,
                    null,
                    null
                ));
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse LLM JSON response");
        }

        return results;
    }


    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public MessageContent? Message { get; set; }
    }

    private class MessageContent
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class LlmQuestion
    {
        [JsonPropertyName("q")]
        public string? QuestionText { get; set; }

        [JsonPropertyName("options")]
        public List<LlmOption>? Options { get; set; }

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }
    }

    private class LlmOption
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("correct")]
        public bool IsCorrect { get; set; }
    }
}
