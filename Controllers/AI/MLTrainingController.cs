using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.DTOs;
using UniStart.Services.AI;

namespace UniStart.Controllers.AI;

/// <summary>
/// Контроллер для управления обучением ML модели
/// </summary>
[Route("api/mltraining")]
[Authorize(Roles = "Admin,Teacher")]
public class MLTrainingController : BaseApiController
{
    private readonly IMLPredictionService _mlService;
    private readonly IMLTrainingDataService _trainingDataService;
    private readonly ILogger<MLTrainingController> _logger;

    public MLTrainingController(
        IMLPredictionService mlService,
        IMLTrainingDataService trainingDataService,
        ILogger<MLTrainingController> logger)
    {
        _mlService = mlService;
        _trainingDataService = trainingDataService;
        _logger = logger;
    }

    /// <summary>
    /// Добавить тестовые данные для обучения модели вручную
    /// </summary>
    [HttpPost("training-data")]
    public async Task<IActionResult> AddTrainingData([FromBody] List<ManualTrainingDataDto> data)
    {
        try
        {
            var result = await _trainingDataService.AddManualTrainingDataAsync(data);
            
            if (result.Success)
            {
                return Ok(new 
                { 
                    message = "Тестовые данные успешно добавлены",
                    recordsAdded = result.RecordsAdded,
                    canTrain = result.TotalRecords >= 100,
                    totalRecords = result.TotalRecords
                });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении тестовых данных");
            return StatusCode(500, new { message = "Ошибка при добавлении данных" });
        }
    }

    /// <summary>
    /// Импортировать данные из CSV файла
    /// </summary>
    [HttpPost("import-csv")]
    public async Task<IActionResult> ImportFromCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Файл не предоставлен" });

        try
        {
            var result = await _trainingDataService.ImportFromCsvAsync(file);
            
            return Ok(new
            {
                message = "Данные импортированы",
                recordsAdded = result.RecordsAdded,
                errors = result.Errors,
                canTrain = result.TotalRecords >= 100
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при импорте CSV");
            return StatusCode(500, new { message = "Ошибка импорта" });
        }
    }

    /// <summary>
    /// Запустить переобучение модели
    /// </summary>
    [HttpPost("retrain")]
    public async Task<IActionResult> RetrainModel()
    {
        try
        {
            var success = await _mlService.RetrainModelAsync();
            
            if (success)
            {
                return Ok(new 
                { 
                    message = "Модель успешно переобучена",
                    status = "trained"
                });
            }

            return BadRequest(new 
            { 
                message = "Недостаточно данных для обучения (минимум 100 записей)",
                status = "insufficient_data"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переобучении модели");
            return StatusCode(500, new { message = "Ошибка обучения" });
        }
    }

    /// <summary>
    /// Получить статистику по тренировочным данным
    /// </summary>
    [HttpGet("training-stats")]
    public async Task<IActionResult> GetTrainingStats()
    {
        try
        {
            var stats = await _trainingDataService.GetTrainingStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики");
            return StatusCode(500, new { message = "Ошибка получения статистики" });
        }
    }

    /// <summary>
    /// Генерировать синтетические тестовые данные
    /// </summary>
    [HttpPost("generate-synthetic-data")]
    public async Task<IActionResult> GenerateSyntheticData([FromQuery] int count = 100)
    {
        try
        {
            if (count < 10 || count > 10000)
                return BadRequest(new { message = "Количество должно быть от 10 до 10000" });

            var result = await _trainingDataService.GenerateSyntheticDataAsync(count);
            
            if (!result.Success)
            {
                return BadRequest(new 
                { 
                    message = result.ErrorMessage,
                    errors = result.Errors
                });
            }
            
            return Ok(new
            {
                message = $"Сгенерировано {result.RecordsAdded} записей. Всего: {result.TotalRecords}",
                recordsGenerated = result.RecordsAdded,
                totalRecords = result.TotalRecords,
                canTrain = result.TotalRecords >= 100,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка генерации синтетических данных");
            return StatusCode(500, new { message = "Ошибка генерации: " + ex.Message });
        }
    }

    /// <summary>
    /// Удаляет все синтетические тестовые данные
    /// </summary>
    [HttpDelete("synthetic-data")]
    public async Task<IActionResult> DeleteSyntheticData()
    {
        try
        {
            var deletedCount = await _trainingDataService.DeleteSyntheticDataAsync();
            
            return Ok(new
            {
                message = deletedCount > 0 
                    ? $"Удалено {deletedCount} синтетических записей" 
                    : "Синтетические данные не найдены",
                deletedCount,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении синтетических данных");
            return StatusCode(500, new { message = "Ошибка удаления: " + ex.Message });
        }
    }
}
