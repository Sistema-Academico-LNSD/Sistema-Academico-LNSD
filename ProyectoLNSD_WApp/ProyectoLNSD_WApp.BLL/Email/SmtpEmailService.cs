using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ProyectoLNSD_WApp.BLL.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task EnviarCorreoRecuperacion(
            string correoDestino,
            string nombreDestino,
            string enlaceRecuperacion)
        {
            using var mensaje = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromName),
                Subject = "Recuperación de contraseña - Campus Virtual LNSD",
                Body =
                    $"Hola {nombreDestino},\n\n" +
                    "Recibimos una solicitud para restablecer tu contraseña. " +
                    "Si fuiste vos, hacé clic en el siguiente enlace (válido por 30 minutos):\n\n" +
                    $"{enlaceRecuperacion}\n\n" +
                    "Si no solicitaste este cambio, podés ignorar este correo; " +
                    "tu contraseña actual seguirá funcionando con normalidad.",
                IsBodyHtml = false
            };

            mensaje.To.Add(correoDestino);

            using var cliente = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            await cliente.SendMailAsync(mensaje);
        }
    }
}