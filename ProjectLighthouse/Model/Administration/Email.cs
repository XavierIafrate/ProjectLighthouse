using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;
using System.Diagnostics;

namespace ProjectLighthouse.Model
{
    public class Email
    {
        private EmailFrom emailFrom = new(
                emailAddressId: "17482",
                name: "Lighthouse Notifications"
            );

        private Configuration config= new()
        {
            Username = "x.iafrate@wixroydgroup.com",
            Password = "6C01F36C-194B-FF3B-EAC8-45A5180A4D4C"
        };

        public List<EmailRecipient> TOs { get; set; } = new();
        public List<EmailRecipient> CCs { get; set; } = new();
        public List<EmailRecipient> BCCs { get; set; } = new();


        public void Send(string subject, string body)
        {
            if (Debugger.IsAttached)
            {
                if (MessageBox.Show("Email triggered, do you want to send?", "Debug options", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            TransactionalEmailApi API = new(config);
            try
            {
                _ = API.EmailSendPost(new IO.ClickSend.ClickSend.Model.Email(
                    to: TOs,
                    cc: CCs,
                    bcc: BCCs,
                    from: emailFrom,
                    subject: subject,
                    body: body
                    ));

                //object test = JsonConvert.DeserializeObject(response);
            }
            catch
            {
                MessageBox.Show("Failed to send email");
            }
            
        }
    }
}

    //    public void SendMail(EmailSendConfigure emailConfig, EmailContent content)
    //    {
    //        MailMessage msg = ConstructEmailMessage(emailConfig, content);
    //        Send(msg, emailConfig);
    //    }

    //    // Put the properties of the email including "to", "cc", "from", "subject" and "email body"  
    //    private static MailMessage ConstructEmailMessage(EmailSendConfigure emailConfig, EmailContent content)
    //    {
    //        MailMessage msg = new();
    //        foreach (string to in emailConfig.TOs)
    //        {
    //            if (!string.IsNullOrEmpty(to))
    //            {
    //                msg.To.Add(to);
    //            }
    //        }

    //        foreach (string cc in emailConfig.CCs)
    //        {
    //            if (!string.IsNullOrEmpty(cc))
    //            {
    //                msg.CC.Add(cc);
    //            }
    //        }

    //        msg.From = new MailAddress(emailConfig.From,
    //                                   emailConfig.FromDisplayName,
    //                                   Encoding.UTF8);
    //        msg.IsBodyHtml = content.IsHtml;
    //        msg.Body = content.Content;
    //        msg.Priority = emailConfig.Priority;
    //        msg.Subject = emailConfig.Subject;
    //        msg.BodyEncoding = Encoding.UTF8;
    //        msg.SubjectEncoding = Encoding.UTF8;

    //        return msg;
    //    }

    //    //Send the email using the SMTP server  
    //    private static void Send(MailMessage message, EmailSendConfigure emailConfig)
    //    {
    //        SmtpClient client = new()
    //        {
    //            UseDefaultCredentials = false,
    //            Credentials = new System.Net.NetworkCredential(
    //                              "notifications@lighthouse.software",
    //                              "XfCTT!qenR9PTBmR"),
    //            Host = "smtp.123-reg.co.uk",
    //            Port = 465,  // this is critical
    //            EnableSsl = true  // this is critical
    //        };

    //        if (Debugger.IsAttached)
    //        {
    //            if (MessageBox.Show("Email triggered, do you want to send?", "Debug options", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
    //            {
    //                message.Dispose();
    //                return;
    //            }
    //        }

    //        client.Timeout = 5000; // ms

    //        client.Send(message);
    //        return;

    //        try
    //        {
    //            client.Send(message);
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine("Error in Send email: {0}", e.Message);
    //        }

    //        message.Dispose();
    //    }

    //}

    //public class EmailSendConfigure
    //{
    //    public string[] TOs { get; set; }
    //    public string[] CCs { get; set; }
    //    public string From { get; set; }
    //    public string FromDisplayName { get; set; }
    //    public string Subject { get; set; }
    //    public string ClientCredentialUserName { get; set; }
    //    public string ClientCredentialPassword { get; set; }
    //    public MailPriority Priority { get; set; }
    //    public EmailSendConfigure()
    //    {
    //    }
    //}

    //public class EmailContent
    //{
    //    public bool IsHtml { get; set; }
    //    public string Content { get; set; }
    //    public string AttachFileName { get; set; }
    //}


