//Tabla de datos biometricos
namespace backend.Models;

public class DatosBiometricos
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string RostroRecortado { get; set; } = string.Empty;
}