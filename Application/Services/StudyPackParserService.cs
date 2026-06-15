using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

/// <summary>
/// LLM-based parser that turns a raw study-pack file (markdown/plain text) into the
/// normalized <see cref="IngestContentDto"/> (Variant B: Topic = concept).
///
/// The whole pedagogical mapping lives in the system prompt:
///   - Skill   = file title
///   - Topic   = ONE concept (PART-1 numbered concept). "PART 1/2" is NOT a level.
///   - Lesson  = PART-1 theory of that concept (markdown, tables allowed)
///   - Formulas= KaTeX formulas of that concept
///   - Questions = PART-2 + NUET-level questions CLASSIFIED onto the concept they test
///
/// Reuses the same DeepSeek/OpenAI-compatible config as LlmExtractionService
/// ("LlmExtraction" section in appsettings.json).
/// </summary>
public class StudyPackParserService : IStudyPackParserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StudyPackParserService> _logger;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public StudyPackParserService(IConfiguration config, ILogger<StudyPackParserService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

        var section = config.GetSection("LlmExtraction");
        _apiKey = section["ApiKey"];
        _model = section["Model"] ?? "deepseek-chat";
        _baseUrl = (section["BaseUrl"] ?? "https://api.deepseek.com").TrimEnd('/');
    }

    public async Task<IngestContentDto> ParseAsync(
        string text, string examTypeCode, string examSectionName, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("LLM is not configured (no API key in LlmExtraction:ApiKey).");
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Empty study-pack text.", nameof(text));

        const int maxChars = 300_000;
        if (text.Length > maxChars)
        {
            _logger.LogWarning("Study-pack too long ({Length} chars), truncating to {Max}", text.Length, maxChars);
            text = text[..maxChars];
        }

        var systemPrompt = BuildSystemPrompt(examTypeCode, examSectionName);

        var userSb = new StringBuilder();
        userSb.AppendLine($"ExamType = \"{examTypeCode}\"");
        userSb.AppendLine($"ExamSection = \"{examSectionName}\"");
        userSb.AppendLine("Parse the following study-pack file into the JSON schema described in the system prompt.");
        userSb.AppendLine();
        userSb.AppendLine("STUDY-PACK TEXT:");
        userSb.AppendLine(text);

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userSb.ToString() }
            },
            temperature = 0.0,
            max_tokens = 8192,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/chat/completions")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        _logger.LogInformation("Parsing study-pack ({Length} chars) with {Model}", text.Length, _model);

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var snippet = responseBody[..Math.Min(500, responseBody.Length)];
            _logger.LogError("LLM API {Status}: {Body}", response.StatusCode, snippet);
            throw new InvalidOperationException($"LLM API returned {(int)response.StatusCode} {response.StatusCode}: {snippet}");
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody);
        var messageContent = completion?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(messageContent))
            throw new InvalidOperationException("LLM returned empty content.");

        return ParseResponse(messageContent, examTypeCode, examSectionName);
    }

    private static string BuildSystemPrompt(string examTypeCode, string examSectionName)
    {
        return $@"You are a study-pack parser for an educational platform.

GOAL: Convert ONE study-pack file into a strict JSON object. The hierarchy is FIXED:
ExamType = ""{examTypeCode}""
ExamSection = ""{examSectionName}""
Skill = the file's main title (strip any "" — Study Pack"" suffix).
Topic = ONE CONCEPT. Do NOT use ""PART 1"" / ""PART 2"" as a level — they are just theory vs practice of the SAME concepts.

HOW TO BUILD TOPICS (this is the most important rule):
1. Read PART 1. Each numbered concept (1., 2., 3., …) becomes ONE topic. Its theory text becomes that topic's ""lessonContent"" (keep as markdown; keep tables as markdown tables). Any formulas become ""formulas"".
2. Read PART 2 (and any ""NUET-Level Questions"" block at the end). For EVERY question, decide WHICH concept (topic) it tests, and attach it to that topic's ""questions"". This is a CLASSIFICATION task: match the question to the most relevant PART-1 concept by its content. Do NOT create separate topics for ""SECTION A/B/C/D"".
3. If a question genuinely fits no concept, attach it to the closest one; only as a last resort create a topic named ""Mixed Problems"".

QUESTION RULES:
- Provide options as an array; each has ""text"" and ""isCorrect"" (boolean). Mark EXACTLY ONE correct.
- The correct answer is the one marked with ✓ in the source. If not marked but a Working/solution is given, derive the correct option from it.
- Put the Working/solution text into ""explanation"" (1–3 sentences is fine).
- Keep math as Unicode or KaTeX as it appears.
- ""difficulty"" is one of: Easy, Medium, Hard (best guess; default Medium).
- Preserve original ordering via ""sortOrder"" (questions: use their Q-number; topics: their concept order).

OUTPUT JSON SHAPE (return ONLY this object, no prose):
{{
  ""examTypeCode"": ""{examTypeCode}"",
  ""examSectionName"": ""{examSectionName}"",
  ""skillName"": ""..."",
  ""topics"": [
    {{
      ""name"": ""concept name"",
      ""sortOrder"": 1,
      ""lessonContent"": ""markdown theory or null"",
      ""formulas"": [
        {{ ""title"": ""..."", ""formula"": ""KaTeX"", ""description"": ""... or null"", ""sortOrder"": 0 }}
      ],
      ""questions"": [
        {{
          ""text"": ""question text"",
          ""options"": [
            {{ ""text"": ""A) ..."", ""isCorrect"": false }},
            {{ ""text"": ""B) ..."", ""isCorrect"": true }}
          ],
          ""explanation"": ""working/solution or null"",
          ""hint"": null,
          ""difficulty"": ""Medium"",
          ""sortOrder"": 1
        }}
      ]
    }}
  ]
}}

Return ONLY valid JSON.";
    }

    private IngestContentDto ParseResponse(string responseContent, string examTypeCode, string examSectionName)
    {
        var trimmed = responseContent.Trim();
        // Strip ```json fences if the model added them despite json_object mode.
        if (trimmed.StartsWith("```"))
        {
            var firstNl = trimmed.IndexOf('\n');
            if (firstNl >= 0) trimmed = trimmed[(firstNl + 1)..];
            if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];
            trimmed = trimmed.Trim();
        }

        IngestContentDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<IngestContentDto>(trimmed, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse study-pack LLM JSON. Raw: {Raw}",
                trimmed[..Math.Min(400, trimmed.Length)]);
            throw new InvalidOperationException("LLM returned malformed JSON.", ex);
        }

        if (dto == null)
            throw new InvalidOperationException("LLM returned null payload.");

        // Trust our own exam context over whatever the model echoed back.
        var topics = dto.Topics ?? new List<IngestTopicDto>();
        return dto with
        {
            ExamTypeCode = examTypeCode,
            ExamSectionName = examSectionName,
            Topics = topics
        };
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public MessageContent? Message { get; set; }
    }

    private sealed class MessageContent
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
