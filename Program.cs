using System.Diagnostics;
using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using UniStart.Application.Interfaces;
using UniStart.Application.Services;
using UniStart.Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.FeatureManagement;
using UniStart.Application.Validators;
using UniStart.Hubs;
using UniStart.Infrastructure.Data;
using UniStart.Infrastructure.Repositories;

// ═══════════════════════════════════════════════════════════
//  SERILOG BOOTSTRAP (OP-5)
// ═══════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/unistart-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting UniStart application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // ═══════════════════════════════════════════════════════
    //  DATABASE (OP-1: connection string from env var if set)
    // ═══════════════════════════════════════════════════════
    var connectionString = Environment.GetEnvironmentVariable("UNISTART_DB_CONNECTION")
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<UniStartDbContext>(options =>
        options.UseNpgsql(connectionString));

    // ═══════════════════════════════════════════════════════
    //  REPOSITORIES & SERVICES
    // ═══════════════════════════════════════════════════════
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IExamService, ExamService>();
    builder.Services.AddScoped<IAdaptiveEngineService, AdaptiveEngineService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddScoped<IStudyPlanService, StudyPlanService>();
    builder.Services.AddScoped<IScorePredictionService, ScorePredictionService>();
    builder.Services.AddScoped<IRecommendationService, RecommendationService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<ILessonService, LessonService>();
    builder.Services.AddScoped<IMockExamService, MockExamService>();
    builder.Services.AddScoped<IOnboardingService, OnboardingService>();
    builder.Services.AddScoped<IDiagnosticService, DiagnosticService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IBackgroundJobsService, BackgroundJobsService>();
    builder.Services.AddScoped<ITutorService, TutorService>();
    builder.Services.AddScoped<IMessageService, MessageService>();

    // Learning v2 services (TH-1..TH-6)
    builder.Services.AddScoped<IFormulaService, FormulaService>();
    builder.Services.AddScoped<IFlashcardService, FlashcardService>();
    builder.Services.AddScoped<ITimedDrillService, TimedDrillService>();
    builder.Services.AddScoped<IStrategyService, StrategyService>();
    builder.Services.AddScoped<IMistakeService, MistakeService>();

    // ═══════════════════════════════════════════════════════
    //  PRESENCE TRACKER — Singleton (T-9)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddSingleton<PresenceTracker>();

    // ═══════════════════════════════════════════════════════
    //  SIGNALR — Real-time Chat
    // ═══════════════════════════════════════════════════════
    builder.Services.AddSignalR();

    // ═══════════════════════════════════════════════════════
    //  HANGFIRE — Background Job Processing (OP-12)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(opts =>
            opts.UseNpgsqlConnection(connectionString)));
    builder.Services.AddHangfireServer(opts =>
    {
        opts.WorkerCount = 2;
        opts.Queues = new[] { "default", "emails" };
    });

    // ═══════════════════════════════════════════════════════
    //  FLUENT VALIDATION (OP-11)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestionDtoValidator>();

    // ═══════════════════════════════════════════════════════
    //  FEATURE FLAGS (OP-22)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddFeatureManagement();

    // ═══════════════════════════════════════════════════════
    //  CACHING (OP-10)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddMemoryCache();

    // ═══════════════════════════════════════════════════════
    //  RESPONSE COMPRESSION (OP-17)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddResponseCompression(opts =>
    {
        opts.EnableForHttps = true;
    });

    // ═══════════════════════════════════════════════════════
    //  JWT AUTHENTICATION (OP-1: secret from env var if set)
    // ═══════════════════════════════════════════════════════
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = Environment.GetEnvironmentVariable("UNISTART_JWT_SECRET")
                    ?? jwtSettings["SecretKey"]
                    ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        // SignalR passes JWT via query string for WebSocket connections
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

    builder.Services.AddAuthorization();

    // ═══════════════════════════════════════════════════════
    //  RATE LIMITING (OP-2)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Strict limit for auth endpoints (brute-force protection)
        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });

        // General API limit per user/IP
        options.AddSlidingWindowLimiter("api", opt =>
        {
            opt.PermitLimit = 120;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.SegmentsPerWindow = 4;
            opt.QueueLimit = 0;
        });

        // Global fallback
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

    // ═══════════════════════════════════════════════════════
    //  HEALTH CHECKS (OP-6)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString!, name: "postgresql", tags: new[] { "db", "ready" });

    // ═══════════════════════════════════════════════════════
    //  CONTROLLERS + API VERSIONING (OP-15)
    // ═══════════════════════════════════════════════════════
    builder.Services.AddControllers();

    builder.Services.AddApiVersioning(opts =>
    {
        opts.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        opts.AssumeDefaultVersionWhenUnspecified = true;
        opts.ReportApiVersions = true;
        // Non-breaking: clients can optionally pass header or query string
        opts.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
            new Asp.Versioning.HeaderApiVersionReader("x-api-version"),
            new Asp.Versioning.QueryStringApiVersionReader("api-version")
        );
    })
    .AddApiExplorer(opts =>
    {
        opts.GroupNameFormat = "'v'VVV";
    });

    // ═══════════════════════════════════════════════════════
    //  CORS (OP-19: origins from config)
    // ═══════════════════════════════════════════════════════
    var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:3000", "http://localhost:5173" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactApp", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // ═══════════════════════════════════════════════════════
    //  SWAGGER
    // ═══════════════════════════════════════════════════════
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "UniStart API",
            Version = "v1",
            Description = "Adaptive SAT/TOEFL/NUET Preparation Platform API"
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

    var app = builder.Build();

    // ═══════════════════════════════════════════════════════
    //  GLOBAL EXCEPTION HANDLER (OP-4)
    // ═══════════════════════════════════════════════════════
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;
            var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

            // Log the exception
            Log.Error(exception, "Unhandled exception for request {RequestId} {Method} {Path}",
                requestId, context.Request.Method, context.Request.Path);

            // Map exception type to HTTP status code
            var (statusCode, title) = exception switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = $"https://httpstatuses.com/{statusCode}",
                Extensions = { ["requestId"] = requestId }
            };

            // Only include details in development
            if (app.Environment.IsDevelopment() && exception != null)
            {
                problem.Detail = exception.Message;
                problem.Extensions["stackTrace"] = exception.StackTrace;
            }
            else if (statusCode < 500 && exception != null)
            {
                // For 4xx errors, include user-friendly message
                problem.Detail = exception.Message;
            }

            await context.Response.WriteAsJsonAsync(problem);
        });
    });

    // ═══════════════════════════════════════════════════════
    //  MIDDLEWARE PIPELINE
    // ═══════════════════════════════════════════════════════

    // Response compression (before everything)
    app.UseResponseCompression();

    // Serilog request logging (OP-5)
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

    // Security headers (OP-3)
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        await next();
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // Swagger (dev only)
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

    // Rate limiting (OP-2)
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    // ═══════════════════════════════════════════════════════
    //  BLOCKED USER CHECK (OP-14)
    // ═══════════════════════════════════════════════════════
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                var db = context.RequestServices.GetRequiredService<UniStart.Infrastructure.Data.UniStartDbContext>();
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user is { IsBlocked: true })
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                        title = "Account Blocked",
                        status = 403,
                        detail = user.BlockReason ?? "Your account has been blocked. Contact support."
                    });
                    return;
                }
            }
        }
        await next();
    });

    app.MapControllers();
    app.MapHub<ChatHub>("/hubs/chat");

    // ═══════════════════════════════════════════════════════
    //  HEALTH CHECK ENDPOINTS (OP-6)
    // ═══════════════════════════════════════════════════════
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false // liveness — just checks process is alive
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

    // ═══════════════════════════════════════════════════════
    //  HANGFIRE DASHBOARD & RECURRING JOBS (OP-12)
    // ═══════════════════════════════════════════════════════
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAdminAuthFilter() },
        DashboardTitle = "UniStart Jobs"
    });

    // Register recurring jobs
    RecurringJob.AddOrUpdate<IBackgroundJobsService>(
        "streak-reminder",
        service => service.ProcessStreakRemindersAsync(),
        "0 */6 * * *"); // every 6 hours

    RecurringJob.AddOrUpdate<IBackgroundJobsService>(
        "weekly-digest",
        service => service.ProcessWeeklyDigestsAsync(),
        "0 8 * * 1"); // Mondays at 08:00 UTC

    // ═══════════════════════════════════════════════════════
    //  AUTO-MIGRATE & SEED (dev only)
    // ═══════════════════════════════════════════════════════
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UniStartDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = new DatabaseSeeder(dbContext);
        await seeder.SeedAsync();

        var expansionSeeder = new QuestionExpansionSeeder(dbContext);
        await expansionSeeder.SeedAsync();
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
