using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Windows;

namespace ProjectLighthouse.Model
{
    public class Email
    {
        public Email()
        {
        }

        public void SendMail(EmailSendConfigure emailConfig, EmailContent content)
        {
            MailMessage msg = ConstructEmailMessage(emailConfig, content);
            Send(msg, emailConfig);
        }

        // Put the properties of the email including "to", "cc", "from", "subject" and "email body"  
        private static MailMessage ConstructEmailMessage(EmailSendConfigure emailConfig, EmailContent content)
        {
            MailMessage msg = new();
            foreach (string to in emailConfig.TOs)
            {
                if (!string.IsNullOrEmpty(to))
                {
                    msg.To.Add(to);
                }
            }

            foreach (string cc in emailConfig.CCs)
            {
                if (!string.IsNullOrEmpty(cc))
                {
                    msg.CC.Add(cc);
                }
            }

            msg.From = new MailAddress(emailConfig.From,
                                       emailConfig.FromDisplayName,
                                       Encoding.UTF8);
            msg.IsBodyHtml = content.IsHtml;
            msg.Body = content.Content;
            msg.Priority = emailConfig.Priority;
            msg.Subject = emailConfig.Subject;
            msg.BodyEncoding = Encoding.UTF8;
            msg.SubjectEncoding = Encoding.UTF8;

            return msg;
        }

        //Send the email using the SMTP server  
        private static void Send(MailMessage message, EmailSendConfigure emailConfig)
        {
            SmtpClient client = new()
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(
                                  "xav@lighthouse.software",
                                  "L$tcxtRbxTPcnL6m"),
                Host = "smtp.gmail.com",
                Port = 587,  // this is critical
                EnableSsl = true  // this is critical
            };

            if (Debugger.IsAttached)
            {
                if (MessageBox.Show("Email triggered, do you want to send?", "Debug options", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    message.Dispose();
                    return;
                }
            }
            
            try
            {
                client.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Send email: {0}", e.Message);
            }

            message.Dispose();
        }

    }

    public class EmailSendConfigure
    {
        public string[] TOs { get; set; }
        public string[] CCs { get; set; }
        public string From { get; set; }
        public string FromDisplayName { get; set; }
        public string Subject { get; set; }
        public string ClientCredentialUserName { get; set; }
        public string ClientCredentialPassword { get; set; }
        public MailPriority Priority { get; set; }
        public EmailSendConfigure()
        {
        }
    }

    public class EmailContent
    {
        public bool IsHtml { get; set; }
        public string Content { get; set; }
        public string AttachFileName { get; set; }
    }
}

