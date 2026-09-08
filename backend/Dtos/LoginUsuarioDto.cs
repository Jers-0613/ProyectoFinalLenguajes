// DTO utilizado para recibir únicamente los datos necesarios
// para iniciar sesión. Evita enviar o recibir el modelo Usuario completo.

namespace backend.Dtos;

public class LoginUsuarioDto
{
    public string Correo { get; set; } = string.Empty;

    // Contraseña proporcionada durante el inicio de sesión.
    // El backend la verifica contra el hash almacenado.
    public string Password { get; set; } = string.Empty;
}