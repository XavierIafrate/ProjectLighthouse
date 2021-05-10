using ProjectLighthouse;
using System;
using System.Collections.Generic;

namespace dev
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SENTRY ANALYTICS");
            RetreiveData();
        }

        static void RetreiveData()
        {
            DateTime fromDate = DateTime.Now.AddDays(-7);
            DateTime toDate = DateTime.Now;

            Console.WriteLine(String.Format("Processing data from {0:dd/MM HH:mm} to {1:dd/MM HH:mm}", fromDate, toDate));

            //List<MachineStatistics> statistics = DatabaseHelper.Read<MachineStatistics>().Where()
        }

        static void ProcessData()
        {

        }

        static void PrintData()
        {

        }
    }
}
