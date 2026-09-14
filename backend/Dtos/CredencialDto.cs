
namespace backend.Dtos;

public class CredencialDto
{
    public int Id { get; set; }

    public Guid CodigoCredencial { get; set; }

    public string Nickname { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public string? Foto { get; set; }
}

