using MongoDB.Driver;

namespace NutriGestion.Api.Repositories;

/// <summary>
/// Repositorio genérico para operaciones CRUD sobre cualquier colección de MongoDB.
/// Recibe IMongoDatabase inyectada (singleton) — NO crea su propio MongoClient.
/// </summary>
public class RepositorioMongo<T>
{
 protected readonly IMongoCollection<T> _coleccion;

 public RepositorioMongo(IMongoDatabase baseDatos, string nombreColeccion)
 {
  _coleccion = baseDatos.GetCollection<T>(nombreColeccion);
 }

 /// <summary>Expone la colección para queries personalizados en los servicios</summary>
 public IMongoCollection<T> Coleccion => _coleccion;

 /// <summary>Obtiene todos los documentos</summary>
 public async Task<List<T>> ObtenerTodosAsync() =>
     await _coleccion.Find(_ => true).ToListAsync();

 /// <summary>Obtiene un documento por su ObjectId</summary>
 public async Task<T?> ObtenerPorIdAsync(string id)
 {
  var filtro = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
  return await _coleccion.Find(filtro).FirstOrDefaultAsync();
 }

 /// <summary>Inserta un nuevo documento y lo devuelve con su Id asignado</summary>
 public async Task<T> CrearAsync(T entidad)
 {
  await _coleccion.InsertOneAsync(entidad);
  return entidad;
 }

 /// <summary>Reemplaza un documento completo por su Id</summary>
 public async Task ActualizarAsync(string id, T entidad)
 {
  var filtro = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
  await _coleccion.ReplaceOneAsync(filtro, entidad);
 }

 /// <summary>Elimina un documento por su Id</summary>
 public async Task EliminarAsync(string id)
 {
  var filtro = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
  await _coleccion.DeleteOneAsync(filtro);
 }
}
