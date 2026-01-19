using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UniStart.Data;
using UniStart.Middleware;
using UniStart.Models;
using UniStart.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Игнорировать циклические ссылки (Flashcard <-> FlashcardSet, Quiz <-> Question)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // Для более читаемого JSON (опционально в production можно убрать)
        options.JsonSerializerOptions.WriteIndented = true;
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Автоматически находит все validators

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Application Services
builder.Services.AddScoped<ISpacedRepetitionService, SpacedRepetitionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<ITeacherStatisticsService, TeacherStatisticsService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// AI Services
builder.Services.AddScoped<UniStart.Services.AI.IMLPredictionService, UniStart.Services.AI.MLPredictionService>();
builder.Services.AddScoped<UniStart.Services.AI.IMLTrainingDataService, UniStart.Services.AI.MLTrainingDataService>();
builder.Services.AddScoped<UniStart.Services.AI.IUniversityRecommendationService, UniStart.Services.AI.UniversityRecommendationService>();
builder.Services.AddScoped<UniStart.Services.AI.IContentRecommendationService, UniStart.Services.AI.ContentRecommendationService>();
builder.Services.AddScoped<UniStart.Services.AI.IAIContentGeneratorService, UniStart.Services.AI.AIContentGeneratorService>();
builder.Services.AddScoped<UniStart.Services.AI.IAIFlashcardGeneratorService, UniStart.Services.AI.AIFlashcardGeneratorService>();

// AI Settings
builder.Services.Configure<UniStart.DTOs.AIServiceSettings>(
    builder.Configuration.GetSection("AI:Flashcards"));

// Background Services
builder.Services.AddHostedService<UniStart.Services.BackgroundServices.MLRetrainingBackgroundService>();

// Repository Pattern
builder.Services.AddScoped<UniStart.Repositories.IUnitOfWork, UniStart.Repositories.UnitOfWork>();
builder.Services.AddScoped<UniStart.Repositories.IQuizRepository, UniStart.Repositories.QuizRepository>();
builder.Services.AddScoped<UniStart.Repositories.IExamRepository, UniStart.Repositories.ExamRepository>();
builder.Services.AddScoped<UniStart.Repositories.IFlashcardSetRepository, UniStart.Repositories.FlashcardSetRepository>();
builder.Services.AddScoped<UniStart.Repositories.IUserQuizAttemptRepository, UniStart.Repositories.UserQuizAttemptRepository>();
builder.Services.AddScoped<UniStart.Repositories.IUserExamAttemptRepository, UniStart.Repositories.UserExamAttemptRepository>();
builder.Services.AddScoped<UniStart.Repositories.IUserFlashcardProgressRepository, UniStart.Repositories.UserFlashcardProgressRepository>();
builder.Services.AddScoped<UniStart.Repositories.IAchievementRepository, UniStart.Repositories.AchievementRepository>();
builder.Services.AddScoped(typeof(UniStart.Repositories.IRepository<>), typeof(UniStart.Repositories.Repository<>));

// CORS для фронтенда
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:5173") // React dev servers
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UniStart API",
        Version = "v1",
        Description = "API для образовательной платформы UniStart"
    });
    
    // Настройка JWT в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer {your token}"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed database with test data (только для разработки)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Применяем миграции автоматически (создаёт БД если её нет)
        await context.Database.MigrateAsync();
        
        // Инициализируем все начальные данные (роли, предметы, достижения, международные данные)
        await UniStart.Seeders.DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации базы данных");
    }
}

// Configure the HTTP request pipeline.

// Глобальная обработка ошибок
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UniStart API v1");
    });
}

// Статические файлы (wwwroot)
app.UseDefaultFiles(); // Использует index.html по умолчанию - ДОЛЖЕН БЫТЬ ПЕРЕД UseStaticFiles()
app.UseStaticFiles();

// Включаем CORS
app.UseCors("AllowReactApp");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Маршруты контроллеров
app.MapControllers();

app.Run();