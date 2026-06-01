using Microsoft.AspNetCore.Mvc;
using NutriGestion.Api.Models;
using NutriGestion.Api.Services;

namespace NutriGestion.Api.Controllers;

[ApiController]
[Route("api/autenticacion")]
public class AutenticacionController : ControllerBase
{
 private readonly ServicioAutenticacion _servicio;

 public AutenticacionController(ServicioAutenticacion servicio) => _servicio = servicio;

 /// <summary>POST api/autenticacion/login — Inicia sesión y devuelve token JWT</summary>
 [HttpPost("login")]
 public async Task<IActionResult> Login([FromBody] SolicitudLogin solicitud)
 {
  var resultado = await _servicio.IniciarSesionAsync(solicitud);
  if (resultado is null)
   return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

  return Ok(resultado);
 }

 /// <summary>POST api/autenticacion/registro — Registra una nueva cuenta</summary>
 [HttpPost("registro")]
 public async Task<IActionResult> Registro([FromBody] SolicitudRegistro solicitud)
 {
  var resultado = await _servicio.RegistrarAsync(solicitud);
  if (resultado is null)
   return Conflict(new { mensaje = "Ya existe una cuenta con ese correo." });

  return Created(string.Empty, resultado);
 }
}