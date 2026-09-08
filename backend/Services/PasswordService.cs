//mecanismo de hashing de ASP.NET Core basado en PasswordHasher<T>
//contraseña original - hasher - contraseña hasheada - sql
// iniciar sesion - hasher.verify - coincide? si/no


using Microsoft.AspNetCore.Identity;

namespace backend.Services;

// Servicio encargado de proteger las contraseñas de los usuarios.
// Utiliza PasswordHasher de ASP.NET Core para generar y verificar
// hashes sin almacenar nunca la contraseña original.
public class PasswordService
{

    // PasswordHasher se encarga de aplicar el algoritmo de hashing
    // y de administrar el formato y parámetros utilizados por ASP.NET Core.
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    // Comprueba si una contraseña coincide con el hash almacenado.
    // No es necesario descifrar el hash para realizar esta comprobación.
    public bool VerifyPassword(string passwordHash, string password)
    {
        var resultado = _passwordHasher.VerifyHashedPassword(
            null!,
            passwordHash,
            password
        );

        return resultado == PasswordVerificationResult.Success ||
               resultado == PasswordVerificationResult.SuccessRehashNeeded;
    }
}