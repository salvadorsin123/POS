using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.IO;

//csiq hdse ijya gybc

namespace POS.Ventas
{
    class Correo
    {
        public string enviarCorreo(string to, string asunto, string body, string rutaArchivo = null)
        {

            // Validación silenciosa - cancela si no hay correo
            if (string.IsNullOrWhiteSpace(to))
            {
                return string.Empty; // Retorna vacío sin mensaje de error
            }

            string msge = "Error al enviar este correo.";
            string from = "gemsal22@gmail.com"; // tu correo Gmail
            string displayName = "Bicicletas Casa Tavares";
            string password = "csiq hdse ijya gybc"; // la contraseña generada

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(from, displayName);
                    mail.To.Add(to);
                    mail.Subject = asunto;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(rutaArchivo) && File.Exists(rutaArchivo))
                        mail.Attachments.Add(new Attachment(rutaArchivo));

                    using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                    {
                        client.Credentials = new NetworkCredential(from, password);
                        client.EnableSsl = true;
                        client.Send(mail);
                    }
                }

                msge = "¡Correo enviado exitosamente!";
            }
            catch (Exception ex)
            {
                msge = $"Error: {ex.Message}";
            }

            return msge;
        }
    }
}
