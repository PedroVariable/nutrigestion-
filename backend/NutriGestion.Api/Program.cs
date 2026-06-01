using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NutriGestion.Api.Configuration;
using NutriGestion.Api.Middleware;
using NutriGestion.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuración ──────────────────────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var configuracionMongo = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>()!;
var configuracionJwt   = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

// Fail-fast: si faltan secretos críticos, la app no arranca (mejor un error claro
// que ver requests fallar después con un mensaje confuso).
if (string.IsNullOrWhiteSpace(configuracionMongo.ConnectionString))
 throw new InvalidOperationException(
     "Falta MongoDb__ConnectionString. Configúrala como variable de entorno en producción " +
     "o en appsettings.Development.json para desarrollo local.");
if (string.IsNullOrWhiteSpace(configuracionJwt.Secret))
 throw new InvalidOperationException("Falta Jwt__Secret.");

// ── MongoDB — IMongoClient e IMongoDatabase como SINGLETON ─────────────────────
// Crítico: MongoClient maneja su propio pool de conexiones y es thread-safe.
// Debe ser singleton — crear uno por request hace DNS, TLS handshake y auth
// en cada petición, añadiendo 500ms–2s de latencia.
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(configuracionMongo.ConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(configuracionMongo.DatabaseName));

// ── Inyección de servicios ─────────────────────────────────────────────────────
builder.Services.AddScoped<ServicioAutenticacion>();
builder.Services.AddScoped<ServicioPacientes>();
builder.Services.AddScoped<ServicioConsultas>();
builder.Services.AddScoped<ServicioCitas>();
builder.Services.AddScoped<ServicioArchivos>();

// HostedService que crea los índices UNA sola vez al arrancar (antes lo hacía en cada request)
builder.Services.AddHostedService<InicializadorIndicesMongo>();

// ── Compresión de respuestas (reduce el tamaño JSON ~70%) ──────────────────────
builder.Services.AddResponseCompression(opciones =>
{
 opciones.EnableForHttps = true;
 opciones.Providers.Add<BrotliCompressionProvider>();
 opciones.Providers.Add<GzipCompressionProvider>();
 opciones.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

// ── Autenticación JWT ──────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
     opciones.TokenValidationParameters = new TokenValidationParameters
     {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = configuracionJwt.Issuer,
      ValidAudience = configuracionJwt.Audience,
      IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(configuracionJwt.Secret))
     };
    });

builder.Services.AddAuthorization();

// ── CORS — orígenes leídos desde configuración (env var en producción) ─────────
var origenesPermitidos = (builder.Configuration["Cors:AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opciones =>
    opciones.AddPolicy("Frontend", politica =>
        politica
            .WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

// ── Controladores + Swagger ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
 opciones.SwaggerDoc("v1", new OpenApiInfo
 {
  Title = "NutriGestión API",
  Version = "v1",
  Description = "API REST para el Sistema de Gestión del Consultorio Nutricional"
 });

 opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
 {
  Name = "Authorization",
  Type = SecuritySchemeType.ApiKey,
  Scheme = "Bearer",
  In = ParameterLocation.Header,
  Description = "Ingresa: Bearer {tu_token}"
 });

 opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id   = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

// ── Construcción de la app ─────────────────────────────────────────────────────
var app = builder.Build();

// Middleware de manejo global de excepciones (debe ir primero)
app.UseMiddleware<ManejoDeExcepcionesMiddleware>();

app.UseResponseCompression();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
 c.SwaggerEndpoint("/swagger/v1/swagger.json", "NutriGestión API v1");
 c.RoutePrefix = "swagger";
});

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint público para keep-alive (UptimeRobot, Cron-job.org) y health checks.
// Devuelve 200 sin tocar la base de datos — no agrega carga a Atlas.
app.MapGet("/health", () => Results.Ok(new { estado = "ok", hora = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();
