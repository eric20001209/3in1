using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;

namespace eCommerce_API.Services
{
    public interface iMailService
    {
        string email { get; set; }
        string password { get; set; }
        Task sendEmail(string email,string subject, string content, Attachment attachment);
    }
    public class MailService : iMailService
    {
        [EmailAddress]
        public string email { get; set; }
        public string password { get; set; }
        private ILogger<MailService> _logger;
		public MailService(ILogger<MailService> logger)
		{
            _logger = logger;
		}

        public async Task sendEmail(string email,string subject, string content,Attachment attachment)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                //message.From = new MailAddress("gpossuport2176@gmail.com");
                message.From = new MailAddress("dollaritemnz@gmail.com");
                message.To.Add(new MailAddress(email));
                message.Subject = subject;
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = content;
                //message.Body = "Hi, this is your new password<br>";
                //message.Body += "<b>NEW PASSWORD</b> is (" + password + ") !!";
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com"; //for gmail host  
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                if (attachment != null)
                    message.Attachments.Add(attachment);
        //      smtp.Credentials = new NetworkCredential("gpossuport2176@gmail.com", "suocqnxvfxaqvrjd"); 
                smtp.Credentials = new NetworkCredential("dollaritemnz@gmail.com", "zwwqqunuyugwdhjf");
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\r\n" + $"Send email error.");
                return ;
            }
        }
    }
}
