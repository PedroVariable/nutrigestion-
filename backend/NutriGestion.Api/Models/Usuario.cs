using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NutriGestion.Api.Models;

/// <summary>
/// Usuario del sistema (nutriólogo/administrador)
/// </summary>
public class Usuario
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonElement("nombre")]
 public string Nombre { get; set; } = string.Empty;

 [BsonElement("correo")]
 public string Correo { get; set; } = string.Empty;

 [BsonElement("hashContrasena")]
 public string HashContrasena { get; set; } = string.Empty;

 [BsonElement("creadoEn")]
 public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

// ── DTOs de autenticación ──────────────────────────────────────────────────────

/// <summary>Datos para iniciar sesión</summary>
public record SolicitudLogin(string Correo, string Contrasena);

/// <summary>Datos para registrar una nueva cuenta</summary>
public record SolicitudRegistro(string Nombre, string Correo, string Contrasena);

/// <summary>Respuesta al iniciar sesión exitosamente</summary>
public record RespuestaLogin(string Token, string Nombre, string Correo);