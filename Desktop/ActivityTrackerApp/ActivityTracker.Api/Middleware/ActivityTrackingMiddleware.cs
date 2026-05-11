using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;

namespace ActivityTracker.Api.Middleware;

public class ActivityTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public ActivityTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IActivityLogRepository activityLogRepository, ICurrentUserService currentUserService)
    {
        await _next(context);

        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            return;
        }

        var statusCode = context.Response.StatusCode;
        var activityType = statusCode >= 400 ? "ApiFailure" : "ApiRequest";

        await activityLogRepository.AddAsync(new ActivityLog
        {
            UserId = currentUserService.UserId,
            ActivityType = activityType,
            Description = $"{context.Request.Method} {context.Request.Path}",
            Endpoint = context.Request.Path,
            Method = context.Request.Method,
            StatusCode = statusCode,
            IpAddress = currentUserService.IpAddress,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
