using Microsoft.EntityFrameworkCore;
using Serilog;
using Yippy.Caching;
using Yippy.Common;
using Yippy.Templating.Data;
using Yippy.Templating.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) =>
{
    config.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddGrpc();
builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("YippyContext")!);
builder.Services.AddYippyCaching(builder.Configuration);

builder.Services.AddDbContext<YippyTemplatingDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("YippyContext")));

builder.Services.AddSingleton<ITemplateVariableProcessor, TemplateVariableProcessor>();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetRequiredService<YippyTemplatingDbContext>();
await db.Database.MigrateAsync();

app.UseYippySerilogRequestLogging();
app.MapHealthChecks("/health");
app.MapGrpcService<TemplateResolvingService>();

app.Run();