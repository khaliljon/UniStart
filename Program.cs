using System.Diagnostics;
using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Startup;

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

    builder.Configuration.AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.local.json",
        optional: true, reloadOnChange: true);

    builder.Host.UseSerilog();

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 100_000_000;
    });

    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 100_000_000;
    });

    var connectionString = Environment.GetEnvironmentVariable("UNISTART_DB_CONNECTION")
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddUniStartServices(builder.Configuration, connectionString!);

    var app = builder.Build();

    if (args.Contains("--email-preview"))
    {
        await app.RenderEmailPreviewsAsync();
        return;
    }

    using (var startupScope = app.Services.CreateScope())
    {
        var llm = startupScope.ServiceProvider.GetRequiredService<ILlmExtractionService>();
        Log.Information("LLM question extraction configured: {Configured}", llm.IsConfigured);
    }

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;
            var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

            Log.Error(exception, "Unhandled exception for request {RequestId} {Method} {Path}",
                requestId, context.Request.Method, context.Request.Path);

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

            if (app.Environment.IsDevelopment() && exception != null)
            {
                problem.Detail = exception.Message;
                problem.Extensions["stackTrace"] = exception.StackTrace;
            }
            else if (statusCode < 500 && exception != null)
            {
                problem.Detail = exception.Message;
            }

            await context.Response.WriteAsJsonAsync(problem);
        });
    });

    app.UseUniStartPipeline();

    await app.MigrateAndSeedAsync();

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
