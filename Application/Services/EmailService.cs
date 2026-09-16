using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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

    public async Task SendPasswordResetCodeAsync(string toEmail, string userName, string code)
    {
        var subject = "Восстановление пароля — UniStart";
        var body = GetPasswordResetTemplate(userName, code);
        var sent = await SendEmailAsync(toEmail, subject, body);
        if (!sent)
            throw new EmailDeliveryException("Failed to send password reset code email");
    }

    public async Task SendPurchaseReceiptAsync(string toEmail, string userName, decimal total, string currency)
    {
        var subject = "Спасибо за покупку — UniStart";
        var body = GetPurchaseReceiptTemplate(userName, total, currency);
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendKaspiAmountMismatchEmailAsync(string toEmail, string userName, string orderCode, decimal expected, decimal actual, string currency)
    {
        var subject = "Проблема с оплатой заказа — UniStart";
        var body = GetKaspiAmountMismatchTemplate(userName, orderCode, expected, actual, currency);
        await SendEmailAsync(toEmail, subject, body);
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
            ? $@"<br><a href=""{baseUrl}/profile/notifications"" style=""color:#c20f2c;text-decoration:none;"">Настройки уведомлений</a>"
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
    <div style=""background:#c20f2c;padding:32px 24px;text-align:center;"">
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
      <div style=""background:#fbeaec;border-radius:12px;padding:24px;margin:24px 0;text-align:center;"">
        <div style=""font-size:36px;font-weight:800;letter-spacing:8px;color:#c20f2c;font-family:monospace;"">{code}</div>
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
        Добро пожаловать в <strong>UniStart</strong> — платформу подготовки к поступлению в вузы Китая.
      </p>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">Вот что вас ждёт:</p>
      <ul style=""font-size:15px;line-height:1.8;color:#555;padding-left:20px;"">
        <li><strong>Пробные экзамены (моки)</strong> — более 6000 вопросов по 5 предметам с разбивкой по уровням сложности.</li>
        <li><strong>ИИ-объяснения ошибок</strong> — сразу понимайте, что исправить.</li>
        <li><strong>Учебные материалы</strong> — структурированные пособия по каждому предмету для самостоятельной подготовки.</li>
        <li><strong>Доступ 24/7</strong> — занимайтесь в удобное время без записи к репетитору.</li>
        <li><strong>Первый мок — бесплатно.</strong></li>
      </ul>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/"" style=""display:inline-block;background:#c20f2c;color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
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
      <div style=""background:#fbeaec;border-radius:12px;padding:24px;margin:24px 0;text-align:center;"">
        <div style=""font-size:36px;font-weight:800;letter-spacing:8px;color:#c20f2c;font-family:monospace;"">{code}</div>
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
      <div style=""background:#fbeaec;border-radius:12px;padding:24px;margin:24px 0;text-align:center;"">
        <div style=""font-size:14px;color:#666;margin-bottom:4px;"">Итого</div>
        <div style=""font-size:32px;font-weight:800;color:#c20f2c;"">{totalStr} {cur}</div>
      </div>
      <p style=""font-size:15px;line-height:1.6;color:#555;"">
        Запуски пробников доступны во вкладке «Пробные экзамены», учебники — в разделе «Материалы».
      </p>
      <div style=""text-align:center;margin:32px 0;"">
        <a href=""{baseUrl}/mocks"" style=""display:inline-block;background:#c20f2c;color:#fff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:600;font-size:16px;"">
          Перейти к пробникам →
        </a>
      </div>
      <p style=""font-size:14px;color:#999;text-align:center;"">Если это были не вы — напишите в поддержку.</p>";

        return WrapInLayout("Спасибо за покупку — UniStart", content, showNotificationSettings: false);
    }


    private string GetKaspiAmountMismatchTemplate(string userName, string orderCode, decimal expected, decimal actual, string currency)
    {
        var name = System.Net.WebUtility.HtmlEncode(userName);
        var code = System.Net.WebUtility.HtmlEncode(orderCode);
        var cur = System.Net.WebUtility.HtmlEncode(currency);
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var expectedStr = expected.ToString("N0", inv);
        var actualStr = actual.ToString("N0", inv);
        var content = $@"
      <h2 style=""color:#1a1a2e;margin:0 0 16px;"">Здравствуйте, {name}!</h2>
      <p style=""font-size:16px;line-height:1.6;color:#555;"">
        Мы получили платёж по заказу <b>{code}</b>, но его сумма не совпадает с суммой заказа,
        поэтому доступ пока <b>не активирован</b>.
      </p>
      <div style=""background:#fbeaec;border-radius:12px;padding:20px;margin:20px 0;"">
        <div style=""display:flex;justify-content:space-between;font-size:15px;color:#555;margin-bottom:6px;"">
          <span>Ожидалось:</span><span><b>{expectedStr} {cur}</b></span>
        </div>
        <div style=""display:flex;justify-content:space-between;font-size:15px;color:#555;"">
          <span>Фактически оплачено:</span><span><b>{actualStr} {cur}</b></span>
        </div>
      </div>
      <p style=""font-size:15px;line-height:1.6;color:#555;"">
        Пожалуйста, свяжитесь с поддержкой UniStart по адресу unistart.kz@gmail.com. Мы проверим платёж и подскажем дальнейшие действия.
      </p>
      <p style=""font-size:15px;line-height:1.6;color:#c20f2c;font-weight:600;"">
        Не совершайте дополнительный платёж до уточнения ситуации.
      </p>";

        return WrapInLayout("Проблема с оплатой заказа — UniStart", content, showNotificationSettings: false);
    }


    public static readonly IReadOnlyList<(string Key, string Label)> PreviewKeys = new List<(string Key, string Label)>
    {
        ("verification", "Код подтверждения"),
        ("welcome", "Добро пожаловать"),
        ("password-reset", "Восстановление пароля"),
        ("purchase", "Чек об оплате"),
        ("kaspi-mismatch", "Проблема с оплатой (Kaspi)"),
    };

    public string RenderPreview(string key) => key switch
    {
        "verification" => GetVerificationCodeTemplate("Халыч Каландаров", "605372"),
        "welcome" => GetWelcomeTemplate("Халыч Каландаров"),
        "password-reset" => GetPasswordResetTemplate("Халыч Каландаров", "605372"),
        "purchase" => GetPurchaseReceiptTemplate("Халыч Каландаров", 25000m, "KZT"),
        "kaspi-mismatch" => GetKaspiAmountMismatchTemplate("Халыч Каландаров", "US-K-7QF2M9AH", 10990m, 10000m, "KZT"),
        _ => throw new ArgumentException($"Unknown preview key: {key}", nameof(key)),
    };
}
