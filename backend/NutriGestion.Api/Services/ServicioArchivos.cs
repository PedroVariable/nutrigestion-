using MongoDB.Driver;
using NutriGestion.Api.Models;
using NutriGestion.Api.Repositories;

namespace NutriGestion.Api.Services;

/// <summary>
/// Lógica de negocio para subir y descargar archivos de pacientes
/// </summary>
public class ServicioArchivos
{
 private readonly RepositorioMongo<ArchivoPaciente> _repositorio;
 private const long TamanoMaximoBytes = 10 * 1024 * 1024; // 10 MB

 private static readonly string[] TiposPermitidos =
     { "application/pdf", "image/jpeg", "image/png", "image/webp" };

 public ServicioArchivos(IMongoDatabase baseDatos)
 {
  _repositorio = new RepositorioMongo<ArchivoPaciente>(baseDatos, "archivos");
 }

 /// <summary>
 /// Lista los archivos de un paciente sin incluir el binario
 /// (más eficiente para mostrar el listado)
 /// </summary>
 public async Task<List<ArchivoPaciente>> ObtenerPorPacienteAsync(string pacienteId)
 {
  var proyeccion = Builders<ArchivoPaciente>.Projection.Exclude(a => a.Datos);
  return await _repositorio.Coleccion
      .Find(a => a.PacienteId == pacienteId)
      .Project<ArchivoPaciente>(proyeccion)
      .SortByDescending(a => a.SubidoEn)
      .ToListAsync();
 }

 /// <summary>Obtiene un archivo completo (con su binario) para descarga</summary>
 public async Task<ArchivoPaciente?> ObtenerPorIdAsync(string id) =>
     await _repositorio.ObtenerPorIdAsync(id);

 /// <summary>
 /// Sube un archivo y lo almacena en MongoDB.
 /// Valida tamaño máximo (10 MB) y tipos de archivo permitidos.
 /// </summary>
 public async Task<(bool Exitoso, string Error, ArchivoPaciente? Archivo)> SubirAsync(
     string pacienteId, IFormFile archivo, string tipoArchivo)
 {
  if (archivo.Length > TamanoMaximoBytes)
   return (false, "El archivo supera el límite de 10 MB.", null);

  if (!TiposPermitidos.Contains(archivo.ContentType))
   return (false, "Solo se permiten archivos PDF, JPG, PNG o WEBP.", null);

  using var flujo = new MemoryStream();
  await archivo.CopyToAsync(flujo);

  var archivoPaciente = new ArchivoPaciente
  {
   PacienteId = pacienteId,
   NombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}",
   NombreOriginal = archivo.FileName,
   TipoContenido = archivo.ContentType,
   TipoArchivo = tipoArchivo,
   Datos = flujo.ToArray(),
   TamanoBytes = archivo.Length,
   SubidoEn = DateTime.UtcNow
  };

  await _repositorio.CrearAsync(archivoPaciente);
  return (true, string.Empty, archivoPaciente);
 }

 /// <summary>Elimina un archivo de un paciente</summary>
 public async Task EliminarAsync(string id) =>
     await _repositorio.EliminarAsync(id);
}
