namespace backend.Dtos;

public class NotificacionDto
{
    public int UsuarioId { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public byte[]? Archivo { get; set; }
    public string? NombreArchivo { get; set; }
}