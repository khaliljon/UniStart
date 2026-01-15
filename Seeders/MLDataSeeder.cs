using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Core;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;

namespace UniStart.Seeders;

/// <summary>
/// Seeder для генерации тестовых данных для ML обучения
/// ВАЖНО: Используется только в Development окружении!
/// </summary>
public class MLDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MLDataSeeder> _logger;

    public MLDataSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<MLDataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Начало генерации ML тестовых данных...");

            // 1. Создаем тестовых пользователей (студентов)
            await SeedTestUsersAsync();

            // 2. Создаем тестовые FlashcardSets и Flashcards
            await SeedTestFlashcardsAsync();

            _logger.LogInformation("✅ ML тестовые данные успешно созданы");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при создании ML тестовых данных");
            throw;
        }
    }

    private async Task SeedTestUsersAsync()
    {
        const int usersCount = 30; // Генерируем 30 тестовых студентов
        var existingTestUsers = await _userManager.Users
            .Where(u => u.Email!.StartsWith("ml_test_student"))
            .CountAsync();

        if (existingTestUsers >= usersCount)
        {
            _logger.LogInformation("Тестовые пользователи уже существуют ({Count}), пропускаем генерацию", existingTestUsers);
            return;
        }

        var usersToCreate = usersCount - existingTestUsers;
        _logger.LogInformation("Создание {Count} тестовых пользователей для ML...", usersToCreate);

        for (int i = existingTestUsers + 1; i <= usersCount; i++)
        {
            var user = new ApplicationUser
            {
                UserName = $"ml_test_student{i}@unistart.local",
                Email = $"ml_test_student{i}@unistart.local",
                EmailConfirmed = true,
                FirstName = $"MLTest{i}",
                LastName = "Student",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, "Test123!");
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");
            }
            else
            {
                _logger.LogWarning("Не удалось создать пользователя {Email}: {Errors}", 
                    user.Email, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✅ Создано {Count} тестовых пользователей", usersToCreate);
    }

    private async Task SeedTestFlashcardsAsync()
    {
        const int flashcardsCount = 150; // Генерируем 150 карточек
        
        // Получаем тестового преподавателя или создаем нового
        var teacher = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == "ml_test_teacher@unistart.local");

        if (teacher == null)
        {
            teacher = new ApplicationUser
            {
                UserName = "ml_test_teacher@unistart.local",
                Email = "ml_test_teacher@unistart.local",
                EmailConfirmed = true,
                FirstName = "MLTest",
                LastName = "Teacher",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(teacher, "Test123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(teacher, "Teacher");
                _logger.LogInformation("✅ Создан тестовый преподаватель для ML данных");
            }
        }

        // Проверяем существующие тестовые наборы
        var existingSet = await _context.Set<FlashcardSet>()
            .FirstOrDefaultAsync(fs => fs.Title == "ML Test Dataset");

        FlashcardSet testSet;
        
        if (existingSet == null)
        {
            testSet = new FlashcardSet
            {
                Title = "ML Test Dataset",
                Description = "Набор карточек для тестирования ML модели",
                Subject = "ML Testing",
                UserId = teacher.Id,
                IsPublic = true,
                IsPublished = true
            };
            _context.Set<FlashcardSet>().Add(testSet);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("✅ Создан тестовый набор карточек");
        }
        else
        {
            testSet = existingSet;
        }

        // Проверяем сколько карточек уже есть
        var existingFlashcards = await _context.Set<Flashcard>()
            .CountAsync(f => f.FlashcardSetId == testSet.Id);

        if (existingFlashcards >= flashcardsCount)
        {
            _logger.LogInformation("Тестовые карточки уже существуют ({Count}), пропускаем генерацию", existingFlashcards);
            return;
        }

        var flashcardsToCreate = flashcardsCount - existingFlashcards;
        _logger.LogInformation("Создание {Count} тестовых карточек...", flashcardsToCreate);

        // Создаем карточки по разным темам
        var topics = new[]
        {
            "Programming",
            "Mathematics",
            "Physics",
            "Chemistry",
            "Biology",
            "History",
            "Geography",
            "English"
        };

        for (int i = existingFlashcards + 1; i <= flashcardsCount; i++)
        {
            var topic = topics[i % topics.Length];
            
            var flashcard = new Flashcard
            {
                FlashcardSetId = testSet.Id,
                Question = $"Test Question {i} ({topic})",
                Answer = $"Test Answer {i} for {topic}",
                Type = FlashcardType.SingleChoice // Простой текстовый вопрос
            };

            _context.Set<Flashcard>().Add(flashcard);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✅ Создано {Count} тестовых карточек", flashcardsToCreate);
    }

    /// <summary>
    /// Удаляет все тестовые данные, созданные этим seeder'ом
    /// </summary>
    public async Task CleanupAsync()
    {
        try
        {
            _logger.LogInformation("Начало очистки ML тестовых данных...");

            // Удаляем тестовых пользователей
            var testUsers = await _userManager.Users
                .Where(u => u.Email!.StartsWith("ml_test_"))
                .ToListAsync();

            foreach (var user in testUsers)
            {
                await _userManager.DeleteAsync(user);
            }

            // Удаляем тестовые наборы карточек
            var testSets = await _context.Set<FlashcardSet>()
                .Where(fs => fs.Title == "ML Test Dataset")
                .ToListAsync();

            _context.Set<FlashcardSet>().RemoveRange(testSets);

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ ML тестовые данные успешно удалены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при очистке ML тестовых данных");
            throw;
        }
    }
}
