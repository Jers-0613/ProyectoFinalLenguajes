//Recibirá el correo para recuperar la contraseña 
namespace backend.Dtos;

public class RecuperarPasswordDto
{
    public string Correo { get; set; } = string.Empty;
}