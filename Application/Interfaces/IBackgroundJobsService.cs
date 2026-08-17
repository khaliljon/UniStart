namespace UniStart.Application.Interfaces;

public interface IBackgroundJobsService
{
    Task PurgeSoftDeletedRecordsAsync();

    Task CalibrateIrtParametersAsync();
}
