namespace NutriGestion.Api.Configuration;

/// <summary>
/// Configuración del token JWT — se mapea desde appsettings.json sección "Jwt"
/// </summary>
public class JwtSettings
{
 public string Secret { get; set; } = string.Empty;
 public string Issuer { get; set; } = string.Empty;
 public string Audience { get; set; } = string.Empty;
 public int ExpiresInHours { get; set; } = 8;
}