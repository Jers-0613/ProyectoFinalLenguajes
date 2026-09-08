// DTO utilizado para recibir los datos necesarios durante el registro.
// Separa la información recibida desde el frontend del modelo Usuario
// que finalmente se almacena en la base de datos.

namespace backend.Dtos;

public class RegistroUsuarioDto
{
    public string Correo { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public string Nickname { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmarPassword { get; set; } = string.Empty;

    public int PreferenciaNotificacionId { get; set; }
}