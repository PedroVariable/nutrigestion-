using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NutriGestion.Api.Configuration;
using NutriGestion.Api.Models;
using NutriGestion.Api.Repositories;

namespace NutriGestion.Api.Services;

/// <summary>
/// Maneja registro, inicio de sesión y generación de tokens JWT
/// </summary>
public class ServicioAutenticacion
{
 private readonly RepositorioMongo<Usuario> _repositorio;
 private readonly JwtSettings _jwt;

 public ServicioAutenticacion(
     IMongoDatabase baseDatos,
     IOptions<JwtSettings> configuracionJwt)
 {
  _repositorio = new RepositorioMongo<Usuario>(baseDatos, "usuarios");
  _jwt = configuracionJwt.Value;
 }

 /// <summary>
 /// Valida las credenciales del usuario y devuelve un token JWT si son correctas.
 /// Devuelve null si el correo no existe o la contraseña es incorrecta.
 /// </summary>
 public async Task<RespuestaLogin?> IniciarSesionAsync(SolicitudLogin solicitud)
 {
  var usuario = await _repositorio.Coleccion
      .Find(u => u.Correo == solicitud.Correo)
      .FirstOrDefaultAsync();

  if (usuario is null) return null;
  if (!BCrypt.Net.BCrypt.Verify(solicitud.Contrasena, usuario.HashContrasena)) return null;

  return new RespuestaLogin(GenerarToken(usuario), usuario.Nombre, usuario.Correo);
 }

 /// <summary>
 /// Registra un nuevo usuario en el sistema.
 /// Devuelve null si ya existe una cuenta con ese correo.
 /// </summary>
 public async Task<RespuestaLogin?> RegistrarAsync(SolicitudRegistro solicitud)
 {
  var existe = await _repositorio.Coleccion
      .Find(u => u.Correo == solicitud.Correo)
      .AnyAsync();

  if (existe) return null;

  var usuario = new Usuario
  {
   Nombre = solicitud.Nombre,
   Correo = solicitud.Correo,
   HashContrasena = BCrypt.Net.BCrypt.HashPassword(solicitud.Contrasena),
   CreadoEn = DateTime.UtcNow
  };

  await _repositorio.CrearAsync(usuario);
  return new RespuestaLogin(GenerarToken(usuario), usuario.Nombre, usuario.Correo);
 }

 /// <summary>Genera un token JWT firmado con los datos del usuario</summary>
 private string GenerarToken(Usuario usuario)
 {
  var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
  var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

  var claims = new[]
  {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id!),
            new Claim(ClaimTypes.Email,          usuario.Correo),
            new Claim(ClaimTypes.Name,           usuario.Nombre)
        };

  var token = new JwtSecurityToken(
      issuer: _jwt.Issuer,
      audience: _jwt.Audience,
      claims: claims,
      expires: DateTime.UtcNow.AddHours(_jwt.ExpiresInHours),
      signingCredentials: credenciales
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
 }
}
