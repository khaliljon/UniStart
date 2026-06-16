using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniStart.Application.DTOs;
using UniStart.Application.Exceptions;
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

        _logger.LogInformation("Parsing study-pack ({Length} chars) with {Model}", text.Length, _model);
        var messageContent = await CallLlmAsync(systemPrompt, userSb.ToString(), ct);
        return ParseResponse(messageContent, examTypeCode, examSectionName);
    }

    public async Task<IReadOnlyList<IngestContentDto>> ParseTsaPairAsync(
        string questionsText, string answersText, string examTypeCode, string examSectionName,
        IReadOnlyList<string> unitSkillNames, IReadOnlyDictionary<string, string>? unitGlossary = null,
        CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("LLM is not configured (no API key in LlmExtraction:ApiKey).");
        if (string.IsNullOrWhiteSpace(questionsText))
            throw new ArgumentException("Empty TSA questions text.", nameof(questionsText));

        // Cap each side so the combined prompt stays within budget.
        const int maxChars = 150_000;
        if (questionsText.Length > maxChars) questionsText = questionsText[..maxChars];
        if (answersText.Length > maxChars) answersText = answersText[..maxChars];

        var units = unitSkillNames is { Count: > 0 }
            ? unitSkillNames
            : Enumerable.Range(1, 7).Select(i => $"Unit {i}").ToList();

        var systemPrompt = BuildTsaSystemPrompt(examTypeCode, examSectionName, units, unitGlossary);

        var userSb = new StringBuilder();
        userSb.AppendLine($"ExamType = \"{examTypeCode}\"");
        userSb.AppendLine($"ExamSection = \"{examSectionName}\"");
        userSb.AppendLine($"Allowed units (skillName must be EXACTLY one of these): {string.Join(", ", units.Select(u => $"\"{u}\""))}");
        userSb.AppendLine();
        userSb.AppendLine("QUESTIONS FILE:");
        userSb.AppendLine(questionsText);
        userSb.AppendLine();
        userSb.AppendLine("ANSWER-KEY FILE:");
        userSb.AppendLine(answersText);

        _logger.LogInformation("Parsing TSA pair (Q {Q} chars + A {A} chars) into {N} units with {Model}",
            questionsText.Length, answersText.Length, units.Count, _model);

        var messageContent = await CallLlmAsync(systemPrompt, userSb.ToString(), ct);
        return ParseTsaResponse(messageContent, examTypeCode, examSectionName, units);
    }

    /// <summary>
    /// Single OpenAI/DeepSeek-compatible chat completion call returning the message
    /// content. Surfaces a 402 balance error as <see cref="LlmPaymentRequiredException"/>.
    /// </summary>
    private async Task<string> CallLlmAsync(string systemPrompt, string userContent, CancellationToken ct)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userContent }
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

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var snippet = responseBody[..Math.Min(500, responseBody.Length)];
            _logger.LogError("LLM API {Status}: {Body}", response.StatusCode, snippet);
            // 402 Payment Required ("Insufficient Balance") is fatal for a batch:
            // surface it as a distinct type so a Drive sync can stop immediately
            // instead of failing every remaining file the same way.
            if ((int)response.StatusCode == 402)
                throw new LlmPaymentRequiredException(
                    $"LLM API returned 402 Payment Required (insufficient balance): {snippet}");
            throw new InvalidOperationException($"LLM API returned {(int)response.StatusCode} {response.StatusCode}: {snippet}");
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody);
        var messageContent = completion?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(messageContent))
            throw new InvalidOperationException("LLM returned empty content.");

        return messageContent;
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

    private static string BuildTsaSystemPrompt(
        string examTypeCode, string examSectionName, IReadOnlyList<string> units,
        IReadOnlyDictionary<string, string>? unitGlossary = null)
    {
        var unitList = string.Join(", ", units.Select(u => $"\"{u}\""));

        // Optional glossary: 1–2 line description of each unit so the classifier knows
        // what each unit actually tests (names alone are often ambiguous).
        var glossaryBlock = string.Empty;
        if (unitGlossary is { Count: > 0 })
        {
            var lines = units
                .Where(u => unitGlossary.ContainsKey(u) && !string.IsNullOrWhiteSpace(unitGlossary[u]))
                .Select(u => $"- \"{u}\": {unitGlossary[u].Trim()}");
            var joined = string.Join("\n", lines);
            if (!string.IsNullOrWhiteSpace(joined))
                glossaryBlock = "\n\nUNIT GLOSSARY (use this to decide which unit each question tests):\n" + joined;
        }

        return $@"You are a past-paper parser for an educational platform (Critical Thinking / TSA).{glossaryBlock}

GOAL: You are given TWO files for the SAME past paper: a QUESTIONS file and an ANSWER-KEY file. Pair every question with its correct answer from the key, then DISTRIBUTE each question across the fixed skill units.

The hierarchy is FIXED:
ExamType = ""{examTypeCode}""
ExamSection = ""{examSectionName}""
Skill = ONE OF THE ALLOWED UNITS (you MUST classify each question into the unit it best fits): {unitList}
Topic = a concept/skill WITHIN that unit (e.g. ""Identifying Assumptions"", ""Drawing Conclusions"", ""Detecting Flaws"").

RULES:
1. PAIR: For each numbered question in the QUESTIONS file, find its answer in the ANSWER-KEY file (matched by question number). Mark EXACTLY ONE option ""isCorrect"": true accordingly. If the key gives a worked solution, put it into ""explanation"".
2. CLASSIFY: Decide which UNIT each question tests, using ONLY the allowed unit names above for ""skillName"". Then place it under a concept ""topic"" inside that unit.
3. GROUP the output by unit. Each unit appears at most once in ""units"", containing its topics, each topic containing its questions.
4. If a question's correct answer cannot be found in the key, still include it but pick the most defensible option as correct and note it in ""explanation"".
5. Keep text/Unicode/KaTeX as-is. ""difficulty"" ∈ {{Easy, Medium, Hard}} (default Medium). Preserve original question numbers via ""sortOrder"".

OUTPUT JSON SHAPE (return ONLY this object, no prose):
{{
  ""units"": [
    {{
      ""skillName"": ""Unit 3"",
      ""topics"": [
        {{
          ""name"": ""concept within the unit"",
          ""sortOrder"": 1,
          ""lessonContent"": null,
          ""formulas"": [],
          ""questions"": [
            {{
              ""text"": ""question text"",
              ""options"": [
                {{ ""text"": ""A) ..."", ""isCorrect"": false }},
                {{ ""text"": ""B) ..."", ""isCorrect"": true }}
              ],
              ""explanation"": ""why B / worked solution or null"",
              ""hint"": null,
              ""difficulty"": ""Medium"",
              ""sortOrder"": 1
            }}
          ]
        }}
      ]
    }}
  ]
}}

Return ONLY valid JSON.";
    }

    private IReadOnlyList<IngestContentDto> ParseTsaResponse(
        string responseContent, string examTypeCode, string examSectionName, IReadOnlyList<string> allowedUnits)
    {
        var trimmed = responseContent.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNl = trimmed.IndexOf('\n');
            if (firstNl >= 0) trimmed = trimmed[(firstNl + 1)..];
            if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];
            trimmed = trimmed.Trim();
        }

        TsaEnvelope? env;
        try
        {
            env = JsonSerializer.Deserialize<TsaEnvelope>(trimmed, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse TSA LLM JSON. Raw: {Raw}", trimmed[..Math.Min(400, trimmed.Length)]);
            throw new InvalidOperationException("LLM returned malformed JSON for TSA pair.", ex);
        }

        if (env?.Units == null || env.Units.Count == 0)
            throw new InvalidOperationException("LLM returned no units for TSA pair.");

        // Map the LLM's free-form skillName back onto an allowed unit name (defensive:
        // accept exact match first, then a case-insensitive / trailing-number match).
        var payloads = new List<IngestContentDto>();
        foreach (var u in env.Units)
        {
            var skill = ResolveUnit(u.SkillName, allowedUnits);
            if (skill == null || u.Topics == null || u.Topics.Count == 0)
                continue;
            payloads.Add(new IngestContentDto(examTypeCode, examSectionName, skill, u.Topics));
        }

        if (payloads.Count == 0)
            throw new InvalidOperationException("TSA parse produced no ingestible units (no question matched an allowed unit).");

        return payloads;
    }

    private static string? ResolveUnit(string? raw, IReadOnlyList<string> allowed)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var match = allowed.FirstOrDefault(a => a.Equals(raw, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        // Compare by trailing number, e.g. "unit 3" vs "Unit 3".
        var rawNum = System.Text.RegularExpressions.Regex.Match(raw, @"(\d+)").Value;
        if (!string.IsNullOrEmpty(rawNum))
        {
            match = allowed.FirstOrDefault(a =>
                System.Text.RegularExpressions.Regex.Match(a, @"(\d+)").Value == rawNum);
            if (match != null) return match;
        }
        return null;
    }

    private sealed class TsaEnvelope
    {
        [JsonPropertyName("units")]
        public List<TsaUnit>? Units { get; set; }
    }

    private sealed class TsaUnit
    {
        [JsonPropertyName("skillName")]
        public string? SkillName { get; set; }

        [JsonPropertyName("topics")]
        public List<IngestTopicDto>? Topics { get; set; }
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        // LLMs frequently emit numbers as strings ("sortOrder": "1") and booleans as
        // strings ("isCorrect": "true"). Tolerate both so a single bad field doesn't
        // fail the whole parse.
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new FlexibleBoolConverter() },
    };

    /// <summary>Reads bool from real JSON booleans AND string/number forms ("true"/"1"/"yes").</summary>
    private sealed class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True: return true;
                case JsonTokenType.False: return false;
                case JsonTokenType.Number: return reader.GetDouble() != 0;
                case JsonTokenType.String:
                    var s = reader.GetString()?.Trim();
                    return s is not null && (s.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || s.Equals("yes", StringComparison.OrdinalIgnoreCase)
                        || s == "1");
                default: return false;
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            => writer.WriteBooleanValue(value);
    }


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
