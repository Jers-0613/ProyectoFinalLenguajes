namespace backend.Messages;

public static class MensajesCorreo
{
    public static string CredencialRegistro(string nickname)
    {
        return $"""
        Hola {nickname},

        Tu registro en el sistema Analizador Léxico se realizó correctamente.

        Adjuntamos tu credencial de identificación en formato PDF.

        Conserva este documento, ya que el código QR de la credencial
        será utilizado posteriormente para iniciar sesión.

        Saludos,
        Analizador Léxico
        """;
    }

    public static string RecuperacionPassword(
        string nickname,
        string passwordTemporal)
    {
        return $"""
        Hola {nickname},

        Hemos recibido tú solicitud para recuperar tu contraseña.

        Tu contraseña temporal es:

        {passwordTemporal}

        Por seguridad, deberás cambiar esta contraseña después de iniciar sesión.

        Saludos,
        Analizador Léxico
        """;
    }
}