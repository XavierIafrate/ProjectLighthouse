using ProjectLighthouse;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloseManufactureOrders
{
    class Program
    {
        static string DatabasePath;
        static void Main(string[] args)
        {
            Console.WriteLine("Closing manufacture orders");

#if DEBUG
            DatabasePath = $"{ApplicationRootPaths.DEBUG_ROOT}{ApplicationRootPaths.DEBUG_DB_NAME}";
#else
            DatabasePath = $"{ApplicationRootPaths.RELEASE_ROOT}{ApplicationRootPaths.RELEASE_DB_NAME}";
#endif
            Console.WriteLine($"Database Path = '{DatabasePath}'");

            DatabaseHelper.DatabasePath = DatabasePath;

            CheckForReopenedOrders();
            CheckForClosedOrders();

            Console.WriteLine($"[Process complete]");
            //Console.ReadKey();
        }



        private static void CheckForReopenedOrders()
        {
            // Lathe Manufacture Orders
            List<LatheManufactureOrder> LatheOrders = DatabaseHelper.Read<LatheManufactureOrder>();

            List<LatheManufactureOrder> notClosedLatheOrders = LatheOrders.Where(x => x.IsClosed && x.State < OrderState.Complete).ToList();
            for (int i = 0; i < notClosedLatheOrders.Count; i++)
            {
                notClosedLatheOrders[i].MarkAsNotClosed();
                Console.WriteLine($"Re-opening '{notClosedLatheOrders[i].Name}'");
                LatheOrders.Find(x => x.Id == notClosedLatheOrders[i].Id)!.IsClosed = false;
            }


            // General Manufacture Orders
            List<GeneralManufactureOrder> GeneralOrders = DatabaseHelper.Read<GeneralManufactureOrder>();

            List<GeneralManufactureOrder> notClosedGeneralOrders = GeneralOrders.Where(x => x.IsClosed && x.State < OrderState.Complete).ToList();
            for (int i = 0; i < notClosedGeneralOrders.Count; i++)
            {
                notClosedGeneralOrders[i].MarkAsNotClosed();
                Console.WriteLine($"Re-opening '{notClosedGeneralOrders[i].Name}'");
                GeneralOrders.Find(x => x.Id == notClosedGeneralOrders[i].Id)!.IsClosed = false;
            }
        }

        private static void CheckForClosedOrders()
        {
            CheckForClosedLatheOrders();
            CheckForClosedGeneralOrders();
        }

        private static void CheckForClosedLatheOrders()
        {
            List<LatheManufactureOrder> Orders = DatabaseHelper.Read<LatheManufactureOrder>();
            List<LatheManufactureOrderItem> OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();
            List<Lot> Lots = DatabaseHelper.Read<Lot>();


            List<LatheManufactureOrder> doneButNotClosed = Orders.Where(x => x.State > OrderState.Running && !x.IsClosed).ToList();
            for (int i = 0; i < doneButNotClosed.Count; i++)
            {
                LatheManufactureOrder order = doneButNotClosed[i];
                List<LatheManufactureOrderItem> items = OrderItems.Where(x => x.AssignedMO == order.Name).ToList();
                List<Lot> lots = Lots.Where(x => x.Order == order.Name).ToList();

                if (!OrderCanBeClosed(order, items, lots))
                {
                    continue;
                }

                Console.WriteLine($"Closing '{doneButNotClosed[i].Name}'");

                order.MarkAsClosed();
                Orders.Find(x => x.Id == doneButNotClosed[i].Id)!.IsClosed = true;
            }
        }

        private static void CheckForClosedGeneralOrders()
        {
            List<GeneralManufactureOrder> Orders = DatabaseHelper.Read<GeneralManufactureOrder>();
            List<Lot> Lots = DatabaseHelper.Read<Lot>();


            List<GeneralManufactureOrder> doneButNotClosed = Orders.Where(x => x.State > OrderState.Running && !x.IsClosed).ToList();
            for (int i = 0; i < doneButNotClosed.Count; i++)
            {
                GeneralManufactureOrder order = doneButNotClosed[i];
                List<Lot> lots = Lots.Where(x => x.Order == order.Name).ToList();

                if (!OrderCanBeClosed(order, lots))
                {
                    continue;
                }

                Console.WriteLine($"Closing '{doneButNotClosed[i].Name}'");

                order.MarkAsClosed();
                Orders.Find(x => x.Id == doneButNotClosed[i].Id)!.IsClosed = true;
            }
        }

        static bool OrderCanBeClosed(LatheManufactureOrder order, List<LatheManufactureOrderItem> items, List<Lot> lots)
        {
            if ((order.ModifiedAt ?? DateTime.MinValue).AddDays(1) > DateTime.Now)
            {
                return false;
            }

            if (order.State > OrderState.Running)
            {
                List<LatheManufactureOrderItem> itemsWithBadCycleTimes = items.Where(i => i.CycleTime == 0 && i.QuantityMade > 0).ToList();
                List<Lot> unresolvedLots = lots.Where(l => l.Quantity != 0 && !l.IsDelivered && !l.IsReject && l.AllowDelivery).ToList();

                return itemsWithBadCycleTimes.Count == 0 // ensure cycle time is updated
                    && unresolvedLots.Count == 0; // ensure lots are fully processed
            }
            else
            {
                return false;
            }
        }

        static bool OrderCanBeClosed(GeneralManufactureOrder order, List<Lot> lots)
        {
            if ((order.ModifiedAt ?? DateTime.MinValue).AddDays(1) > DateTime.Now)
            {
                return false;
            }

            if (order.State > OrderState.Running)
            {
                List<Lot> unresolvedLots = lots.Where(l => l.Quantity != 0 && !l.IsDelivered && !l.IsReject && l.AllowDelivery).ToList();

                return unresolvedLots.Count == 0; // ensure lots are fully processed
            }
            else
            {
                return false;
            }
        }
    }
}
