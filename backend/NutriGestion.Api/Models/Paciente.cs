using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NutriGestion.Api.Models;

/// <summary>
/// Representa un paciente del consultorio nutricional
/// </summary>
public class Paciente
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonElement("nombre")]
 public string Nombre { get; set; } = string.Empty;

 [BsonElement("apellido")]
 public string Apellido { get; set; } = string.Empty;

 [BsonElement("correo")]
 public string? Correo { get; set; }

 [BsonElement("telefono")]
 public string? Telefono { get; set; }

 [BsonElement("fechaNacimiento")]
 public DateTime? FechaNacimiento { get; set; }

 [BsonElement("genero")]
 public string? Genero { get; set; } // F | M | Otro

 [BsonElement("direccion")]
 public string? Direccion { get; set; }

 [BsonElement("notas")]
 public string? Notas { get; set; }

 [BsonElement("activo")]
 public bool Activo { get; set; } = true;

 [BsonElement("creadoEn")]
 public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

 [BsonElement("actualizadoEn")]
 public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

 // Snapshot del último registro — se actualiza al crear consulta
 [BsonElement("ultimoPeso")]
 public double? UltimoPeso { get; set; }

 [BsonElement("ultimaTalla")]
 public double? UltimaTalla { get; set; }
}