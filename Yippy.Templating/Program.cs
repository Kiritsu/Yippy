using Yippy.Caching;
using Yippy.Common;
using Yippy.Templating.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("YippyContext")!);
builder.Services.AddYippyCaching(builder.Configuration);

builder.Services.AddSingleton<ITemplateVariableProcessor, TemplateVariableProcessor>();

var app = builder.Build();

app.UseYippySerilogRequestLogging();
app.MapHealthChecks("/health");
app.MapGrpcService<TemplateResolvingService>();

app.Run();