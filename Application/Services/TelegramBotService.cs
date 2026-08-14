using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class TelegramBotService : ITelegramBotService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly HttpClient _http;
    private readonly string? _token;
    private readonly long _supportChatId;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_token);
    public string? WebhookSecret { get; }

    public TelegramBotService(UniStartDbContext db, IConfiguration config, ILogger<TelegramBotService> logger)
    {
        _db = db;
        _logger = logger;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        var section = config.GetSection("Telegram");
        _token = section["BotToken"];
        WebhookSecret = section["WebhookSecret"];
        long.TryParse(section["SupportChatId"], out _supportChatId);
    }

    private static readonly (string Key, string Question, string Answer)[] Faq =
    {
        ("what", "Что такое CSCA?",
            "CSCA (China Scholastic Competency Assessment) — стандартизированный вступительный экзамен для иностранных абитуриентов, поступающих в университеты Китая. С 2026 года обязателен для большинства программ."),
        ("dates", "📅 Даты экзамена 2026",
            "CSCA проводится 5 раз в год. Точные даты и добавление в календарь — на сайте unistart.kz в разделе «О CSCA»."),
        ("price", "💳 Сколько стоит подготовка?",
            "Отдельные пробники по предметам (Математика, Физика, Химия, Технический китайский, Гуманитарный китайский): от 3 990 ₸ за 1 мок до 10 990 ₸ за 5 моков.\n\nПакеты со скидкой: Duo (2 предмета) — 8 990 ₸, Trio (3 предмета) — 11 990 ₸, Complete (все 5×3) — 15 990 ₸, Ultimate (все 5×5) — 19 990 ₸.\n\nУчебники (химия, математика) — 7 990 ₸ каждый.\n\nОформить можно на unistart.kz в личном кабинете."),
        ("free", "🎁 Бесплатные материалы",
            "Зарегистрируйтесь на unistart.kz — и получите бесплатный пробный тест с ИИ-объяснениями."),
        ("operator", "💬 Связаться с оператором",
            "Напишите ваш вопрос одним сообщением прямо сюда — оператор поддержки ответит вам в этом чате."),
    };

    private const string Welcome =
        "👋 Здравствуйте! Это поддержка UniStart — подготовка к экзамену CSCA.\n\n" +
        "Выберите вопрос из меню ниже или напишите свой вопрос одним сообщением — оператор ответит вам здесь.";

    public async Task ProcessUpdateAsync(JsonElement update, CancellationToken ct = default)
    {
        if (!IsConfigured) return;

        try
        {
            if (update.TryGetProperty("callback_query", out var cb))
            {
                await HandleCallbackAsync(cb, ct);
                return;
            }

            if (update.TryGetProperty("message", out var msg))
            {
                await HandleMessageAsync(msg, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Telegram update");
        }
    }

    private async Task HandleMessageAsync(JsonElement msg, CancellationToken ct)
    {
        var chat = msg.GetProperty("chat");
        var chatId = chat.GetProperty("id").GetInt64();
        var chatType = chat.TryGetProperty("type", out var t) ? t.GetString() : "private";
        var text = msg.TryGetProperty("text", out var tx) ? tx.GetString() ?? "" : "";

        if (_supportChatId != 0 && chatId == _supportChatId)
        {
            if (msg.TryGetProperty("reply_to_message", out var replyTo)
                && replyTo.TryGetProperty("message_id", out var rmid))
            {
                await RouteOperatorReplyAsync(rmid.GetInt64(), text, ct);
            }
            return;
        }

        if (chatType == "private")
        {
            if (text.StartsWith("/start") || text.StartsWith("/help") || text.StartsWith("/menu"))
            {
                await SendMessageAsync(chatId, Welcome, BuildFaqKeyboard(), ct);
                return;
            }
            if (string.IsNullOrWhiteSpace(text)) return;

            await HandleUserQuestionAsync(msg, chatId, text, ct);
        }
    }

    private async Task HandleUserQuestionAsync(JsonElement msg, long chatId, string text, CancellationToken ct)
    {
        var from = msg.GetProperty("from");
        var userId = from.GetProperty("id").GetInt64();
        var firstName = from.TryGetProperty("first_name", out var fn) ? fn.GetString() : null;
        var username = from.TryGetProperty("username", out var un) ? un.GetString() : null;

        var ticket = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TelegramUserId == userId, ct);
        if (ticket == null)
        {
            ticket = new SupportTicket
            {
                TelegramUserId = userId,
                TelegramChatId = chatId,
                Username = username,
                FirstName = firstName,
                Status = "Open",
                LastMessageAt = DateTime.UtcNow,
            };
            _db.SupportTickets.Add(ticket);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            ticket.TelegramChatId = chatId;
            ticket.Username = username;
            ticket.FirstName = firstName;
            ticket.Status = "Open";
            ticket.LastMessageAt = DateTime.UtcNow;
        }

        var inbound = new SupportMessage { TicketId = ticket.Id, Direction = "In", Text = text };
        _db.SupportMessages.Add(inbound);
        await _db.SaveChangesAsync(ct);

        if (_supportChatId != 0)
        {
            var handle = string.IsNullOrWhiteSpace(username) ? "" : $" (@{username})";
            var header = $"🆕 Вопрос от {firstName}{handle} · id {userId}\nОтветьте на это сообщение, чтобы ответить пользователю.\n\n{text}";
            var groupMsgId = await SendMessageAsync(_supportChatId, header, null, ct);
            if (groupMsgId is not null)
            {
                inbound.GroupMessageId = groupMsgId;
                await _db.SaveChangesAsync(ct);
            }
        }

        await SendMessageAsync(chatId, "✅ Ваш вопрос получен! Оператор ответит вам здесь в ближайшее время.", null, ct);
    }

    private async Task RouteOperatorReplyAsync(long groupMessageId, string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var origin = await _db.SupportMessages
            .Include(m => m.Ticket)
            .FirstOrDefaultAsync(m => m.GroupMessageId == groupMessageId, ct);
        if (origin?.Ticket == null) return;

        _db.SupportMessages.Add(new SupportMessage { TicketId = origin.TicketId, Direction = "Out", Text = text });
        origin.Ticket.LastMessageAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await SendMessageAsync(origin.Ticket.TelegramChatId, $"💬 Поддержка UniStart:\n\n{text}", null, ct);
    }

    private async Task HandleCallbackAsync(JsonElement cb, CancellationToken ct)
    {
        var data = cb.TryGetProperty("data", out var d) ? d.GetString() ?? "" : "";
        var cbId = cb.GetProperty("id").GetString();
        await AnswerCallbackAsync(cbId, ct);

        if (!data.StartsWith("faq:")) return;
        var key = data[4..];
        var entry = Faq.FirstOrDefault(f => f.Key == key);
        if (entry.Key == null) return;

        if (cb.TryGetProperty("message", out var m) && m.TryGetProperty("chat", out var c))
        {
            var chatId = c.GetProperty("id").GetInt64();
            await SendMessageAsync(chatId, $"❓ {entry.Question}\n\n{entry.Answer}", BuildFaqKeyboard(), ct);
        }
    }

    public async Task<bool> SendOperatorReplyAsync(int ticketId, string text, CancellationToken ct = default)
    {
        var ticket = await _db.SupportTickets.FindAsync(new object[] { ticketId }, ct);
        if (ticket == null) return false;

        _db.SupportMessages.Add(new SupportMessage { TicketId = ticketId, Direction = "Out", Text = text });
        ticket.LastMessageAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var sent = await SendMessageAsync(ticket.TelegramChatId, $"💬 Поддержка UniStart:\n\n{text}", null, ct);
        return sent is not null;
    }

    private object BuildFaqKeyboard()
    {
        var rows = Faq.Select(f => new[] { new { text = f.Question, callback_data = $"faq:{f.Key}" } }).ToArray();
        return new { inline_keyboard = rows };
    }

    private async Task<long?> SendMessageAsync(long chatId, string text, object? replyMarkup, CancellationToken ct)
    {
        var payload = new Dictionary<string, object?>
        {
            ["chat_id"] = chatId,
            ["text"] = text,
        };
        if (replyMarkup != null) payload["reply_markup"] = replyMarkup;

        var result = await CallAsync("sendMessage", payload, ct);
        if (result is not null && result.Value.TryGetProperty("result", out var r)
            && r.TryGetProperty("message_id", out var mid))
        {
            return mid.GetInt64();
        }
        return null;
    }

    private async Task AnswerCallbackAsync(string? callbackQueryId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(callbackQueryId)) return;
        await CallAsync("answerCallbackQuery", new Dictionary<string, object?> { ["callback_query_id"] = callbackQueryId }, ct);
    }

    private async Task<JsonElement?> CallAsync(string method, object payload, CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"https://api.telegram.org/bot{_token}/{method}", content, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Telegram API {Method} failed: {Status} {Body}", method, (int)resp.StatusCode, body);
                return null;
            }
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram API call {Method} threw", method);
            return null;
        }
    }
}
