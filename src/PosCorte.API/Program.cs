using Serilog;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;
using PosCorte.API.Data;
using Microsoft.EntityFrameworkCore;
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
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
