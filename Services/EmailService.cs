using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MiRoti.Services
{
    public class EmailService
    {
        private readonly string _from = "no-reply@miroti.com";
        private readonly string _smtp = "smtp.gmail.com";
        private readonly int _port = 587;
        private readonly string _password = "tu_contraseña_segura"; // ⚠️ reemplazar

        // ✅ Enviar correo básico (asincrónico)
        public async Task EnviarCorreoAsync(string destino, string asunto, string cuerpo)
        {
            using (var client = new SmtpClient(_smtp, _port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_from, _password);

                var mail = new MailMessage(_from, destino, asunto, cuerpo)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mail);
            }
        }
    }
}
