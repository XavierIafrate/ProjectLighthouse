using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class ReportingHelper
    {
        public static void GetReport()
        {
            DateTime fromDate = DateTime.Now.AddDays(-7);
            DateTime toDate = DateTime.Now;

            Debug.WriteLine(string.Format("Processing data from {0:dd/MM HH:mm} to {1:dd/MM HH:mm}.", fromDate, toDate));
            Performace performace = FetchData(fromDate, toDate);
            PrintPerformance(performace);
        }

        private static void PrintPerformance(Performace p)
        {
            foreach(PerformanceByMachine m in p.summary)
            {
                logLine("++++++++++++++++++++++++++++++++++");
                logLine(string.Format("MachineID:     {0}", m.MachineID));
                logLine(string.Format("Machine Model: {0}", m.MachineModel));
                logLine(string.Format("From Time:     {0:dd/MM HH:mm}", m.fromDate));
                logLine(string.Format("To Time:       {0:dd/MM HH:mm}", m.toDate));
                //logLine(string.Format("Duration:      {0:dd/MM HH:mm}", m.duration));
                logLine(string.Format("Dec Uptime:    {0:dd/MM HH:mm}", m.decimalUptime));
                logLine(string.Format("Parts Prod:    {0}", m.numPartsProduced));
                logLine("");

                foreach(PerformanceBlob b in m.performance)
                {
                    logLine(string.Format("State:         {0}", b.state));
                    logLine(string.Format("Duration:      {0}", b.duration));
                    logLine(string.Format("From Time:     {0:ddd dd/MM HH:mm}", b.startTime));
                    logLine(string.Format("To Time:       {0:ddd dd/MM HH:mm}", b.endTime));
                    logLine("");
                }
            }
        }

        private static void logLine(string line)
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(docPath, String.Format("{0}.txt", "performance")), append: true))
            {
                outputFile.WriteLine(line);
            }
        }

        private static void WriteToExcel(Performace p)
        {
            Microsoft.Office.Interop.Excel.Application oXL = null;
            Microsoft.Office.Interop.Excel._Workbook oWB = null;
            Microsoft.Office.Interop.Excel._Worksheet oSheet = null;

            try
            {
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oWB = oXL.Workbooks.Open("d:\\MyExcel.xlsx");
                oSheet = String.IsNullOrEmpty(sheetName) ? (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet : (Microsoft.Office.Interop.Excel._Worksheet)oWB.Worksheets[sheetName];

                oSheet.Cells[row, col] = data;

                oWB.Save();

                MessageBox.Show("Done!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (oWB != null)
                    oWB.Close();
            }
        }

        private static Performace FetchData(DateTime from, DateTime to)
        {
            DateTime start = DateTime.Now;
            List<MachineStatistics> statistics = DatabaseHelper.Read<MachineStatistics>().Where(n => n.DataTime > from && n.DataTime < to).ToList();
            Debug.WriteLine(string.Format("{0} records found in {1}s", statistics.Count, (DateTime.Now - start).TotalSeconds));

            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();

            Performace performace = new Performace();
            performace.summary = new List<PerformanceByMachine>();
            PerformanceByMachine tmpMachineSummary = new PerformanceByMachine();

            foreach (Lathe lathe in lathes)
            {
                tmpMachineSummary = new PerformanceByMachine()
                {
                    MachineID = lathe.FullName,
                    MachineModel = lathe.Model,
                    fromDate = from,
                    toDate = to
                };
                Debug.WriteLine(string.Format("Fetching information for {0}", lathe.FullName));
                tmpMachineSummary.performance = GetPerformanceBlob(tmpMachineSummary.MachineID, statistics);
                Debug.WriteLine(string.Format("{0} performance blobs", tmpMachineSummary.performance.Count));
                performace.summary.Add(tmpMachineSummary);
            }
            statistics.Clear();
            Debug.WriteLine(string.Format("Complete in {0}s", (DateTime.Now - start).TotalSeconds));
            return performace;
        }

        private static List<PerformanceBlob> GetPerformanceBlob(string machineID, List<MachineStatistics> stats)
        {
            List<PerformanceBlob> results = new List<PerformanceBlob>();
            PerformanceBlob blob = new PerformanceBlob();
            string state = String.Empty;
            DateTime lastDateTime = DateTime.MinValue;

            foreach(MachineStatistics s in stats)
            {
                if (s.MachineID != machineID)
                    continue;
                if (blob.state == null)
                {
                    lastDateTime = s.DataTime;
                    blob = new PerformanceBlob()
                    {
                        state = s.Status,
                        startTime = s.DataTime
                    };
                    state = s.Status;
                }

                if (state != s.Status)
                {
                    blob.endTime = lastDateTime;
                    blob.duration = blob.endTime - blob.startTime;
                    results.Add(blob);
                    blob = new PerformanceBlob()
                    {
                        state = s.Status,
                        startTime = lastDateTime
                    };
                    state = s.Status;
                }

                lastDateTime = s.DataTime;
            }

            return results;
        }

        private class PerformanceBlob
        {
            public string state { get; set; }
            public TimeSpan duration { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
        }

        private class PerformanceByMachine
        {
            public string MachineID { get; set; }
            public string MachineModel { get; set; }
            public TimeSpan duration { get; set; }
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
            public List<PerformanceBlob> performance { get; set; }
            public double decimalUptime { get; set; }
            public int numPartsProduced { get; set; }
        }

        private class Performace
        {
            public List<PerformanceByMachine> summary { get; set; }
        }
    }
}
