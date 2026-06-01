using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGestion.Api.Models;
using NutriGestion.Api.Services;

namespace NutriGestion.Api.Controllers;

[ApiController]
[Route("api/citas")]
[Authorize]
public class CitasController : ControllerBase
{
 private readonly ServicioCitas _servicio;

 public CitasController(ServicioCitas servicio) => _servicio = servicio;

 /// <summary>GET api/citas?desde=&hasta= — Citas en un rango de fechas (default: próximos 30 días)</summary>
 [HttpGet]
 public async Task<IActionResult> ObtenerPorRango(
     [FromQuery] DateTime? desde,
     [FromQuery] DateTime? hasta)
 {
  var inicio = desde ?? DateTime.UtcNow.Date;
  var fin = hasta ?? inicio.AddDays(30);
  return Ok(await _servicio.ObtenerPorRangoAsync(inicio, fin));
 }

 /// <summary>GET api/citas/hoy — Citas programadas para el día de hoy</summary>
 [HttpGet("hoy")]
 public async Task<IActionResult> ObtenerHoy() =>
     Ok(await _servicio.ObtenerHoyAsync());

 /// <summary>GET api/citas/paciente/{pacienteId} — Todas las citas de un paciente</summary>
 [HttpGet("paciente/{pacienteId}")]
 public async Task<IActionResult> ObtenerPorPaciente(string pacienteId) =>
     Ok(await _servicio.ObtenerPorPacienteAsync(pacienteId));

 /// <summary>GET api/citas/{id} — Detalle de una cita</summary>
 [HttpGet("{id}")]
 public async Task<IActionResult> ObtenerPorId(string id)
 {
  var cita = await _servicio.ObtenerPorIdAsync(id);
  return cita is null ? NotFound(new { mensaje = "Cita no encontrada." }) : Ok(cita);
 }

 /// <summary>POST api/citas — Programa una nueva cita</summary>
 [HttpPost]
 public async Task<IActionResult> Crear([FromBody] Cita cita)
 {
  var creada = await _servicio.CrearAsync(cita);
  return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
 }

 /// <summary>PUT api/citas/{id} — Actualiza una cita (reagendar, cambiar estado, etc.)</summary>
 [HttpPut("{id}")]
 public async Task<IActionResult> Actualizar(string id, [FromBody] Cita cita)
 {
  if (await _servicio.ObtenerPorIdAsync(id) is null)
   return NotFound(new { mensaje = "Cita no encontrada." });

  cita.Id = id;
  await _servicio.ActualizarAsync(id, cita);
  return NoContent();
 }

 /// <summary>DELETE api/citas/{id} — Elimina una cita</summary>
 [HttpDelete("{id}")]
 public async Task<IActionResult> Eliminar(string id)
 {
  if (await _servicio.ObtenerPorIdAsync(id) is null)
   return NotFound(new { mensaje = "Cita no encontrada." });

  await _servicio.EliminarAsync(id);
  return NoContent();
 }
}