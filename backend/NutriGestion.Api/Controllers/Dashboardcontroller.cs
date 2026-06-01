using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGestion.Api.Services;

namespace NutriGestion.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
 private readonly ServicioPacientes _servicioPacientes;
 private readonly ServicioCitas _servicioCitas;

 public DashboardController(ServicioPacientes servicioPacientes, ServicioCitas servicioCitas)
 {
  _servicioPacientes = servicioPacientes;
  _servicioCitas = servicioCitas;
 }

 /// <summary>GET api/dashboard/resumen � Estad�sticas generales para la pantalla principal</summary>
 [HttpGet("resumen")]
 public async Task<IActionResult> ObtenerResumen()
 {
  // Las 3 queries son independientes — se lanzan en paralelo en lugar de en serie.
  // Antes: suma de las 3 latencias. Ahora: la del más lento.
  var tareaTotalPacientes = _servicioPacientes.ContarActivosAsync();
  var tareaCitasProximas  = _servicioCitas.ContarProximasAsync();
  var tareaCitasHoy       = _servicioCitas.ObtenerHoyAsync();

  await Task.WhenAll(tareaTotalPacientes, tareaCitasProximas, tareaCitasHoy);

  return Ok(new
  {
   TotalPacientes = tareaTotalPacientes.Result,
   CitasProximas  = tareaCitasProximas.Result,
   CitasHoy       = tareaCitasHoy.Result
  });
 }
}