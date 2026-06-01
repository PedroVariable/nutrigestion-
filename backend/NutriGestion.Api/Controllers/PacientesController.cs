using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGestion.Api.Models;
using NutriGestion.Api.Services;

namespace NutriGestion.Api.Controllers;

[ApiController]
[Route("api/pacientes")]
[Authorize]
public class PacientesController : ControllerBase
{
 private readonly ServicioPacientes _servicio;

 public PacientesController(ServicioPacientes servicio) => _servicio = servicio;

 /// <summary>GET api/pacientes — Lista todos los pacientes activos (o filtra por búsqueda)</summary>
 [HttpGet]
 public async Task<IActionResult> ObtenerTodos([FromQuery] string? busqueda) =>
     Ok(string.IsNullOrWhiteSpace(busqueda)
         ? await _servicio.ObtenerTodosAsync()
         : await _servicio.BuscarAsync(busqueda));

 /// <summary>GET api/pacientes/{id} — Obtiene un paciente por Id</summary>
 [HttpGet("{id}")]
 public async Task<IActionResult> ObtenerPorId(string id)
 {
  var paciente = await _servicio.ObtenerPorIdAsync(id);
  return paciente is null ? NotFound(new { mensaje = "Paciente no encontrado." }) : Ok(paciente);
 }

 /// <summary>POST api/pacientes — Registra un nuevo paciente</summary>
 [HttpPost]
 public async Task<IActionResult> Crear([FromBody] Paciente paciente)
 {
  var creado = await _servicio.CrearAsync(paciente);
  return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
 }

 /// <summary>PUT api/pacientes/{id} — Actualiza los datos de un paciente</summary>
 [HttpPut("{id}")]
 public async Task<IActionResult> Actualizar(string id, [FromBody] Paciente paciente)
 {
  if (await _servicio.ObtenerPorIdAsync(id) is null)
   return NotFound(new { mensaje = "Paciente no encontrado." });

  paciente.Id = id;
  await _servicio.ActualizarAsync(id, paciente);
  return NoContent();
 }

 /// <summary>DELETE api/pacientes/{id} — Desactiva un paciente (eliminación lógica)</summary>
 [HttpDelete("{id}")]
 public async Task<IActionResult> Eliminar(string id)
 {
  if (await _servicio.ObtenerPorIdAsync(id) is null)
   return NotFound(new { mensaje = "Paciente no encontrado." });

  await _servicio.EliminarLogicoAsync(id);
  return NoContent();
 }
}