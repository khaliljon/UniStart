using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
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
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MLTrainingController(
        IMLPredictionService mlService,
        IMLTrainingDataService trainingDataService,
        ILogger<MLTrainingController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _mlService = mlService;
        _trainingDataService = trainingDataService;
        _logger = logger;
        _context = context;
        _userManager = userManager;
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
    /// Удаляет все тестовые данные из ML сидера (пользователи, карточки)
    /// </summary>
    [HttpDelete("test-data")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTestData()
    {
        try
        {
            var deletedUsers = 0;
            var deletedFlashcardSets = 0;
            var deletedFlashcards = 0;

            // 1. Удаляем тестовых пользователей
            var testUsers = await _userManager.Users
                .Where(u => u.Email!.StartsWith("ml_test_student"))
                .ToListAsync();

            foreach (var user in testUsers)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    deletedUsers++;
            }

            // 2. Удаляем тестовые flashcard sets
            var testSets = await _context.FlashcardSets
                .Include(fs => fs.Flashcards)
                .Where(fs => fs.Title == "ML Test Dataset")
                .ToListAsync();

            foreach (var set in testSets)
            {
                deletedFlashcards += set.Flashcards?.Count ?? 0;
                _context.FlashcardSets.Remove(set);
                deletedFlashcardSets++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Удалены тестовые данные: {Users} пользователей, {Sets} наборов, {Cards} карточек",
                deletedUsers, deletedFlashcardSets, deletedFlashcards);

            return Ok(new
            {
                message = "Тестовые данные успешно удалены",
                deletedUsers,
                deletedFlashcardSets,
                deletedFlashcards,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении тестовых данных");
            return StatusCode(500, new { message = "Ошибка удаления: " + ex.Message });
        }
    }
}
