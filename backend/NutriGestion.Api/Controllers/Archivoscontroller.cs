using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGestion.Api.Services;

namespace NutriGestion.Api.Controllers;

[ApiController]
[Route("api/pacientes/{pacienteId}/archivos")]
[Authorize]
public class ArchivosController : ControllerBase
{
 private readonly ServicioArchivos _servicio;

 public ArchivosController(ServicioArchivos servicio) => _servicio = servicio;

 /// <summary>GET api/pacientes/{pacienteId}/archivos — Lista los archivos del paciente (sin binario)</summary>
 [HttpGet]
 public async Task<IActionResult> ObtenerPorPaciente(string pacienteId) =>
     Ok(await _servicio.ObtenerPorPacienteAsync(pacienteId));

 /// <summary>POST api/pacientes/{pacienteId}/archivos — Sube un archivo (PDF, JPG, PNG, WEBP — máx. 10 MB)</summary>
 [HttpPost]
 [RequestSizeLimit(10_485_760)]
 public async Task<IActionResult> Subir(
     string pacienteId,
     IFormFile archivo,
     [FromForm] string tipoArchivo = "documento")
 {
  var (exitoso, error, archivoPaciente) = await _servicio.SubirAsync(pacienteId, archivo, tipoArchivo);

  if (!exitoso)
   return BadRequest(new { mensaje = error });

  return Ok(new
  {
   archivoPaciente!.Id,
   archivoPaciente.NombreOriginal,
   archivoPaciente.TipoArchivo,
   archivoPaciente.TipoContenido,
   archivoPaciente.TamanoBytes,
   archivoPaciente.SubidoEn
  });
 }

 /// <summary>GET api/pacientes/{pacienteId}/archivos/{id}/descargar — Descarga el archivo</summary>
 [HttpGet("{id}/descargar")]
 public async Task<IActionResult> Descargar(string pacienteId, string id)
 {
  var archivo = await _servicio.ObtenerPorIdAsync(id);
  if (archivo is null || archivo.PacienteId != pacienteId)
   return NotFound(new { mensaje = "Archivo no encontrado." });

  return File(archivo.Datos, archivo.TipoContenido, archivo.NombreOriginal);
 }

 /// <summary>DELETE api/pacientes/{pacienteId}/archivos/{id} — Elimina un archivo</summary>
 [HttpDelete("{id}")]
 public async Task<IActionResult> Eliminar(string pacienteId, string id)
 {
  var archivo = await _servicio.ObtenerPorIdAsync(id);
  if (archivo is null || archivo.PacienteId != pacienteId)
   return NotFound(new { mensaje = "Archivo no encontrado." });

  await _servicio.EliminarAsync(id);
  return NoContent();
 }
}