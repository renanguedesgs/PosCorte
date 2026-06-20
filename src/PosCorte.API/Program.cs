using Serilog;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;
using PosCorte.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// ===== SERILOG =====
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/poscorte-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "PosCorte.API")
    .CreateLogger();

builder.Host.UseSerilog();

// ===== SERVICES =====
builder.Services.AddScoped<IPrecificacaoService, PrecificacaoService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

// ===== AUTH SERVICE =====
builder.Services.AddScoped<IAuthService, AuthService>();

// ===== JWT AUTHENTICATION =====
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// ===== BANCO DE DADOS - Supabase (PostgreSQL) =====
builder.Services.AddDbContext<PosCorteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios com EF Core
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioEF<>));

// Refit HTTP Client para integração com Provedor externo
builder.Services.AddRefitClient<IProvedorApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(
        builder.Configuration["ProvedorApi:BaseUrl"] ?? "https://api.provider.com"));

builder.Services.AddScoped<IProvedorService, ProvedorService>();

// ===== CONTROLLERS & SWAGGER =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PósCorte API",
        Version = "v1",
        Description = "API de intermediação de serviços de montagem de móveis planejados",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PósCorte",
            Url = new Uri("https://poscorte.com")
        }
    });
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== MIGRATIONS AUTOMÁTICAS =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PosCorteDbContext>();
    db.Database.Migrate();
}

// ===== MIDDLEWARE PIPELINE =====
app.UseMiddleware<PosCorte.API.Middleware.ErroMiddleware>();
app.UseMiddleware<PosCorte.API.Middleware.LoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PósCorte API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
