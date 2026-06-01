using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGestion.Api.Models;
using NutriGestion.Api.Services;

namespace NutriGestion.Api.Controllers;

[ApiController]
[Route("api/pacientes/{pacienteId}/consultas")]
[Authorize]
public class ConsultasController : ControllerBase
{
 private readonly ServicioConsultas _servicio;

 public ConsultasController(ServicioConsultas servicio) => _servicio = servicio;

 /// <summary>GET api/pacientes/{pacienteId}/consultas — Historial de consultas del paciente</summary>
 [HttpGet]
 public async Task<IActionResult> ObtenerPorPaciente(string pacienteId) =>
     Ok(await _servicio.ObtenerPorPacienteAsync(pacienteId));

 /// <summary>GET api/pacientes/{pacienteId}/consultas/{id} — Detalle de una consulta</summary>
 [HttpGet("{id}")]
 public async Task<IActionResult> ObtenerPorId(string pacienteId, string id)
 {
  var consulta = await _servicio.ObtenerPorIdAsync(id);
  if (consulta is null || consulta.PacienteId != pacienteId)
   return NotFound(new { mensaje = "Consulta no encontrada." });

  return Ok(consulta);
 }

 /// <summary>POST api/pacientes/{pacienteId}/consultas — Registra una nueva consulta (IMC se calcula automáticamente)</summary>
 [HttpPost]
 public async Task<IActionResult> Crear(string pacienteId, [FromBody] Consulta consulta)
 {
  consulta.PacienteId = pacienteId;
  var creada = await _servicio.CrearAsync(consulta);
  return CreatedAtAction(nameof(ObtenerPorId), new { pacienteId, id = creada.Id }, creada);
 }

 /// <summary>PUT api/pacientes/{pacienteId}/consultas/{id} — Actualiza una consulta</summary>
 [HttpPut("{id}")]
 public async Task<IActionResult> Actualizar(string pacienteId, string id, [FromBody] Consulta consulta)
 {
  var existente = await _servicio.ObtenerPorIdAsync(id);
  if (existente is null || existente.PacienteId != pacienteId)
   return NotFound(new { mensaje = "Consulta no encontrada." });

  consulta.Id = id;
  consulta.PacienteId = pacienteId;
  await _servicio.ActualizarAsync(id, consulta);
  return NoContent();
 }

 /// <summary>DELETE api/pacientes/{pacienteId}/consultas/{id} — Elimina una consulta</summary>
 [HttpDelete("{id}")]
 public async Task<IActionResult> Eliminar(string pacienteId, string id)
 {
  var existente = await _servicio.ObtenerPorIdAsync(id);
  if (existente is null || existente.PacienteId != pacienteId)
   return NotFound(new { mensaje = "Consulta no encontrada." });

  await _servicio.EliminarAsync(id);
  return NoContent();
 }
}