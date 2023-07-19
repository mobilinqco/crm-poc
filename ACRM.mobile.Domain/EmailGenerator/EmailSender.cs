using ACRM.mobile.Domain.EmailGenerator.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ACRM.mobile.Domain.EmailGenerator
{
    public class EmailSender
    {
        public static async Task SendMailAsync(IMessage email, EmailConfiguration emailConfiguration)
        {
            try
            {
                Email mail = (Email)email;
                using var smtpServer = new SmtpClient(emailConfiguration.SmtpClient);
                smtpServer.Port = emailConfiguration.Port;
                smtpServer.Credentials = new NetworkCredential(emailConfiguration.Username, emailConfiguration.Password);
                smtpServer.EnableSsl = true;
                smtpServer.Timeout = 15000; //15s timeout
                

                MailMessage mailMessage = new MailMessage(emailConfiguration.Email, mail.To, mail.Subject, mail.Body);
                mailMessage.From = new MailAddress(emailConfiguration.Email, string.Empty);
                mailMessage.IsBodyHtml = true;
                await smtpServer.SendMailAsync(mailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
