using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string toEmail, string userName, string code);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendPasswordResetCodeAsync(string toEmail, string userName, string code);
    Task SendStreakReminderAsync(string toEmail, string userName, int lastStreak, int inactiveDays);
    Task SendWeeklyDigestAsync(string toEmail, WeeklyDigestDataDto data);
    Task SendStudyPlanReminderAsync(string toEmail, string userName, string todayPlanSummary);
    Task SendAchievementEmailAsync(string toEmail, string userName, string achievementTitle, string achievementIcon);
    Task SendNewSchoolApplicationNotificationAsync(string adminEmail, string schoolName, string contactName, string contactEmail);
    Task SendSchoolApplicationStatusAsync(string toEmail, string contactName, string schoolName, bool approved);
    Task SendVerificationRequestNotificationAsync(string adminEmail, string tutorName, string tutorEmail);
    Task SendContactFormAsync(string adminEmail, string senderName, string senderEmail, string message);
    Task SendPurchaseReceiptAsync(string toEmail, string userName, decimal total, string currency);
}
