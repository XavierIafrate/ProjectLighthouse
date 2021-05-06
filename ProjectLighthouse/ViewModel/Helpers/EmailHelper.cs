using ProjectLighthouse.Model;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class EmailHelper
    {
        public static void SendEmail(string toPerson, string alertSubject, string message)
        {
            Email mailMan = new Email();

            EmailSendConfigure myConfig = new EmailSendConfigure();
            myConfig.ClientCredentialUserName = "x.iafrate@wixroydgroup.com";
            myConfig.ClientCredentialPassword = "Teleport55@@";
            myConfig.TOs = new string[] { toPerson };
            myConfig.CCs = new string[] { };
            myConfig.From = "x.iafrate@wixroydgroup.com";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = System.Net.Mail.MailPriority.Normal;
            myConfig.Subject = alertSubject;

            EmailContent myContent = new EmailContent();
            myContent.Content = message;

            mailMan.SendMail(myConfig, myContent);

        }
    }
}
