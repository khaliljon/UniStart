namespace UniStart.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string toEmail, string userName, string code);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendPasswordResetCodeAsync(string toEmail, string userName, string code);
    Task SendPurchaseReceiptAsync(string toEmail, string userName, decimal total, string currency);
    Task SendKaspiAmountMismatchEmailAsync(string toEmail, string userName, string orderCode, decimal expected, decimal actual, string currency);
}
