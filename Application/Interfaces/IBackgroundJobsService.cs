namespace UniStart.Application.Interfaces;

public interface IBackgroundJobsService
{
    Task ProcessStreakRemindersAsync();

    Task ProcessWeeklyDigestsAsync();

    Task PurgeSoftDeletedRecordsAsync();

    Task CalibrateIrtParametersAsync();
}
