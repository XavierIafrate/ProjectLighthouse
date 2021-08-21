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
            Email mailMan = new();

            EmailSendConfigure myConfig = new();
            myConfig.TOs = new string[] { toPerson };
            myConfig.CCs = Array.Empty<string>();
            myConfig.From = "notifications@lighthouse.software";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = MailPriority.Normal;
            myConfig.Subject = alertSubject;

            EmailContent myContent = new();
            myContent.Content = message;

            mailMan.SendMail(myConfig, myContent);
        }

        public static void NotifyRequestApproved(Request approvedRequest)
        {
            Email mailMan = new();
            EmailSendConfigure myConfig = new();

            List<User> users = DatabaseHelper.Read<User>();
            User PersonWhoRaisedRequest = new();
            foreach (User user in users)
            {
                if (user.GetFullName() == approvedRequest.RaisedBy)
                {
                    PersonWhoRaisedRequest = user;
                    break;
                }
            }

            if (string.IsNullOrEmpty(PersonWhoRaisedRequest.EmailAddress)) return;

            myConfig.TOs = new string[] { PersonWhoRaisedRequest.EmailAddress };
            myConfig.CCs = Array.Empty<string>();
            myConfig.From = "notifications@lighthouse.software";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = MailPriority.Normal;
            myConfig.Subject = "Request Approved";

            EmailContent myContent = new();
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
            Email mailMan = new();
            EmailSendConfigure myConfig = new();

            List<User> users = DatabaseHelper.Read<User>();
            User PersonWhoRaisedRequest = new();
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
            myConfig.CCs = Array.Empty<string>();
            myConfig.From = "notifications@lighthouse.software";
            myConfig.FromDisplayName = "Lighthouse Notifications";
            myConfig.Priority = MailPriority.Normal;
            myConfig.Subject = "Request Declined";

            EmailContent myContent = new();
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
            EmailSendConfigure emailConfig = new();
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
            emailConfig.CCs = new string[] { "x.iafrate@wixroydgroup.com", "purchasing@automotioncomponents.co.uk" };
            emailConfig.From = "notifications@lighthouse.software";
            emailConfig.FromDisplayName = "Lighthouse Notifications";
            emailConfig.Priority = MailPriority.Normal;
            emailConfig.Subject = $"New Manufacture Order - {order.Name}";

            EmailContent myContent = new();
            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#00695C'>New Manufacture Order</h2>" +
                $"<p>Good {greeting}, a new Manufacture Order ({order.Name}) has been raised.</p>" +
                $"<p>Please update the order details in Lighthouse at your earliest convenience.</p>";


            foreach (LatheManufactureOrderItem item in items)
            {
                if (item.RequiredQuantity > 0)
                {
                    message += $"<p><b>Customer requirement:</b> {item.ProductName} - {item.RequiredQuantity:#,##0}pcs for {item.DateRequired:dddd d MMMM}. Target quantity: {item.TargetQuantity:#,##0}pcs</p>";
                }
                else
                {
                    message += $"<p>{item.ProductName} - {item.TargetQuantity:#,##0}pcs</p>";
                }
            }

            message += "</font></html>";

            myContent.IsHtml = true;
            myContent.Content = message;

            mailMan.SendMail(emailConfig, myContent);
        }

        public static void NotifyNewRequest(Request request, TurnedProduct turnedProduct, User personWhoRaised)
        {
            Email mailMan = new();
            EmailSendConfigure emailConfig = new();
            List<User> users = DatabaseHelper.Read<User>();

            List<string> emails = new();
            List<string> emails_cc = new();

            foreach (User user in users)
            {
                if ((user.UserRole == "Scheduling" || user.UserRole == "admin") && user.CanApproveRequests && !string.IsNullOrEmpty(user.EmailAddress))
                {
                    emails.Add(user.EmailAddress);
                }
            }

            if (!emails.Contains(personWhoRaised.EmailAddress))
            {
                emails_cc.Add(personWhoRaised.EmailAddress);
            }


            emailConfig.TOs = emails.ToArray();
            //emailConfig.TOs = new string[] { "xavieriafrate@gmail.com" };
            emailConfig.CCs = emails_cc.ToArray();
            emailConfig.From = "notifications@lighthouse.software";
            emailConfig.FromDisplayName = "Lighthouse Notifications";
            emailConfig.Priority = MailPriority.Normal;
            emailConfig.Subject = $"New Request Raised - {request.Product}";
            

            EmailContent myContent = new();
            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#01579B'>New Manufacture Request</h2>" +
                $"<p>Good {greeting}, a new request for {request.QuantityRequired:#,##0} pcs of {request.Product} has been raised.</p>" +
                $"<p>Please approve/decline this in Lighthouse at your earliest convenience.</p>" +
                "<p></p>" +
                $"<p>Material: {turnedProduct.Material}</p>" +
                $"<p>Major length: {turnedProduct.MajorLength}mm</p>" +
                $"<p>Major diameter: {turnedProduct.MajorDiameter}mm</p>" +
                $"<p>Thread: {turnedProduct.ThreadSize}</p>" +
                $"<p>Drive: '{turnedProduct.DriveType}', size '{turnedProduct.DriveSize}'</p>";


            if (turnedProduct.QuantityManufactured > 0)
            {
                message += $"<p>Made in house: {turnedProduct.QuantityManufactured:#,##0} pcs, last made {turnedProduct.lastManufactured:D}</p>";
            }

            message += "</font></html>";

            myContent.IsHtml = true;
            myContent.Content = message;

            mailMan.SendMail(emailConfig, myContent);
        }


        public static void TestEmail()
        {
            Email mailMan = new();
            EmailSendConfigure emailConfig = new();

            emailConfig.TOs = new string[] { "xavieriafrate@gmail.com" };
            emailConfig.CCs = Array.Empty<string>();
            emailConfig.From = "notifications@lighthouse.software";
            emailConfig.FromDisplayName = "Lighthouse Notifications";
            emailConfig.Priority = MailPriority.Normal;
            emailConfig.Subject = "TEST";

            EmailContent myContent = new();

            string message = "This is a test email";

            myContent.IsHtml = true;
            myContent.Content = message;

            mailMan.SendMail(emailConfig, myContent);
        }
    }
}
