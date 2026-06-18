using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

/// <summary>
/// LLM-based question extraction using DeepSeek or OpenAI-compatible API.
/// Sends raw text (e.g. OCR output) to an LLM with a structured prompt
/// and parses the response into ExtractedQuestion objects.
/// 
/// Config (appsettings.json):
///   "LlmExtraction": {
///     "Provider": "deepseek",        // or "openai"
///     "ApiKey": "sk-...",
///     "Model": "deepseek-chat",      // or "gpt-4o-mini"
///     "BaseUrl": "https://api.deepseek.com"
///   }
/// </summary>
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
            // Truncate very long texts to stay within context window
            const int maxChars = 300_000;
            if (text.Length > maxChars)
            {
                _logger.LogWarning("Text too long for LLM ({Length} chars), truncating to {Max}", text.Length, maxChars);
                text = text[..maxChars];
            }

            _logger.LogInformation("Starting LLM extraction with {Model}, text length: {Length} chars", _model, text.Length);

            // Strategy: send the FULL text in each request but ask for different question ranges.
            // DeepSeek input is 128K tokens (plenty for 10K chars), output limit is 8192 tokens.
            // Each batch asks for ~15 questions. We always try at least 2 batches.
            // After batch 2 we pass the already-extracted question summaries to avoid duplicates.

            const int batchSize = 15;
            var allQuestions = new List<ExtractedQuestion>();

            // Batch 1: questions 1..15
            _logger.LogInformation("LLM batch 1: extracting questions 1-{Max} from full text", batchSize);
            var batch1 = await CallLlmApiAsync(text, 1, batchSize, null, instructions, ct);
            _logger.LogInformation("LLM batch 1 yielded {Count} questions", batch1.Count);
            allQuestions.AddRange(batch1);

            // Always try batch 2 if batch 1 found anything
            if (batch1.Count > 0)
            {
                int nextStart = allQuestions.Count + 1;
                int nextEnd = nextStart + batchSize - 1;
                _logger.LogInformation("LLM batch 2: extracting questions {Start}-{End} from full text", nextStart, nextEnd);
                // Pass summaries of already-extracted questions to avoid duplicates
                var alreadyExtracted = BuildAlreadyExtractedSummary(allQuestions);
                var batch2 = await CallLlmApiAsync(text, nextStart, nextEnd, alreadyExtracted, instructions, ct);
                _logger.LogInformation("LLM batch 2 yielded {Count} questions", batch2.Count);
                allQuestions.AddRange(batch2);

                // Batch 3 only if batch 2 found >= batchSize results (i.e. there might be more)
                if (batch2.Count >= batchSize)
                {
                    nextStart = allQuestions.Count + 1;
                    nextEnd = nextStart + batchSize - 1;
                    alreadyExtracted = BuildAlreadyExtractedSummary(allQuestions);
                    _logger.LogInformation("LLM batch 3: extracting questions {Start}-{End} from full text", nextStart, nextEnd);
                    var batch3 = await CallLlmApiAsync(text, nextStart, nextEnd, alreadyExtracted, instructions, ct);
                    _logger.LogInformation("LLM batch 3 yielded {Count} questions", batch3.Count);
                    allQuestions.AddRange(batch3);
                }
            }

            // Deduplicate questions by normalized text similarity
            var beforeDedup = allQuestions.Count;
            allQuestions = DeduplicateQuestions(allQuestions);
            if (allQuestions.Count < beforeDedup)
                _logger.LogInformation("Deduplication removed {Removed} duplicate(s): {Before} → {After}",
                    beforeDedup - allQuestions.Count, beforeDedup, allQuestions.Count);

            _logger.LogInformation("LLM extracted {Count} questions total (after dedup)", allQuestions.Count);
            return allQuestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM extraction failed");
            return new List<ExtractedQuestion>();
        }
    }

    /// <summary>
    /// Build a short summary of already-extracted questions so the LLM can skip them.
    /// </summary>
    private static string BuildAlreadyExtractedSummary(List<ExtractedQuestion> questions)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < questions.Count; i++)
        {
            // Take first 80 chars of each question text
            var qText = questions[i].QuestionText;
            var summary = qText.Length > 80 ? qText[..80] + "…" : qText;
            sb.AppendLine($"  #{i + 1}: {summary}");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Remove near-duplicate questions based on normalized text comparison.
    /// Uses Jaccard similarity on character trigrams.
    /// </summary>
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

                // Check exact normalized match
                if (normalized == existingNorm)
                {
                    isDuplicate = true;
                    break;
                }

                // Check trigram Jaccard similarity (threshold 0.7 = ~70% overlap)
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
        // Remove leading question numbers like "1.", "1)", "#1", "Вопрос 1:"
        text = Regex.Replace(text, @"^[\s#]*\d+[\.\)\:]?\s*", "");
        // Collapse whitespace, lowercase, remove punctuation that doesn't affect meaning
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

        // Jaccard: |A ∩ B| / |A ∪ B|
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

    private async Task<List<ExtractedQuestion>> CallLlmApiAsync(string text, int startNum, int endNum, string? alreadyExtracted, string? instructions, CancellationToken ct)
    {
        var systemPrompt = @"You are a math exam question extractor for a Chinese-to-Russian educational platform.

INPUT: OCR text from a scanned Chinese math textbook (about 30 pages). It contains ~30 numbered exam questions mixed with textbook theory, definitions, and examples.

YOUR TASK: Find ALL numbered exam/test questions in the text. This includes:
- Multiple-choice questions (选择题) with A/B/C/D options
- Fill-in-the-blank questions (填空题) 
- Computation/proof questions (解答题/计算题)

For EVERY question you find, output it as multiple-choice with exactly 4 options (A, B, C, D):
- If the original already has A/B/C/D options, keep them
- If the original is fill-in-the-blank or computation, CREATE 4 plausible options (one correct, three wrong but mathematically plausible)
- If a question has multiple sub-parts (e.g. (1) ... (2) ... (3) ...), extract EACH sub-part as a SEPARATE question with its own 4 options.

SKIP: textbook definitions (定义), examples (例), notes (注), properties (性质), chapter headers, table of contents.

RULES:
1. FIX OCR errors: 'e'/'€' → '∈', '¢' → '∉', 'ixeZ|' → '{x ∈ Z |', 'U' between sets → '∪', 'N' between sets → '∩', '[,'/(',' before U → '∁', 'ix|'/'1x|' → '{x|'
2. Translate to clean Russian. Use math Unicode: ∈ ∉ ⊂ ⊆ ∪ ∩ ∁ ∅ ≤ ≥ ≠ √ π ℤ ℕ ℚ ℝ
3. Exactly 4 options per question. Mark exactly ONE as correct.
4. Solve each problem yourself carefully to determine the correct answer.
5. Keep explanations to 1 short sentence.
6. The text has approximately 30 questions total. Extract questions " + startNum + @" through " + endNum + @" (by order of appearance in the text).
7. DO NOT repeat any question that was already extracted in a previous batch.

JSON format:
{""questions"":[{""q"":""Текст"",""options"":[{""text"":""вариант"",""correct"":false},{""text"":""вариант"",""correct"":true},{""text"":""вариант"",""correct"":false},{""text"":""вариант"",""correct"":false}],""explanation"":""Пояснение""}]}

Return ONLY valid JSON.";

        var userPromptSb = new StringBuilder();
        userPromptSb.AppendLine($"Extract exam questions #{startNum} through #{endNum} from this OCR text.");
        userPromptSb.AppendLine("Include ALL question types (multiple-choice, fill-in-blank, computation) — convert non-MCQ to MCQ format with 4 options.");
        userPromptSb.AppendLine("If a question has sub-parts like (1)(2)(3)..., split each sub-part into a separate question.");
        userPromptSb.AppendLine("Skip textbook theory/definitions/examples.");

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
        userPromptSb.AppendLine("OCR TEXT:");
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

        // Parse the chat completion response
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
            // The response might be wrapped in a JSON object with a "questions" key
            // or it might be a direct array
            var trimmed = responseContent.Trim();

            List<LlmQuestion>? questions = null;

            // Try parsing as { "questions": [...] } wrapper
            if (trimmed.StartsWith("{"))
            {
                var wrapper = JsonSerializer.Deserialize<JsonElement>(trimmed);
                // Find the first array property
                foreach (var prop in wrapper.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        questions = JsonSerializer.Deserialize<List<LlmQuestion>>(prop.Value.GetRawText(), _jsonOptions);
                        break;
                    }
                }
            }

            // Try as direct array
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

    // ═══════════════════════════════════════════════════════
    //  JSON DTOs for LLM API communication
    // ═══════════════════════════════════════════════════════

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
