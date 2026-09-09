//DTO Para cambio de password desde Perfil

namespace backend.Dtos;

public class CambioPasswordDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
    public string ConfirmarNuevaPassword { get; set; } = string.Empty;
}