//Tabla de bitacoras

namespace backend.Models;

public class BitacoraLogin
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Ip { get; set; } = string.Empty;
}