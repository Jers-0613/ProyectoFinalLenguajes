
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

using backend.Data;
using backend.Dtos;

using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class NotificacionService
{
    private readonly IConfiguration configuration;
    private readonly ApplicationDbContext db;

    public NotificacionService(IConfiguration configuration,ApplicationDbContext db)
    {
        this.configuration = configuration;
        this.db = db;
    }

    public async Task EnviarNotificacionAsync(
        NotificacionDto notificacion)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u =>
                u.Id == notificacion.UsuarioId);

        if (usuario == null)
        {
            throw new Exception(
                "No se encontró el usuario para enviar la notificación."
            );
        }

        var preferencia = await db.PreferenciasNotificacion
            .FirstOrDefaultAsync(p =>
                p.Id == usuario.PreferenciaNotificacionId);

        if (preferencia == null)
        {
            throw new Exception(
                "No se encontró la preferencia de notificación del usuario."
            );
        }

        if (preferencia.Id == 1)
        {
            if (notificacion.Archivo == null)
            {
                await EnviarCorreoAsync(
                    usuario.Correo,
                    notificacion.Asunto,
                    notificacion.Mensaje
                );
            }
            else
            {
                await EnviarCorreoConAdjuntoAsync(
                    usuario.Correo,
                    notificacion.Asunto,
                    notificacion.Mensaje,
                    notificacion.Archivo,
                    notificacion.NombreArchivo!
                );
            }
        }

        if (preferencia.Id == 2)
        {
            // WhatsApp se implementará posteriormente.
        }

        if (preferencia.Id == 3)
        {
            if (notificacion.Archivo == null)
            {
                await EnviarCorreoAsync(
                    usuario.Correo,
                    notificacion.Asunto,
                    notificacion.Mensaje
                );
            }
            else
            {
                await EnviarCorreoConAdjuntoAsync(
                    usuario.Correo,
                    notificacion.Asunto,
                    notificacion.Mensaje,
                    notificacion.Archivo,
                    notificacion.NombreArchivo!
                );
            }

            // WhatsApp se implementará posteriormente.
        }
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

