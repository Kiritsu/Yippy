using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Serilog;
using Yippy.Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(x =>
{
    x.RejectionStatusCode = 429;
    x.AddPolicy("AuthPolicy", context =>
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var callerIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        var authToken = context.Request.Headers.Authorization.ToString();
        
        var key = (!string.IsNullOrWhiteSpace(authToken) ? authToken : null) ?? forwardedFor ?? callerIp ?? "default-key-unknown-caller";
        var hashKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        
        return RateLimitPartition.GetSlidingWindowLimiter(hashKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
            SegmentsPerWindow = 5
        });
    });
});

builder.Services.AddCors();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    context.Request.Headers.Append("X-Yippy-Trace", Guid.NewGuid().ToString());
    await next();
});
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseYippySerilogRequestLogging();
app.UseRateLimiter();
app.MapReverseProxy();

app.Run();