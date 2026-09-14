
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace backend.Services;

public class NotificacionService
{
    private readonly IConfiguration configuration;

    public NotificacionService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task EnviarCorreoAsync(
        string destinatario,
        string asunto,
        string mensaje)
    {
        var servidor =
            configuration["Correo:Servidor"];

        var puerto =
            int.Parse(configuration["Correo:Puerto"]!);

        var usuario =
            configuration["Correo:Usuario"];

        var password =
            Environment.GetEnvironmentVariable(
                "CORREO_PASSWORD"
            );

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new Exception(
                "No se encontró la variable de entorno CORREO_PASSWORD."
            );
        }

        var correo = new MimeMessage();

        correo.From.Add(
            new MailboxAddress(
                "Analizador Léxico",
                usuario
            )
        );

        correo.To.Add(
            MailboxAddress.Parse(destinatario)
        );

        correo.Subject = asunto;

        correo.Body = new TextPart("plain")
        {
            Text = mensaje
        };

        using var cliente = new SmtpClient();

        await cliente.ConnectAsync(
            servidor,
            puerto,
            SecureSocketOptions.StartTls
        );

        await cliente.AuthenticateAsync(
            usuario,
            password
        );

        await cliente.SendAsync(correo);

        await cliente.DisconnectAsync(true);
    }

    
    public async Task EnviarCorreoConAdjuntoAsync(
        string destinatario,
        string asunto,
        string mensaje,
        byte[] archivo,
        string nombreArchivo)
        {
        var servidor =
            configuration["Correo:Servidor"];

        var puerto =
            int.Parse(configuration["Correo:Puerto"]!);

        var usuario =
            configuration["Correo:Usuario"];

        var password =
            Environment.GetEnvironmentVariable(
                "CORREO_PASSWORD"
            );

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new Exception(
                "No se encontró la variable de entorno CORREO_PASSWORD."
            );
        }

        var correo = new MimeMessage();

        correo.From.Add(
            new MailboxAddress(
                "Analizador Léxico",
                usuario
            )
        );

        correo.To.Add(
            MailboxAddress.Parse(destinatario)
        );

        correo.Subject = asunto;

        var cuerpo = new BodyBuilder
        {
            TextBody = mensaje
        };

        cuerpo.Attachments.Add(
            nombreArchivo,
            archivo,
            ContentType.Parse("application/pdf")
        );

        correo.Body = cuerpo.ToMessageBody();

        using var cliente = new SmtpClient();

        await cliente.ConnectAsync(
            servidor,
            puerto,
            SecureSocketOptions.StartTls
        );

        await cliente.AuthenticateAsync(
            usuario,
            password
        );

        await cliente.SendAsync(correo);

        await cliente.DisconnectAsync(true);
    }




}

