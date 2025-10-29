using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Yippy.Common;
using Yippy.Common.Identity;
using Yippy.Identity;
using Yippy.Identity.Data;
using Yippy.Identity.Configuration;
using Yippy.Identity.Services;
using Yippy.Messaging;
using Yippy.Caching;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
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

builder.Services.AddYippyCaching(builder.Configuration);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<JwtRevocationService>();

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