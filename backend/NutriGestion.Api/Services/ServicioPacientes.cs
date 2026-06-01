using MongoDB.Driver;
using NutriGestion.Api.Models;
using NutriGestion.Api.Repositories;

namespace NutriGestion.Api.Services;

/// <summary>
/// Lógica de negocio para la gestión de pacientes
/// </summary>
public class ServicioPacientes
{
 private readonly RepositorioMongo<Paciente> _repositorio;

 public ServicioPacientes(IMongoDatabase baseDatos)
 {
  _repositorio = new RepositorioMongo<Paciente>(baseDatos, "pacientes");
 }

 /// <summary>Obtiene todos los pacientes activos ordenados por apellido</summary>
 public async Task<List<Paciente>> ObtenerTodosAsync() =>
     await _repositorio.Coleccion
         .Find(p => p.Activo)
         .SortBy(p => p.Apellido)
         .ToListAsync();

 /// <summary>Obtiene un paciente por su Id</summary>
 public async Task<Paciente?> ObtenerPorIdAsync(string id) =>
     await _repositorio.ObtenerPorIdAsync(id);

 /// <summary>Búsqueda por nombre, apellido o teléfono (insensible a mayúsculas, anclada al inicio para usar índice)</summary>
 public async Task<List<Paciente>> BuscarAsync(string termino)
 {
  // Escapamos caracteres especiales de regex y anclamos con ^ para que MongoDB pueda usar el índice
  var terminoEscapado = System.Text.RegularExpressions.Regex.Escape(termino);
  var regex = new MongoDB.Bson.BsonRegularExpression($"^{terminoEscapado}", "i");
  var filtro = Builders<Paciente>.Filter.And(
      Builders<Paciente>.Filter.Eq(p => p.Activo, true),
      Builders<Paciente>.Filter.Or(
          Builders<Paciente>.Filter.Regex(p => p.Nombre, regex),
          Builders<Paciente>.Filter.Regex(p => p.Apellido, regex),
          Builders<Paciente>.Filter.Regex(p => p.Telefono, regex)
      )
  );
  return await _repositorio.Coleccion.Find(filtro).ToListAsync();
 }

 /// <summary>Crea un nuevo paciente</summary>
 public async Task<Paciente> CrearAsync(Paciente paciente)
 {
  paciente.CreadoEn = paciente.ActualizadoEn = DateTime.UtcNow;
  return await _repositorio.CrearAsync(paciente);
 }

 /// <summary>Actualiza los datos de un paciente existente</summary>
 public async Task ActualizarAsync(string id, Paciente paciente)
 {
  paciente.ActualizadoEn = DateTime.UtcNow;
  await _repositorio.ActualizarAsync(id, paciente);
 }

 /// <summary>
 /// Elimina de forma lógica — solo marca Activo = false,
 /// no borra el registro para conservar el historial
 /// </summary>
 public async Task EliminarLogicoAsync(string id)
 {
  var paciente = await _repositorio.ObtenerPorIdAsync(id);
  if (paciente is null) return;
  paciente.Activo = false;
  paciente.ActualizadoEn = DateTime.UtcNow;
  await _repositorio.ActualizarAsync(id, paciente);
 }

 /// <summary>Cuenta total de pacientes activos</summary>
 public async Task<long> ContarActivosAsync() =>
     await _repositorio.Coleccion.CountDocumentsAsync(p => p.Activo);
}
