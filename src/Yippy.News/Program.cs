using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Yippy.Common;
using Yippy.Common.Authentication;
using Yippy.Identity;
using Yippy.Messaging;
using Yippy.News;
using Yippy.News.Data;
using Yippy.News.Services;
using Yippy.News.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) =>
{
    config.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("YippyContext")!);

builder.Services.Configure<MessageBrokerOptions>(builder.Configuration.GetSection(MessageBrokerOptions.Name));
builder.Services.AddRabbitMqMessagingService();

builder.Services.AddDbContext<YippyNewsDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("YippyContext")));

builder.Services.AddYippyAuthentication(authorize =>
{
    authorize.DefaultPolicy = new AuthorizationPolicyBuilder(
            YippyAuthenticationHandler.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddGrpcClient<TokenValidation.TokenValidationClient>(
    opt => opt.Address = new Uri(builder.Configuration["Backends:Identity"]!));

builder.Services.AddValidatorsFromAssemblyContaining<PostCreateRequestValidator>();

builder.Services.AddScoped<DbRightsCheckingService>();
builder.Services.AddScoped<IPostService, PostService>();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetRequiredService<YippyNewsDbContext>();
await db.Database.MigrateAsync();

app.UseYippySerilogRequestLogging();
app.MapHealthChecks("/health");
app.UseYippyAuthentication();
app.MapYippyApi();

app.Run();