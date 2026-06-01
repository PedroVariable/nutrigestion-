using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NutriGestion.Api.Models;

/// <summary>
/// Representa una consulta nutricional de un paciente
/// </summary>
public class Consulta
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 [BsonElement("pacienteId")]
 public string PacienteId { get; set; } = string.Empty;

 [BsonElement("fecha")]
 public DateTime Fecha { get; set; }

 [BsonElement("peso")]
 public double? Peso { get; set; }       // kg

 [BsonElement("talla")]
 public double? Talla { get; set; }      // cm

 [BsonElement("imc")]
 public double? Imc { get; set; }        // calculado automáticamente

 [BsonElement("observaciones")]
 public string? Observaciones { get; set; }

 [BsonElement("recomendaciones")]
 public string? Recomendaciones { get; set; }

 [BsonElement("proximaCita")]
 public DateTime? ProximaCita { get; set; }

 [BsonElement("creadoEn")]
 public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}