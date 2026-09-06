// tabla usuarios

namespace backend.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Correo { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public string Nickname { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? FotoOriginal { get; set; }

    public string? FotoModificada { get; set; }

    public int PreferenciaNotificacionId { get; set; }

    public int RolId { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }
}