using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Yippy.Common;
using Yippy.Common.Identity;
using Yippy.Identity;
using Yippy.Identity.Data;
using Yippy.Identity.Services;
using Yippy.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddGrpc();
builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("YippyContext")!);

builder.Services.Configure<MessageBrokerOptions>(builder.Configuration.GetSection(MessageBrokerOptions.Name));
builder.Services.AddRabbitMqMessagingService();
builder.Services.AddMessagePublisher<TokenGeneratedMessage>();

builder.Services.AddDbContext<YippyIdentityDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("YippyContext")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<JwtService>();

var app = builder.Build();

app.MapDefaultEndpoints();

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetRequiredService<YippyIdentityDbContext>();
await db.Database.MigrateAsync();

app.UseYippySerilogRequestLogging();
app.MapGrpcService<TokenValidationService>();
app.UseAuthentication();
app.UseAuthorization();
app.MapYippyApi();

app.Run();