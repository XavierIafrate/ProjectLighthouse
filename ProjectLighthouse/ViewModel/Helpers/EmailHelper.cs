using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class EmailHelper
    {
        public void SendEmail()
        {

            //Not set up 
            var fromAddress = new MailAddress("alextodorov01@abv.bg", "From Name");
            var toAddress = new MailAddress("kozichka01@abv.bg", "To Name");
            const string fromPassword = "fromPassword";
            const string subject = "an error ocurred";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = "TEST"
            })
            {
                smtp.Send(message);
            }
        }
    }
}
