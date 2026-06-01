using MongoDB.Driver;
using NutriGestion.Api.Models;
using NutriGestion.Api.Repositories;

namespace NutriGestion.Api.Services;

/// <summary>
/// Lógica de negocio para el historial de consultas nutricionales
/// </summary>
public class ServicioConsultas
{
 private readonly RepositorioMongo<Consulta> _repositorio;
 private readonly ServicioPacientes _servicioPacientes;

 public ServicioConsultas(IMongoDatabase baseDatos, ServicioPacientes servicioPacientes)
 {
  _repositorio = new RepositorioMongo<Consulta>(baseDatos, "consultas");
  _servicioPacientes = servicioPacientes;
 }

 /// <summary>Obtiene todas las consultas de un paciente, de más reciente a más antigua</summary>
 public async Task<List<Consulta>> ObtenerPorPacienteAsync(string pacienteId) =>
     await _repositorio.Coleccion
         .Find(c => c.PacienteId == pacienteId)
         .SortByDescending(c => c.Fecha)
         .ToListAsync();

 /// <summary>Obtiene una consulta por su Id</summary>
 public async Task<Consulta?> ObtenerPorIdAsync(string id) =>
     await _repositorio.ObtenerPorIdAsync(id);

 /// <summary>
 /// Crea una nueva consulta.
 /// Calcula el IMC automáticamente si se proporcionaron peso y talla.
 /// Actualiza el snapshot de peso/talla en el perfil del paciente.
 /// </summary>
 public async Task<Consulta> CrearAsync(Consulta consulta)
 {
  // Calcular IMC: peso(kg) / talla(m)²
  if (consulta.Peso.HasValue && consulta.Talla is > 0)
  {
   double tallaEnMetros = consulta.Talla.Value / 100.0;
   consulta.Imc = Math.Round(consulta.Peso.Value / (tallaEnMetros * tallaEnMetros), 2);
  }

  consulta.CreadoEn = DateTime.UtcNow;
  var creada = await _repositorio.CrearAsync(consulta);

  // Actualizar snapshot en el paciente para mostrarlo en el listado
  var paciente = await _servicioPacientes.ObtenerPorIdAsync(consulta.PacienteId);
  if (paciente is not null)
  {
   paciente.UltimoPeso = consulta.Peso;
   paciente.UltimaTalla = consulta.Talla;
   await _servicioPacientes.ActualizarAsync(paciente.Id!, paciente);
  }

  return creada;
 }

 /// <summary>Actualiza una consulta existente (también recalcula IMC)</summary>
 public async Task ActualizarAsync(string id, Consulta consulta)
 {
  if (consulta.Peso.HasValue && consulta.Talla is > 0)
  {
   double tallaEnMetros = consulta.Talla.Value / 100.0;
   consulta.Imc = Math.Round(consulta.Peso.Value / (tallaEnMetros * tallaEnMetros), 2);
  }
  await _repositorio.ActualizarAsync(id, consulta);
 }

 /// <summary>Elimina una consulta</summary>
 public async Task EliminarAsync(string id) =>
     await _repositorio.EliminarAsync(id);
}
