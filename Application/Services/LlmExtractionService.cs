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

            // Strategy: send the FULL text in each request and pull questions in batches.
            // DeepSeek input is 128K tokens (plenty), output limit is 8192 tokens, so we
            // cannot return 50+ questions in one response. We loop, passing a summary of
            // already-extracted questions each round so the model only returns NEW ones,
            // and stop when a round returns nothing new (or a short final batch). The number
            // of questions is driven by the actual document, not a hardcoded count.
            const int batchSize = 15;
            const int maxBatches = 12; // safety cap (~180 questions)
            var allQuestions = new List<ExtractedQuestion>();

            for (int batch = 1; batch <= maxBatches; batch++)
            {
                var alreadyExtracted = allQuestions.Count > 0 ? BuildAlreadyExtractedSummary(allQuestions) : null;
                var result = await CallLlmApiAsync(text, batchSize, alreadyExtracted, instructions, ct);
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

                if (newlyAdded == 0) break;          // model is repeating itself → done
                if (result.Count < batchSize) break;  // short batch → end of document
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

    private async Task<List<ExtractedQuestion>> CallLlmApiAsync(string text, int batchSize, string? alreadyExtracted, string? instructions, CancellationToken ct)
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
