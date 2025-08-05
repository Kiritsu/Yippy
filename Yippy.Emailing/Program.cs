using Serilog;
using Yippy.Common;
using Yippy.Common.Identity;
using Yippy.Emailing;
using Yippy.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) =>
{
    config.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddHealthChecks();

builder.Services.Configure<MessageBrokerOptions>(builder.Configuration.GetSection(MessageBrokerOptions.Name));
builder.Services.AddRabbitMqMessagingService();
builder.Services.AddMessageConsumer<TokenGeneratedMessage, TokenGeneratedConsumer>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<EmailingService>();
builder.Services.AddHostedService(x => x.GetRequiredService<EmailingService>());

var app = builder.Build();

app.UseYippySerilogRequestLogging();
app.MapHealthChecks("/health");

app.Run();
