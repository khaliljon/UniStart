using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UniStart.Application.DTOs;
using UniStart.Application.Exceptions;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendVerificationCodeAsync(string toEmail, string userName, string code)
    {
        var subject = "Код подтверждения — UniStart";
        var body = GetVerificationCodeTemplate(userName, code);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Добро пожаловать в UniStart!";
        var body = GetWelcomeTemplate(userName);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPurchaseReceiptAsync(string toEmail, string userName, decimal total, string currency)
    {
        var subject = "Спасибо за покупку — UniStart";
        var body = GetPurchaseReceiptTemplate(userName, total, currency);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetCodeAsync(string toEmail, string userName, string code)
    {
        var subject = "Восстановление пароля — UniStart";
        var body = GetPasswordResetTemplate(userName, code);
        var sent = await SendEmailAsync(toEmail, subject, body);
        if (!sent)
            throw new EmailDeliveryException("Failed to send password reset code email");
    }

    public async Task SendStreakReminderAsync(string toEmail, string userName, int lastStreak, int inactiveDays)
    {
        var subject = "Не потеряйте серию! — UniStart";
        var body = GetStreakReminderTemplate(userName, lastStreak, inactiveDays);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWeeklyDigestAsync(string toEmail, WeeklyDigestDataDto data)
    {
        var subject = "Ваш еженедельный отчёт — UniStart";
        var body = GetWeeklyDigestTemplate(data);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendStudyPlanReminderAsync(string toEmail, string userName, string todayPlanSummary)
    {
        var subject = "Ваш план на сегодня готов! — UniStart";
        var body = GetStudyPlanReminderTemplate(userName, todayPlanSummary);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendAchievementEmailAsync(string toEmail, string userName, string achievementTitle, string achievementIcon)
    {
        var subject = $"Новое достижение: {achievementTitle}! — UniStart";
        var body = GetAchievementTemplate(userName, achievementTitle, achievementIcon);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendNewSchoolApplicationNotificationAsync(string adminEmail, string schoolName, string contactName, string contactEmail)
    {
        var subject = $"Новая заявка от школы: {schoolName} — UniStart";
        var body = GetNewSchoolApplicationTemplate(schoolName, contactName, contactEmail);
        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendSchoolApplicationStatusAsync(string toEmail, string contactName, string schoolName, bool approved)
    {
        var status = approved ? "одобрена" : "отклонена";
        var subject = $"Ваша заявка {status} — UniStart";
        var body = GetSchoolApplicationStatusTemplate(contactName, schoolName, approved);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendVerificationRequestNotificationAsync(string adminEmail, string tutorName, string tutorEmail)
    {
        var subject = $"Запрос верификации тьютора: {tutorName} — UniStart";
        var name = System.Net.WebUtility.HtmlEncode(tutorName);
        var mail = System.Net.WebUtility.HtmlEncode(tutorEmail);
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Новый запрос верификации</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Тьютор запросил верификацию. Перейдите в панель администратора для проверки.
      </p>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;border-left:4px solid #6c5ce7;"">
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Тьютор:</strong> {name}</p>
        <p style=""margin:0;font-size:15px;color:#333;""><strong>Email:</strong> {mail}</p>
      </div>";
        var body = WrapInLayout("Запрос верификации тьютора — UniStart", content, showNotificationSettings: false);
        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendContactFormAsync(string adminEmail, string senderName, string senderEmail, string message)
    {
        var subject = $"Обратная связь от {senderName} — UniStart";
        var name = System.Net.WebUtility.HtmlEncode(senderName);
        var mail = System.Net.WebUtility.HtmlEncode(senderEmail);
        var msg = System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br>");
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Обратная связь</h2>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;border-left:4px solid #6c5ce7;"">
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Имя:</strong> {name}</p>
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Email:</strong> {mail}</p>
      </div>
      <p style=""font-size:15px;line-height:1.6;color:#555;""><strong>Сообщение:</strong></p>
      <p style=""font-size:15px;line-height:1.6;color:#333;"">{msg}</p>";
        var body = WrapInLayout("Обратная связь — UniStart", content, showNotificationSettings: false);
        await SendEmailAsync(adminEmail, subject, body);
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var enabled = emailSettings.GetValue<bool>("Enabled");

        if (!enabled)
        {
            _logger.LogInformation("Email disabled. Would send to {Email}: {Subject}", toEmail, subject);
            return true;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailSettings["SenderName"] ?? "UniStart",
                emailSettings["SenderEmail"] ?? "noreply@unistart.kz"
            ));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var host = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
            var port = emailSettings.GetValue<int>("SmtpPort", 587);
            var useSsl = emailSettings.GetValue<bool>("UseSsl", true);

            await client.ConnectAsync(host, port,
                useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }
            else
            {
                _logger.LogWarning(
                    "EmailSettings:Enabled is true but Username/Password are empty. " +
                    "SMTP server {Host} will likely reject the message to {Email}.", host, toEmail);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            return false;
        }
    }


    private string GetBaseUrl() =>
        _config["EmailSettings:ClientBaseUrl"]?.TrimEnd('/') ?? "https://unistart.kz";

    private string WrapInLayout(string title, string content, bool showNotificationSettings = true)
    {
        var baseUrl = GetBaseUrl();
        var notifLink = showNotificationSettings
            ? $@"<br><a href=""{baseUrl}/profile/notifications"" style=""color:#6c5ce7;text-decoration:none;"">Настройки уведомлений</a>"
            : "";

        return $@"<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{title}</title>
</head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',Roboto,sans-serif;background:#f4f6f9;color:#1a1a2e;"">
  <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);margin-top:24px;margin-bottom:24px;"">
    <!-- Header -->
    <div style=""background:linear-gradient(135deg,#6c5ce7,#a855f7);padding:32px 24px;text-align:center;"">
      <h1 style=""margin:0;color:#fff;font-size:28px;font-weight:700;"">UniStart</h1>
      <p style=""margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px;"">Подготовка к CSCA</p>
    </div>
    <!-- Content -->
    <div style=""padding:32px 24px;"">
      {content}
    </div>
    <!-- Footer -->
    <div style=""background:#f8f9fa;padding:20px 24px;text-align:center;border-top:1px solid #eee;"">
      <p style=""margin:0;font-size:12px;color:#999;"">
        © {DateTime.UtcNow.Year} UniStart. Все права защищены.{notifLink}
      </p>
    </div>
  </div>
</body>
</html>";
    }

    private string GetVerificationCodeTemplate(string userName, string code)
    {
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Привет, {userName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Ваш код подтверждения для входа в UniStart:
      </p>
      <div style=""background:#f0f4ff;border-radius:12px;padding:24px;margin:24px 0;text-align:center;"">
        <div style=""font-size:36px;font-weight:800;letter-spacing:8px;color:#6c5ce7;font-family:monospace;"">{code}</div>
      </div>
      <p style=""font-size:14px;line-height:1.6;color:#999;"">
        Код действителен 10 минут. Если вы не запрашивали этот код, проигнорируйте это письмо.
      </p>";

        return WrapInLayout("Код подтверждения — UniStart", content, showNotificationSettings: false);
    }

    private string GetWelcomeTemplate(string userName)
    {
        var baseUrl = GetBaseUrl();
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Привет, {userName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Добро пожаловать в <strong>UniStart</strong> — адаптивную платформу подготовки к экзаменам.
      </p>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">Вот что вас ждёт:</p>
      <ul style=""font-size:15px;line-height:1.8;color:#555;padding-left:20px;"">
        <li><strong>Адаптивные тесты</strong> — вопросы подстраиваются под ваш уровень</li>
        <li><strong>Детальная аналитика</strong> — отслеживайте прогресс по каждому навыку</li>
        <li><strong>Персональный план</strong> — оптимальный путь к целевому баллу</li>
        <li><strong>Прогноз оценки</strong> — знайте свой предполагаемый результат</li>
      </ul>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Начать подготовку →
        </a>
      </div>
      <p style=""font-size:14px;color:#999;text-align:center;"">Удачи в подготовке!</p>";

        return WrapInLayout("Добро пожаловать в UniStart!", content, showNotificationSettings: false);
    }

    private string GetPasswordResetTemplate(string userName, string code)
    {
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Привет, {userName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Вы запросили восстановление пароля в UniStart. Используйте код ниже:
      </p>
      <div style=""background:#f0f4ff;border-radius:12px;padding:24px;margin:24px 0;text-align:center;"">
        <div style=""font-size:36px;font-weight:800;letter-spacing:8px;color:#6c5ce7;font-family:monospace;"">{code}</div>
      </div>
      <p style=""font-size:14px;line-height:1.6;color:#999;"">
        Код действителен 10 минут. Если вы не запрашивали сброс пароля, проигнорируйте это письмо.
      </p>";

        return WrapInLayout("Восстановление пароля — UniStart", content, showNotificationSettings: false);
    }

    private string GetPurchaseReceiptTemplate(string userName, decimal total, string currency)
    {
        var baseUrl = GetBaseUrl();
        var name = System.Net.WebUtility.HtmlEncode(userName);
        var cur = System.Net.WebUtility.HtmlEncode(currency);
        var totalStr = total.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Спасибо за покупку, {name}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Ваш заказ оплачен, доступ уже начислен.
      </p>
      <div style=""background:#f0f4ff;border-radius:12px;padding:24px;margin:24px 0;text-align:center;"">
        <div style=""font-size:14px;color:#666;margin-bottom:4px;"">Итого</div>
        <div style=""font-size:32px;font-weight:800;color:#6c5ce7;"">{totalStr} {cur}</div>
      </div>
      <p style=""font-size:15px;line-height:1.6;color:#555;"">
        Запуски пробников доступны во вкладке «Пробные экзамены», учебники — в разделе «Материалы».
      </p>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/mocks"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Перейти к пробникам →
        </a>
      </div>
      <p style=""font-size:14px;color:#999;text-align:center;"">Если это были не вы — напишите в поддержку.</p>";

        return WrapInLayout("Спасибо за покупку — UniStart", content, showNotificationSettings: false);
    }

    private string GetStreakReminderTemplate(string userName, int lastStreak, int inactiveDays)
    {
        var baseUrl = GetBaseUrl();
        var streakText = lastStreak > 0
            ? $"У вас была серия <strong>{lastStreak} {GetDaysWord(lastStreak)}</strong> подряд — не потеряйте её!"
            : "Пора вернуться к учёбе!";

        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Привет, {userName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Вы не занимались уже {inactiveDays} {GetDaysWord(inactiveDays)}. {streakText}
      </p>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;text-align:center;"">
        <div style=""font-size:14px;color:#666;margin-bottom:4px;"">Ваша серия</div>
        <div style=""font-size:28px;font-weight:700;color:#6c5ce7;"">
          {(lastStreak > 0 ? $"{lastStreak} {GetDaysWord(lastStreak)}" : "0 дней")}
        </div>
      </div>
      <p style=""font-size:15px;line-height:1.6;color:#555;"">
        Даже 10 минут практики в день помогают удержать знания и повысить результат.
      </p>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/test"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Продолжить обучение →
        </a>
      </div>";

        return WrapInLayout("Не потеряйте серию!", content);
    }

    private string GetWeeklyDigestTemplate(WeeklyDigestDataDto data)
    {
        var baseUrl = GetBaseUrl();
        var topProgressHtml = string.Join("", data.TopProgress.Select(tp =>
        {
            var trendIcon = tp.Trend switch
            {
                "improving" => "<span style=\"color:#10b981;font-weight:600;\">&#9650;</span>",
                "declining" => "<span style=\"color:#ef4444;font-weight:600;\">&#9660;</span>",
                _ => "<span style=\"color:#999;\">&#8212;</span>"
            };
            return $@"
              <tr>
                <td style=""padding:8px 12px;border-bottom:1px solid #f0f0f0;"">{tp.TopicName}</td>
                <td style=""padding:8px 12px;border-bottom:1px solid #f0f0f0;text-align:center;"">{tp.QuestionsAnswered}</td>
                <td style=""padding:8px 12px;border-bottom:1px solid #f0f0f0;text-align:center;"">{tp.Accuracy:F0}%</td>
                <td style=""padding:8px 12px;border-bottom:1px solid #f0f0f0;text-align:center;"">{trendIcon}</td>
              </tr>";
        }));

        var recommendationsHtml = string.Join("", data.Recommendations.Select(r =>
            $@"<li style=""margin-bottom:8px;"">{r}</li>"));

        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Привет, {data.UserName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">Вот ваш еженедельный отчёт по подготовке к <strong>{data.ExamName}</strong>:</p>

      <!-- Stats Grid -->
      <div style=""display:flex;flex-wrap:wrap;gap:12px;margin:24px 0;"">
        <div style=""flex:1;min-width:120px;background:#f0f4ff;border-radius:8px;padding:16px;text-align:center;"">
          <div style=""font-size:24px;font-weight:700;color:#6c5ce7;"">{data.QuestionsAnswered}</div>
          <div style=""font-size:12px;color:#666;margin-top:4px;"">Вопросов</div>
        </div>
        <div style=""flex:1;min-width:120px;background:#f0fff4;border-radius:8px;padding:16px;text-align:center;"">
          <div style=""font-size:24px;font-weight:700;color:#10b981;"">{data.Accuracy:F0}%</div>
          <div style=""font-size:12px;color:#666;margin-top:4px;"">Точность</div>
        </div>
        <div style=""flex:1;min-width:120px;background:#fff7ed;border-radius:8px;padding:16px;text-align:center;"">
          <div style=""font-size:24px;font-weight:700;color:#f59e0b;"">{data.CurrentStreak}</div>
          <div style=""font-size:12px;color:#666;margin-top:4px;"">Серия</div>
        </div>
        <div style=""flex:1;min-width:120px;background:#fdf2f8;border-radius:8px;padding:16px;text-align:center;"">
          <div style=""font-size:24px;font-weight:700;color:#a855f7;"">{data.PredictedScore}/{data.MaxPossibleScore}</div>
          <div style=""font-size:12px;color:#666;margin-top:4px;"">Прогноз</div>
        </div>
      </div>

      <!-- Top Topics -->
      {(data.TopProgress.Any() ? $@"
      <h3 style=""color:#1a1a2e;margin:24px 0 12px;"">По темам</h3>
      <table style=""width:100%;border-collapse:collapse;font-size:14px;"">
        <thead>
          <tr style=""background:#f8f9fa;"">
            <th style=""padding:8px 12px;text-align:left;"">Тема</th>
            <th style=""padding:8px 12px;text-align:center;"">Вопр.</th>
            <th style=""padding:8px 12px;text-align:center;"">Точн.</th>
            <th style=""padding:8px 12px;text-align:center;"">Тренд</th>
          </tr>
        </thead>
        <tbody>
          {topProgressHtml}
        </tbody>
      </table>" : "")}

      <!-- Recommendations -->
      {(data.Recommendations.Any() ? $@"
      <h3 style=""color:#1a1a2e;margin:24px 0 12px;"">Рекомендации</h3>
      <ul style=""font-size:15px;line-height:1.8;color:#555;padding-left:20px;"">
        {recommendationsHtml}
      </ul>" : "")}

      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/analytics"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Подробная аналитика →
        </a>
      </div>";

        return WrapInLayout("Еженедельный отчёт UniStart", content);
    }

    private string GetStudyPlanReminderTemplate(string userName, string todayPlanSummary)
    {
        var baseUrl = GetBaseUrl();
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Доброе утро, {userName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Ваш план на сегодня:
      </p>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;border-left:4px solid #6c5ce7;"">
        <p style=""margin:0;font-size:15px;line-height:1.6;color:#333;"">
          {todayPlanSummary}
        </p>
      </div>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/study-plan"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Открыть план →
        </a>
      </div>";

        return WrapInLayout("Ваш план на сегодня", content);
    }

    private string GetAchievementTemplate(string userName, string title, string icon)
    {
        var baseUrl = GetBaseUrl();
        var iconHtml = !string.IsNullOrEmpty(icon)
            ? $@"<div style=""font-size:48px;margin-bottom:12px;"">{icon}</div>"
            : "";

        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Поздравляем, {userName}!</h2>
      <div style=""text-align:center;margin:32px 0;"">
        {iconHtml}
        <div style=""font-size:36px;font-weight:700;color:#6c5ce7;"">{title}</div>
        <p style=""font-size:16px;color:#555;margin-top:12px;"">Вы открыли новое достижение!</p>
      </div>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/recommendations"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Все достижения →
        </a>
      </div>";

        return WrapInLayout("Новое достижение!", content);
    }

    private string GetNewSchoolApplicationTemplate(string schoolName, string contactName, string contactEmail)
    {
        var baseUrl = GetBaseUrl();
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Новая заявка от школы</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Поступила новая заявка на подключение школы к платформе UniStart.
      </p>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;border-left:4px solid #6c5ce7;"">
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Школа:</strong> {schoolName}</p>
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Контакт:</strong> {contactName}</p>
        <p style=""margin:0;font-size:15px;color:#333;""><strong>Email:</strong> {contactEmail}</p>
      </div>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/applications"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Открыть заявки →
        </a>
      </div>";

        return WrapInLayout("Новая заявка от школы", content, showNotificationSettings: false);
    }

    private string GetSchoolApplicationStatusTemplate(string contactName, string schoolName, bool approved)
    {
        var baseUrl = GetBaseUrl();
        if (approved)
        {
            var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Здравствуйте, {contactName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Ваша заявка на подключение школы <strong>{schoolName}</strong> к платформе UniStart <strong style=""color:#10b981;"">одобрена</strong>.
      </p>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Наш менеджер свяжется с вами в ближайшее время для обсуждения деталей подключения.
      </p>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}"" style=""display:inline-block;background:linear-gradient(135deg,#6c5ce7,#a855f7);color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Перейти на UniStart →
        </a>
      </div>";

            return WrapInLayout("Заявка одобрена — UniStart", content, showNotificationSettings: false);
        }
        else
        {
            var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Здравствуйте, {contactName}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        К сожалению, ваша заявка на подключение школы <strong>{schoolName}</strong> к платформе UniStart была <strong style=""color:#ef4444;"">отклонена</strong>.
      </p>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Если у вас есть вопросы, свяжитесь с нами по адресу <a href=""mailto:support@unistart.kz"" style=""color:#6c5ce7;"">support@unistart.kz</a>.
      </p>";

            return WrapInLayout("Заявка отклонена — UniStart", content, showNotificationSettings: false);
        }
    }


    private static string GetDaysWord(int count)
    {
        var abs = Math.Abs(count) % 100;
        var lastDigit = abs % 10;
        if (abs is > 10 and < 20) return "дней";
        if (lastDigit is 1) return "день";
        if (lastDigit is >= 2 and <= 4) return "дня";
        return "дней";
    }


    public static readonly IReadOnlyList<(string Key, string Label)> PreviewKeys = new List<(string Key, string Label)>
    {
        ("verification", "Код подтверждения"),
        ("welcome", "Добро пожаловать"),
        ("password-reset", "Восстановление пароля"),
        ("purchase", "Чек об оплате"),
        ("streak", "Напоминание о серии"),
        ("digest", "Еженедельный отчёт"),
        ("study-plan", "План на сегодня"),
        ("achievement", "Новое достижение"),
        ("verification-request", "Запрос верификации (админу)"),
        ("contact-form", "Обратная связь (админу)"),
        ("school-application", "Заявка от школы (админу)"),
        ("school-approved", "Заявка одобрена"),
        ("school-rejected", "Заявка отклонена"),
    };

    public string RenderPreview(string key) => key switch
    {
        "verification" => GetVerificationCodeTemplate("Халыч Каландаров", "605372"),
        "welcome" => GetWelcomeTemplate("Халыч Каландаров"),
        "password-reset" => GetPasswordResetTemplate("Халыч Каландаров", "605372"),
        "purchase" => GetPurchaseReceiptTemplate("Халыч Каландаров", 25000m, "KZT"),
        "streak" => GetStreakReminderTemplate("Халыч Каландаров", 7, 3),
        "digest" => GetWeeklyDigestTemplate(SampleDigest()),
        "study-plan" => GetStudyPlanReminderTemplate("Халыч Каландаров", "3 темы · 20 вопросов · 1 пробный экзамен"),
        "achievement" => GetAchievementTemplate("Халыч Каландаров", "Первые 100 вопросов", "🏆"),
        "verification-request" => VerificationRequestPreview(),
        "contact-form" => ContactFormPreview(),
        "school-application" => GetNewSchoolApplicationTemplate("Лицей №1", "Айгуль Смагулова", "school@example.kz"),
        "school-approved" => GetSchoolApplicationStatusTemplate("Айгуль Смагулова", "Лицей №1", approved: true),
        "school-rejected" => GetSchoolApplicationStatusTemplate("Айгуль Смагулова", "Лицей №1", approved: false),
        _ => throw new ArgumentException($"Unknown preview key: {key}", nameof(key)),
    };

    private static WeeklyDigestDataDto SampleDigest() => new(
        UserName: "Халыч Каландаров",
        QuestionsAnswered: 142,
        CorrectAnswers: 118,
        Accuracy: 83.1,
        CurrentStreak: 7,
        PredictedScore: 82,
        MaxPossibleScore: 100,
        ExamName: "CSCA",
        TopProgress: new List<WeeklyTopicProgressDto>
        {
            new("Алгебра", 48, 88, "improving"),
            new("Геометрия", 32, 72, "declining"),
            new("Механика", 26, 80, "flat"),
        },
        Recommendations: new List<string>
        {
            "Повторите тему «Геометрия» — точность ниже целевой.",
            "Пройдите один пробный экзамен на этой неделе.",
        });

    private string VerificationRequestPreview()
    {
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Новый запрос верификации</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Тьютор запросил верификацию. Перейдите в панель администратора для проверки.
      </p>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;border-left:4px solid #6c5ce7;"">
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Тьютор:</strong> Данияр Оспанов</p>
        <p style=""margin:0;font-size:15px;color:#333;""><strong>Email:</strong> tutor@example.kz</p>
      </div>";
        return WrapInLayout("Запрос верификации тьютора — UniStart", content, showNotificationSettings: false);
    }

    private string ContactFormPreview()
    {
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Обратная связь</h2>
      <div style=""background:#f0f4ff;border-radius:8px;padding:20px;margin:24px 0;border-left:4px solid #6c5ce7;"">
        <p style=""margin:0 0 8px;font-size:15px;color:#333;""><strong>Имя:</strong> Айгерим Нурланова</p>
        <p style=""margin:0;font-size:15px;color:#333;""><strong>Email:</strong> user@example.kz</p>
      </div>
      <p style=""font-size:15px;line-height:1.6;color:#555;""><strong>Сообщение:</strong></p>
      <p style=""font-size:15px;line-height:1.6;color:#333;"">Здравствуйте! Подскажите, как получить доступ к пробным экзаменам?</p>";
        return WrapInLayout("Обратная связь — UniStart", content, showNotificationSettings: false);
    }
}
