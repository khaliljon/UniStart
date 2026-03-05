namespace UniStart.Application.Interfaces;

/// <summary>
/// Defines background/recurring job methods managed by Hangfire.
/// </summary>
public interface IBackgroundJobsService
{
    /// <summary>
    /// Sends streak reminder emails to users who haven't studied for 2+ days.
    /// Scheduled every 6 hours via Hangfire.
    /// </summary>
    Task ProcessStreakRemindersAsync();

    /// <summary>
    /// Sends weekly digest emails with progress summary.
    /// Scheduled every Monday at 08:00 UTC via Hangfire.
    /// </summary>
    Task ProcessWeeklyDigestsAsync();

    /// <summary>
    /// Permanently deletes soft-deleted records (IsDeleted = true) older than 30 days.
    /// Scheduled daily at 02:00 UTC via Hangfire (OP-9 completion).
    /// </summary>
    Task PurgeSoftDeletedRecordsAsync();
}
