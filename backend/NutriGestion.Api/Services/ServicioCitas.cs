using MongoDB.Driver;
using NutriGestion.Api.Models;
using NutriGestion.Api.Repositories;

namespace NutriGestion.Api.Services;

/// <summary>
/// Lógica de negocio para la agenda de citas del consultorio
/// </summary>
public class ServicioCitas
{
 private readonly RepositorioMongo<Cita> _repositorio;

 public ServicioCitas(IMongoDatabase baseDatos)
 {
  _repositorio = new RepositorioMongo<Cita>(baseDatos, "citas");
 }

 /// <summary>Obtiene todas las citas dentro de un rango de fechas</summary>
 public async Task<List<Cita>> ObtenerPorRangoAsync(DateTime desde, DateTime hasta) =>
     await _repositorio.Coleccion
         .Find(c => c.FechaHora >= desde && c.FechaHora <= hasta)
         .SortBy(c => c.FechaHora)
         .ToListAsync();

 /// <summary>Obtiene las citas del día de hoy</summary>
 public async Task<List<Cita>> ObtenerHoyAsync()
 {
  var inicio = DateTime.UtcNow.Date;
  return await ObtenerPorRangoAsync(inicio, inicio.AddDays(1));
 }

 /// <summary>Obtiene todas las citas de un paciente específico</summary>
 public async Task<List<Cita>> ObtenerPorPacienteAsync(string pacienteId) =>
     await _repositorio.Coleccion
         .Find(c => c.PacienteId == pacienteId)
         .SortByDescending(c => c.FechaHora)
         .ToListAsync();

 /// <summary>Obtiene una cita por su Id</summary>
 public async Task<Cita?> ObtenerPorIdAsync(string id) =>
     await _repositorio.ObtenerPorIdAsync(id);

 /// <summary>Crea una nueva cita</summary>
 public async Task<Cita> CrearAsync(Cita cita)
 {
  cita.CreadoEn = DateTime.UtcNow;
  return await _repositorio.CrearAsync(cita);
 }

 /// <summary>Actualiza una cita existente (estado, notas, reagendar)</summary>
 public async Task ActualizarAsync(string id, Cita cita) =>
     await _repositorio.ActualizarAsync(id, cita);

 /// <summary>Elimina una cita</summary>
 public async Task EliminarAsync(string id) =>
     await _repositorio.EliminarAsync(id);

 /// <summary>Cuenta citas programadas que aún no han pasado</summary>
 public async Task<long> ContarProximasAsync() =>
     await _repositorio.Coleccion.CountDocumentsAsync(
         c => c.FechaHora >= DateTime.UtcNow && c.Estado == "programada");
}
