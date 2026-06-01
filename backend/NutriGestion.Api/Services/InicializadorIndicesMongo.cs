using MongoDB.Driver;
using NutriGestion.Api.Models;

namespace NutriGestion.Api.Services;

/// <summary>
/// HostedService que crea los índices de MongoDB UNA sola vez al arrancar la app.
/// Antes esto se ejecutaba en cada request (constructor de cada servicio Scoped),
/// generando un round-trip extra a Atlas por petición.
/// </summary>
public class InicializadorIndicesMongo : IHostedService
{
 private readonly IMongoDatabase _baseDatos;
 private readonly ILogger<InicializadorIndicesMongo> _log;

 public InicializadorIndicesMongo(IMongoDatabase baseDatos, ILogger<InicializadorIndicesMongo> log)
 {
  _baseDatos = baseDatos;
  _log = log;
 }

 public async Task StartAsync(CancellationToken cancellationToken)
 {
  try
  {
   var pacientes = _baseDatos.GetCollection<Paciente>("pacientes");
   await pacientes.Indexes.CreateOneAsync(
       new CreateIndexModel<Paciente>(
           Builders<Paciente>.IndexKeys
               .Ascending(p => p.Apellido)
               .Ascending(p => p.Nombre)),
       cancellationToken: cancellationToken);

   var citas = _baseDatos.GetCollection<Cita>("citas");
   await citas.Indexes.CreateOneAsync(
       new CreateIndexModel<Cita>(
           Builders<Cita>.IndexKeys.Ascending(c => c.FechaHora)),
       cancellationToken: cancellationToken);

   var consultas = _baseDatos.GetCollection<Consulta>("consultas");
   await consultas.Indexes.CreateOneAsync(
       new CreateIndexModel<Consulta>(
           Builders<Consulta>.IndexKeys
               .Ascending(c => c.PacienteId)
               .Descending(c => c.Fecha)),
       cancellationToken: cancellationToken);

   var archivos = _baseDatos.GetCollection<ArchivoPaciente>("archivos");
   await archivos.Indexes.CreateOneAsync(
       new CreateIndexModel<ArchivoPaciente>(
           Builders<ArchivoPaciente>.IndexKeys.Ascending(a => a.PacienteId)),
       cancellationToken: cancellationToken);

   var usuarios = _baseDatos.GetCollection<Usuario>("usuarios");
   await usuarios.Indexes.CreateOneAsync(
       new CreateIndexModel<Usuario>(
           Builders<Usuario>.IndexKeys.Ascending(u => u.Correo),
           new CreateIndexOptions { Unique = true }),
       cancellationToken: cancellationToken);

   _log.LogInformation("Índices de MongoDB verificados/creados correctamente.");
  }
  catch (Exception ex)
  {
   _log.LogError(ex, "Error creando índices de MongoDB. La app continuará pero las queries pueden ser más lentas.");
  }
 }

 public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
