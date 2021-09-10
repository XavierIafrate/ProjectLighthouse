using IO.ClickSend.ClickSend.Model;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class EmailHelper
    {
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

        public static void NotifyNewOrder(LatheManufactureOrder order, List<LatheManufactureOrderItem> items)
        {
            Model.Email email = new();
            List<User> users = DatabaseHelper.Read<User>();

            foreach (User user in users)
            {
                if (user.UserRole == "Production" && !string.IsNullOrEmpty(user.EmailAddress))
                {
                    Debug.WriteLine($"TO: {user.EmailAddress}");
                    email.TOs.Add(new EmailRecipient()
                    {
                        Name = user.GetFullName(),
                        Email = user.EmailAddress
                    });
                }

                if (user.UserRole == "Scheduling" && !string.IsNullOrEmpty(user.EmailAddress))
                {
                    Debug.WriteLine($"TO: {user.EmailAddress}");
                    email.TOs.Add(new EmailRecipient()
                    {
                        Name = user.GetFullName(),
                        Email = user.EmailAddress
                    });
                }
            }

            email.CCs.Add(new EmailRecipient()
            {
                Name = "Automotion Purchasing",
                Email = "purchasing@automotioncomponents.co.uk"
            });

            string subject = $"New Manufacture Order - {order.Name}";
            Debug.WriteLine($"TO: {subject}");

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

            email.Send(subject, message);
        }

        public static void NotifyNewRequest(Request request, TurnedProduct turnedProduct, User personWhoRaised)
        {
            Model.Email email = new();
            List<User> users = DatabaseHelper.Read<User>();

            foreach (User user in users)
            {
                if ((user.UserRole == "Scheduling" || user.UserRole == "admin") && user.CanApproveRequests && !string.IsNullOrEmpty(user.EmailAddress))
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

            if (!email.TOs.Contains(RaisedBy))
            {
                email.CCs.Add(RaisedBy);
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
                $"<p>Drive: '{turnedProduct.DriveType}', size '{turnedProduct.DriveSize}'</p>";


            if (turnedProduct.QuantityManufactured > 0)
            {
                message += $"<p>Made in house: {turnedProduct.QuantityManufactured:#,##0} pcs, last made {turnedProduct.lastManufactured:D}</p>";
            }
            message += "</font></html>";

            email.Send(subject, message);
        }


        public static void TestEmail()
        {
            Model.Email email = new();

            email.TOs.Add(new(
                email: "x.iafrate@wixroydgroup.com",
                name: "Xav"
                ));

            email.Send("Test Subject",
                 $"<html><font face='tahoma'><h2 style='color:#00695C'>New Manufacture Order</h2>" +
                $"<p>Good morning, a new Manufacture Order (TEST) has been raised.</p>" +
                $"<p>Please update the order details in Lighthouse at your earliest convenience.</p>");
        }
    }
}
