//DTO (Data Transfer Object) se encargará de enviar el objeto usuario para no hacerlo en el frontend
//frontend - dto - backen y validaciones - sql

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