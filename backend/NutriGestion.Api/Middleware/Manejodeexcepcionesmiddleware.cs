using System.Net;
using System.Text.Json;

namespace NutriGestion.Api.Middleware;

/// <summary>
/// Captura excepciones no manejadas y devuelve una respuesta JSON amigable.
/// Evita que se exponga el stack trace en producción.
/// </summary>
public class ManejoDeExcepcionesMiddleware
{
 private readonly RequestDelegate _siguiente;
 private readonly ILogger<ManejoDeExcepcionesMiddleware> _logger;

 public ManejoDeExcepcionesMiddleware(
     RequestDelegate siguiente,
     ILogger<ManejoDeExcepcionesMiddleware> logger)
 {
  _siguiente = siguiente;
  _logger = logger;
 }

 public async Task InvokeAsync(HttpContext contexto)
 {
  try
  {
   await _siguiente(contexto);
  }
  catch (Exception ex)
  {
   _logger.LogError(ex, "Error no controlado: {Mensaje}", ex.Message);
   await ManejarExcepcionAsync(contexto, ex);
  }
 }

 private static async Task ManejarExcepcionAsync(HttpContext contexto, Exception excepcion)
 {
  contexto.Response.ContentType = "application/json";
  contexto.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

  var respuesta = new
  {
   estado = 500,
   mensaje = "Ocurrió un error interno en el servidor. Intenta de nuevo más tarde."
  };

  var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
  await contexto.Response.WriteAsync(JsonSerializer.Serialize(respuesta, opciones));
 }
}