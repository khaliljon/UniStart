using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string toEmail, string userName, string code);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendStreakReminderAsync(string toEmail, string userName, int lastStreak, int inactiveDays);
    Task SendWeeklyDigestAsync(string toEmail, WeeklyDigestDataDto data);
    Task SendStudyPlanReminderAsync(string toEmail, string userName, string todayPlanSummary);
    Task SendAchievementEmailAsync(string toEmail, string userName, string achievementTitle, string achievementIcon);
}
