using IO.ClickSend.ClickSend.Model;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class EmailHelper
    {
        public void SendDailyRuntimeReport(List<EmailRecipient> to, AnalyticsHelper data, DateTime? fromDate = null)
        {
            Model.Email email = new();
            email.TOs = to;

            DateTime startingDate;
            if (fromDate != null)
            {
                startingDate = (DateTime)fromDate;
            }
            else
            {
                startingDate = DateTime.Today.AddDays(-1).AddHours(6);
            }

            string message = data.GetDailyEmailMessage(startingDate);

            email.Send($"Machine Runtime report for {FormatDateForEmailSubject(startingDate)}", message);
        }

        private static string FormatDateForEmailSubject(DateTime date)
        {
            int dayOfMonth = date.Day;
            string ordinal;

            ordinal = dayOfMonth switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };

            return $"{dayOfMonth}{ordinal} {date:MMMM}";
        }

        public static void NotifyRequestApproved(Request approvedRequest)
        {
            Model.Email email = new();

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

            if (string.IsNullOrEmpty(PersonWhoRaisedRequest.EmailAddress))
                return;

            email.TOs.Add(new EmailRecipient(
                email: PersonWhoRaisedRequest.EmailAddress,
                name: PersonWhoRaisedRequest.GetFullName()
                ));

            string subject = "Request Approved";
            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#00695C'>Request approved!</h2>" +
                $"<p>Good {greeting} {PersonWhoRaisedRequest.FirstName}, your request for {approvedRequest.QuantityRequired:#,##0}pcs" +
                $" of {approvedRequest.Product} has been approved.</p><p>Please update Lighthouse with the purchase order reference." +
                $"</p></font></html>";

            email.Send(subject, message);
        }

        public static void NotifyRequestDeclined(Request declinedRequest)
        {
            Model.Email email = new();
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

            email.TOs.Add(new EmailRecipient(
                email: PersonWhoRaisedRequest.EmailAddress,
                name: PersonWhoRaisedRequest.GetFullName()
                ));

            string subject = "Request Declined";

            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#B00020'>Request declined</h2>" +
                $"<p>Good {greeting} {PersonWhoRaisedRequest.FirstName}, unfortunately your request for {declinedRequest.QuantityRequired:#,##0}pcs" +
                $" of {declinedRequest.Product} has been declined.</p><p>Reason: {declinedRequest.DeclinedReason}" +
                $"</p></font></html>";


            email.Send(subject, message);
        }

        public static void NotifyNewRequest(Request request, TurnedProduct turnedProduct, User personWhoRaised)
        {
            Model.Email email = new();
            List<User> users = DatabaseHelper.Read<User>();

            foreach (User user in users)
            {
                if ((user.Role >= UserRole.Scheduling) && user.CanApproveRequests && !string.IsNullOrEmpty(user.EmailAddress) && user.ReceivesNotifications)
                {
                    email.TOs.Add(new EmailRecipient(
                        email: user.EmailAddress,
                        name: user.GetFullName()
                        ));
                }
            }





            EmailRecipient RaisedBy = new(
                email: personWhoRaised.EmailAddress,
                name: personWhoRaised.GetFullName());

            if (!email.TOs.Contains(RaisedBy) && personWhoRaised.ReceivesNotifications)
            {
                email.CCs.Add(RaisedBy);
            }


            if (email.TOs.Count == 0)
            {
                return;
            }

            string subject = $"New Request Raised - {request.Product}";


            string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";

            string message = $"<html><font face='tahoma'><h2 style='color:#01579B'>New Manufacture Request</h2>" +
                $"<p>Good {greeting}, a new request for {request.QuantityRequired:#,##0} pcs of {request.Product} has been raised.</p>" +
                $"<p>Please approve/decline this in Lighthouse at your earliest convenience.</p>" +
                "<p></p>" +
                $"<p>Material: {turnedProduct.Material}</p>" +
                $"<p>Major length: {turnedProduct.MajorLength}mm</p>" +
                $"<p>Major diameter: {turnedProduct.MajorDiameter}mm</p>" +
                $"<p>Thread: {turnedProduct.ThreadSize}</p>" +
                $"<p>Drive: '{turnedProduct.DriveType}', size '{turnedProduct.DriveSize}'</p>" +
                $"<p>Date Required: {request.DateRequired:d}</p>";


            if (turnedProduct.QuantityManufactured > 0)
            {
                message += $"<p>Made in house: {turnedProduct.QuantityManufactured:#,##0} pcs, last made {turnedProduct.lastManufactured:D}</p>";
            }
            message += "</font></html>";

            email.Send(subject, message);
        }
    }
}
