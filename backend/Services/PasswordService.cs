//mecanismo de hashing de ASP.NET Core basado en PasswordHasher<T>
//contraseña original - hasher - contraseña hasheada - sql
// iniciar sesion - hasher.verify - coincide? si/no


using Microsoft.AspNetCore.Identity;

namespace backend.Services;

public class PasswordService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

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