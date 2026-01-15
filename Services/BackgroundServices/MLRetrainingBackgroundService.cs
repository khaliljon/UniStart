using UniStart.Services.AI;

namespace UniStart.Services.BackgroundServices;

/// <summary>
/// Фоновый сервис для автоматического переобучения ML модели
/// Запускается раз в неделю (воскресенье в 3:00)
/// </summary>
public class MLRetrainingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MLRetrainingBackgroundService> _logger;
    private readonly TimeSpan _retrainInterval = TimeSpan.FromDays(7); // Раз в неделю

    public MLRetrainingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MLRetrainingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ML Retraining Background Service запущен");

        // Ждем 1 минуту после старта приложения перед первым запуском
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformRetraining(stoppingToken);
                
                // Вычисляем время до следующего воскресенья в 3:00
                var nextRun = GetNextRunTime();
                var delay = nextRun - DateTime.UtcNow;
                
                _logger.LogInformation("Следующее переобучение запланировано на {NextRun}", nextRun);
                
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ML Retraining Background Service остановлен");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в ML Retraining Background Service");
                // Ждем 1 час перед повторной попыткой
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task PerformRetraining(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mlService = scope.ServiceProvider.GetRequiredService<IMLPredictionService>();
        var trainingDataService = scope.ServiceProvider.GetRequiredService<IMLTrainingDataService>();

        try
        {
            _logger.LogInformation("Начинается автоматическое переобучение ML модели");

            // Получаем статистику
            var stats = await trainingDataService.GetTrainingStatsAsync();
            
            if (!stats.CanTrain)
            {
                _logger.LogWarning(
                    "Недостаточно данных для переобучения. Текущее количество: {Count}, требуется минимум 100",
                    stats.TotalRecords);
                return;
            }

            // Запускаем переобучение
            var success = await mlService.RetrainModelAsync();

            if (success)
            {
                _logger.LogInformation(
                    "ML модель успешно переобучена. Использовано записей: {Count}, Уникальных пользователей: {Users}",
                    stats.TotalRecords,
                    stats.UniqueUsers);
            }
            else
            {
                _logger.LogWarning("Переобучение ML модели завершилось неудачно");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при автоматическом переобучении ML модели");
        }
    }

    private DateTime GetNextRunTime()
    {
        var now = DateTime.UtcNow;
        
        // Находим следующее воскресенье
        int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && now.Hour >= 3)
        {
            daysUntilSunday = 7; // Если сегодня воскресенье после 3:00, берем следующее
        }

        var nextSunday = now.Date.AddDays(daysUntilSunday);
        var nextRun = nextSunday.AddHours(3); // 3:00 утра

        return nextRun;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ML Retraining Background Service останавливается...");
        await base.StopAsync(stoppingToken);
    }
}
