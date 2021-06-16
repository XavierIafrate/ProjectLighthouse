using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class EmailHelper
    {
        public static void SendEmail(string toPerson, string alertSubject, string message)
        {
            Email mailMan = new Email();

            EmailSendConfigure myConfig = new EmailSendConfigure();
            myConfig.TOs = new string[] { toPerson };
            myConfig.CCs = new string[] { };
            myConfig.From = "lighthouse@wixroydgroup.com";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = MailPriority.Normal;
            myConfig.Subject = alertSubject;

            EmailContent myContent = new EmailContent();
            myContent.Content = message;

            mailMan.SendMail(myConfig, myContent);
        }

        public static void NotifyRequestApproved(Request approvedRequest)
        {
            Email mailMan = new Email();
            EmailSendConfigure myConfig = new EmailSendConfigure();

            List<User> users = DatabaseHelper.Read<User>();
            User PersonWhoRaisedRequest = new User();
            foreach (User user in users)
            {
                if (user.GetFullName() == approvedRequest.RaisedBy)
                {
                    PersonWhoRaisedRequest = user;
                    break;
                }
            }

            if (string.IsNullOrEmpty(PersonWhoRaisedRequest.EmailAddress))
                return;

            myConfig.TOs = new string[] { PersonWhoRaisedRequest.EmailAddress };
            myConfig.CCs = new string[] { };
            myConfig.From = "lighthouse@wixroydgroup.com";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = MailPriority.Normal;
            myConfig.Subject = "Request Approved";

            EmailContent myContent = new EmailContent();
            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#00695C'>Request approved!</h2>" +
                $"<p>Good {greeting} {PersonWhoRaisedRequest.FirstName}, your request for {approvedRequest.QuantityRequired:#,##0}pcs" +
                $" of {approvedRequest.Product} has been approved.</p><p>Please update Lighthouse with the purchase order reference." +
                $"</p></font></html>";

            myContent.IsHtml = true;
            myContent.Content = message;

            mailMan.SendMail(myConfig, myContent);
        }

        public static void NotifyRequestDeclined(Request declinedRequest)
        {
            Email mailMan = new Email();
            EmailSendConfigure myConfig = new EmailSendConfigure();

            List<User> users = DatabaseHelper.Read<User>();
            User PersonWhoRaisedRequest = new User();
            foreach (User user in users)
            {
                if (user.GetFullName() == declinedRequest.RaisedBy)
                {
                    PersonWhoRaisedRequest = user;
                    break;
                }
            }

            if (string.IsNullOrEmpty(PersonWhoRaisedRequest.EmailAddress))
                return;

            myConfig.TOs = new string[] { PersonWhoRaisedRequest.EmailAddress };
            myConfig.CCs = new string[] { };
            myConfig.From = "lighthouse@wixroydgroup.com";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = MailPriority.Normal;
            myConfig.Subject = "Request Declined";

            EmailContent myContent = new EmailContent();
            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#B00020'>Request declined</h2>" +
                $"<p>Good {greeting} {PersonWhoRaisedRequest.FirstName}, unfortunately your request for {declinedRequest.QuantityRequired:#,##0}pcs" +
                $" of {declinedRequest.Product} has been declined.</p><p>Reason: {declinedRequest.DeclinedReason}" +
                $"</p></font></html>";

            myContent.IsHtml = true;
            myContent.Content = message;

            mailMan.SendMail(myConfig, myContent);
        }

        public static void NotifyNewOrder(LatheManufactureOrder order, List<LatheManufactureOrderItem> items)
        {
            Email mailMan = new();
            EmailSendConfigure emailConfig = new EmailSendConfigure();
            List<User> users = DatabaseHelper.Read<User>();
            List<User> send_to = new();
            List<string> emails = new();
            foreach (User user in users)
            {
                if (user.UserRole == "Production" && !string.IsNullOrEmpty(user.EmailAddress))
                {
                    send_to.Add(user);
                    emails.Add(user.EmailAddress);
                }
            }

            emailConfig.TOs = emails.ToArray();
            //myConfig.TOs = new string[] { "x.iafrate@wixroydgroup.com" };
            emailConfig.CCs = new string[] { "x.iafrate@wixroydgroup.com" };
            emailConfig.From = "lighthouse@wixroydgroup.com";
            emailConfig.FromDisplayName = "Lighthouse Notifications";
            emailConfig.Priority = MailPriority.Normal;
            emailConfig.Subject = $"New Manufacture Order - {order.Name}";

            EmailContent myContent = new EmailContent();
            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#00695C'>New Manufacture Order</h2>" +
                $"<p>Good {greeting}, a new Manufacture Order ({order.Name}) has been raised.</p>" +
                $"<p>Please update the order details in Lighthouse at your earliest convenience.</p>";


            foreach (LatheManufactureOrderItem item in items)
                if (item.RequiredQuantity > 0)
                {
                    message += string.Format("<p><b>Customer requirement:</b> {0} - {1:#,##0}pcs for {2:dddd d MMMM}. Target quantity: {3:#,##0}pcs</p>",
                        item.ProductName, item.RequiredQuantity, item.DateRequired, item.TargetQuantity);
                }
                else
                {
                    message += string.Format("<p>{0} - {1:#,##0}pcs</p>",
                       item.ProductName, item.TargetQuantity);
                }

            message += "</font></html>";

            myContent.IsHtml = true;
            myContent.Content = message;

            mailMan.SendMail(emailConfig, myContent);
        }

    }
}
