using Microsoft.EntityFrameworkCore;
using Yippy.Common;
using Yippy.Common.Identity;
using Yippy.Emailing;
using Yippy.Emailing.Data;
using Yippy.Messaging;
using Yippy.Templating;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("YippyContext")!);

builder.Services.Configure<MessageBrokerOptions>(builder.Configuration.GetSection(MessageBrokerOptions.Name));
builder.Services.AddRabbitMqMessagingService();
builder.Services.AddMessageConsumer<TokenGeneratedMessage, TokenGeneratedConsumer>();

builder.Services.AddDbContext<EmailDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("YippyContext")));

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<EmailingService>();
builder.Services.AddHostedService(x => x.GetRequiredService<EmailingService>());

builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddGrpcClient<TemplateResolver.TemplateResolverClient>(
    opt => opt.Address = new Uri(builder.Configuration["Backends:Templating"]!));

var app = builder.Build();

app.MapDefaultEndpoints();

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetRequiredService<EmailDbContext>();
await db.Database.MigrateAsync();

app.UseYippySerilogRequestLogging();

app.Run();
