using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.AspNetCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.FeatureManagement;
using UniStart.Application.Interfaces;
using UniStart.Application.Services;
using UniStart.Application.Validators;
using UniStart.Domain.Interfaces;
using UniStart.Hubs;
using UniStart.Infrastructure.Data;
using UniStart.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace UniStart.Infrastructure.Startup;

public static class UniStartStartupExtensions
{
    public static IServiceCollection AddUniStartServices(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddHttpContextAccessor();

        // ── Data Protection ──────────────────────────────────
        // Persist keys to a stable location so they survive container
        // restarts/rebuilds. Without this, ASP.NET regenerates the key ring on
        // every start (ephemeral container FS), which invalidates antiforgery
        // tokens and anything encrypted via the Data Protection API, and floods
        // the logs with "No XML encryptor configured" / key-not-found warnings.
        var dpBuilder = services.AddDataProtection().SetApplicationName("UniStart");
        var keysPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_DIR");
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            System.IO.Directory.CreateDirectory(keysPath);
            dpBuilder.PersistKeysToFileSystem(new System.IO.DirectoryInfo(keysPath));
        }

        services.AddDbContext<UniStartDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IAdaptiveEngineService, AdaptiveEngineService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IStudyPlanService, StudyPlanService>();
        services.AddScoped<IScorePredictionService, ScorePredictionService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IMockExamService, MockExamService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IDiagnosticService, DiagnosticService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBackgroundJobsService, BackgroundJobsService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<ITutorService, TutorService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IFormulaService, FormulaService>();
        services.AddScoped<IFlashcardService, FlashcardService>();
        services.AddScoped<ITimedDrillService, TimedDrillService>();
        services.AddScoped<IStrategyService, StrategyService>();
        services.AddScoped<IMistakeService, MistakeService>();
        services.AddScoped<IFileParserService, FileParserService>();
        services.AddScoped<IQuestionExtractorService, QuestionExtractorService>();
        services.AddScoped<IQuestionImportService, QuestionImportService>();
        services.AddSingleton<ILlmExtractionService, LlmExtractionService>();
        services.AddSingleton<IImageUploadService, ImageUploadService>();

        services.AddSingleton<PresenceTracker>();

        services.AddSignalR();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = 2;
            opts.Queues = new[] { "default", "emails" };
        });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateQuestionDtoValidator>();

        services.AddFeatureManagement();

        services.AddMemoryCache();

        services.AddResponseCompression(opts =>
        {
            opts.EnableForHttps = true;
        });

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Environment.GetEnvironmentVariable("UNISTART_JWT_SECRET")
                        ?? jwtSettings["SecretKey"]
                        ?? throw new InvalidOperationException("JWT SecretKey not configured");

        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 20;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.AddSlidingWindowLimiter("api", opt =>
            {
                opt.PermitLimit = 120;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 4;
                opt.QueueLimit = 0;
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0,
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = 429,
                    Title = "Too Many Requests",
                    Detail = "Слишком много запросов. Подождите немного и попробуйте снова.",
                    Type = "https://httpstatuses.com/429"
                }, cancellationToken);
            };
        });

        services.AddHealthChecks()
            .AddNpgSql(connectionString!, name: "postgresql", tags: new[] { "db", "ready" });

        services.AddControllers();

        services.AddApiVersioning(opts =>
        {
            opts.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ReportApiVersions = true;
            opts.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
                new Asp.Versioning.HeaderApiVersionReader("x-api-version"),
                new Asp.Versioning.QueryStringApiVersionReader("api-version")
            );
        })
        .AddApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
        });

        var corsOrigins = configuration.GetSection("CorsOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("ReactApp", policy =>
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                      .AllowCredentials();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UniStart API",
                Version = "v1",
                Description = "CSCA Preparation Platform API"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        return services;
    }

    public static WebApplication UseUniStartPipeline(this WebApplication app)
    {
        app.UseResponseCompression();

        app.Use(async (context, next) =>
        {
            if (context.Request.Headers.TryGetValue("X-Request-Id", out var incoming) && !string.IsNullOrWhiteSpace(incoming))
            {
                context.TraceIdentifier = incoming!;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
                return Task.CompletedTask;
            });

            await next();
        });

        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null) diagnosticContext.Set("UserId", userId);
            };
        });

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "0");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'unsafe-inline' https://accounts.google.com; style-src 'self' 'unsafe-inline' https://accounts.google.com; img-src 'self' data: https:; font-src 'self' https://fonts.gstatic.com; connect-src 'self' wss: ws: https://accounts.google.com; frame-src https://accounts.google.com; frame-ancestors 'none'; base-uri 'self'; form-action 'self'");
            await next();
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
                await next();
            });
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UniStart API v1");
            });
        }

        app.UseHttpsRedirection();

        app.UseCors("ReactApp");

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheKey = $"blocked:{userId}";
                    if (!cache.TryGetValue(cacheKey, out object? cached) || cached is not bool isBlocked)
                    {
                        var db = context.RequestServices.GetRequiredService<UniStartDbContext>();
                        var user = await db.Users.AsNoTracking()
                            .Where(u => u.Id == userId)
                            .Select(u => new { u.IsBlocked, u.BlockReason })
                            .FirstOrDefaultAsync();

                        isBlocked = user?.IsBlocked == true;
                        using var entry = cache.CreateEntry(cacheKey);
                        entry.Value = isBlocked;
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

                        if (isBlocked)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new
                            {
                                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                                title = "Account Blocked",
                                status = 403,
                                detail = user?.BlockReason ?? "Your account has been blocked. Contact support."
                            });
                            return;
                        }
                    }
                    else if ((bool)cached)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                            title = "Account Blocked",
                            status = 403,
                            detail = "Your account has been blocked. Contact support."
                        });
                        return;
                    }
                }
            }

            await next();
        });

        app.MapControllers();
        app.MapHub<ChatHub>("/hubs/chat");

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds + "ms",
                        exception = e.Value.Exception?.Message
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
                });
            }
        });

        app.MapHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAdminAuthFilter() },
            DashboardTitle = "UniStart Jobs"
        });

        RecurringJob.AddOrUpdate<IBackgroundJobsService>(
            "streak-reminder",
            service => service.ProcessStreakRemindersAsync(),
            "0 */6 * * *");

        RecurringJob.AddOrUpdate<IBackgroundJobsService>(
            "weekly-digest",
            service => service.ProcessWeeklyDigestsAsync(),
            "0 8 * * 1");

        RecurringJob.AddOrUpdate<IBackgroundJobsService>(
            "soft-delete-purge",
            service => service.PurgeSoftDeletedRecordsAsync(),
            "0 2 * * *");

        RecurringJob.AddOrUpdate<IBackgroundJobsService>(
            "irt-calibration",
            service => service.CalibrateIrtParametersAsync(),
            "0 3 * * *");

        RecurringJob.AddOrUpdate<IBackupService>(
            "db-backup",
            service => service.CreateBackupAsync("scheduled"),
            "0 1 * * *");

        return app;
    }

    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UniStartDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = new DatabaseSeeder(dbContext);
        await seeder.SeedAsync();
    }
}
