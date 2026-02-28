using Hangfire.Dashboard;

namespace UniStart.Application.Services;

/// <summary>
/// Hangfire dashboard authorization filter — allows only Admin role
/// or all requests in Development environment.
/// </summary>
public class HangfireAdminAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // In Development, allow all access
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
            return true;

        // In production, require Admin role
        return httpContext.User.Identity?.IsAuthenticated == true
               && httpContext.User.IsInRole("Admin");
    }
}
