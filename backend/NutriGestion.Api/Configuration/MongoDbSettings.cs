namespace NutriGestion.Api.Configuration;

/// <summary>
/// Configuración de conexión a MongoDB — se mapea desde appsettings.json sección "MongoDb"
/// </summary>
public class MongoDbSettings
{
 public string ConnectionString { get; set; } = string.Empty;
 public string DatabaseName { get; set; } = string.Empty;
}