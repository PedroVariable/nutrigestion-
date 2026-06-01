using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NutriGestion.Api.Models;

/// <summary>
/// Documento o imagen asociada a un paciente (dietas, recetas, fotos de seguimiento)
/// </summary>
public class ArchivoPaciente
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 [BsonElement("pacienteId")]
 public string PacienteId { get; set; } = string.Empty;

 [BsonElement("nombreArchivo")]
 public string NombreArchivo { get; set; } = string.Empty;     // nombre generado (UUID + extensión)

 [BsonElement("nombreOriginal")]
 public string NombreOriginal { get; set; } = string.Empty;    // nombre que subió el usuario

 [BsonElement("tipoContenido")]
 public string TipoContenido { get; set; } = string.Empty;     // MIME type

 [BsonElement("tipoArchivo")]
 public string TipoArchivo { get; set; } = "documento";        // documento | imagen | dieta | receta

 [BsonElement("datos")]
 public byte[] Datos { get; set; } = Array.Empty<byte>();      // binario del archivo

 [BsonElement("tamanoBytes")]
 public long TamanoBytes { get; set; }

 [BsonElement("subidoEn")]
 public DateTime SubidoEn { get; set; } = DateTime.UtcNow;
}