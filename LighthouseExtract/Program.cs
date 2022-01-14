using System;
using System.Collections.Generic;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;

namespace LighthouseExtract
{
    internal class Program
    {
        const string Repo = @"\\groupfile01\Accounts\Tableau";
        static void Main(string[] args)
        {
            PrintApplicationHeader();

            // Orders
            Console.WriteLine("Fetching Lathe Manufacture Orders...");
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
            CSVHelper.WriteCsv(orders, Repo, "ManufactureOrders");

            // Order Items
            Console.WriteLine("Fetching Lathe Manufacture Order Items...");
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>();
            CSVHelper.WriteCsv(items, Repo, "ManufactureOrderItems");

            // Lots
            Console.WriteLine("Fetching Stock Lots...");
            List<Lot> lots = DatabaseHelper.Read<Lot>();
            CSVHelper.WriteCsv(lots, Repo, "StockLots");

            // Bar Stock
            Console.WriteLine("Fetching Bar Stock...");
            List<BarStock> bar = DatabaseHelper.Read<BarStock>();
            CSVHelper.WriteCsv(bar, Repo, "BarStock");

            // Lathes
            Console.WriteLine("Fetching Lathes...");
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
            CSVHelper.WriteCsv(lathes, Repo, "Lathes");

            // Machine Operating Blocks
            Console.WriteLine("Fetching Machine Operating Blocks...");
            List<MachineOperatingBlock> operating = DatabaseHelper.Read<MachineOperatingBlock>();
            CSVHelper.WriteCsv(operating, Repo, "MachineOperating");

            // Request
            Console.WriteLine("Fetching Requests...");
            List<Request> requests = DatabaseHelper.Read<Request>();
            CSVHelper.WriteCsv(requests, Repo, "Requests");

            // Turned Product
            Console.WriteLine("Fetching Turned Products...");
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>();
            CSVHelper.WriteCsv(products, Repo, "Products");

            Console.WriteLine("\n[    PROCESS COMPLETE    ]");
            Console.ReadLine();
        }

        public static void PrintApplicationHeader()
        {
            ConsoleColor initialColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write("\t__    __ _ __  _______  ____ __  __ ____ \n\t\\ \\/\\/ /| |\\ \\/ /| () )/ () \\\\ \\/ /| _) \\\n\t \\_/\\_/ |_|/_/\\_\\|_|\\_\\\\____/ |__| |____/ \n");
            Console.Write("\n\tLighthouse Data Extract\n");
            Console.Write("\n\t(C) Wixroyd International Ltd 2022\n\n");
            Console.Write("\n\t============================================================================\n\n");
            Console.ForegroundColor = initialColour;
        }
    }
}
