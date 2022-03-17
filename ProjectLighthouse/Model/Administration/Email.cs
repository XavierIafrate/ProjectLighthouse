using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace ProjectLighthouse.Model
{
    public class Email
    {
        private EmailFrom emailFrom = new(
                emailAddressId: "17482",
                name: "Lighthouse Notifications"
            );

        private Configuration config = new()
        {
            Username = "x.iafrate@wixroydgroup.com",
            Password = ClickSendCreds.API_KEY
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send email\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
    }
}

