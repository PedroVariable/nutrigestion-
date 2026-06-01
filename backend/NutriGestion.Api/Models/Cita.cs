using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NutriGestion.Api.Models;

/// <summary>
/// Representa una cita programada en el consultorio
/// </summary>
public class Cita
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 [BsonElement("pacienteId")]
 public string PacienteId { get; set; } = string.Empty;

 // Desnormalizado para queries rápidas sin hacer join
 [BsonElement("nombrePaciente")]
 public string NombrePaciente { get; set; } = string.Empty;

 [BsonElement("fechaHora")]
 public DateTime FechaHora { get; set; }

 [BsonElement("duracionMinutos")]
 public int DuracionMinutos { get; set; } = 45;

 [BsonElement("estado")]
 public string Estado { get; set; } = "programada"; // programada | completada | cancelada

 [BsonElement("notas")]
 public string? Notas { get; set; }

 [BsonElement("creadoEn")]
 public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}