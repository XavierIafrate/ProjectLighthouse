using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class ReportingHelper
    {
        public static void GetReport()
        {
            DateTime fromDate = DateTime.Now.AddDays(-7).Date;
            DateTime toDate = DateTime.Now;

            Debug.WriteLine(string.Format("Processing data from {0:dd/MM HH:mm} to {1:dd/MM HH:mm}.", fromDate, toDate));
            Performace performace = FetchData(fromDate, toDate);
            PrintPerformance(performace);
        }

        private static void PrintPerformance(Performace p)
        {
            foreach (PerformanceByMachine m in p.summary)
            {
                logLine("++++++++++++++++++++++++++++++++++");
                logLine(string.Format("Machine ID:    {0}", m.MachineID));
                logLine(string.Format("Machine Make:  {0}", m.MachineMake));
                logLine(string.Format("Machine Model: {0}", m.MachineModel));
                logLine(string.Format("From Time:     {0:dd/MM HH:mm}", m.fromDate));
                logLine(string.Format("To Time:       {0:dd/MM HH:mm}", m.toDate));
                logLine("");

                foreach (PerformanceBlob b in m.performance)
                {
                    logLine($"{m.MachineID},{b.state},{b.duration},{b.startTime},{b.endTime},{b.StartQuantity},{b.EndQuantity},{b.HasStep},{b.CycleTime}");
                    //logLine(string.Format("State:         {0}", b.state));
                    //logLine(string.Format("Duration:      {0}", b.duration));
                    //logLine(string.Format("From Time:     {0:ddd dd/MM HH:mm}", b.startTime));
                    //logLine(string.Format("To Time:       {0:ddd dd/MM HH:mm}", b.endTime));
                    //logLine("");
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
            DateTime CounterLastChanged = DateTime.MinValue;

            foreach (MachineStatistics s in stats)
            {
                if (s.MachineID != machineID) // Only get records for relevant machine
                    continue;

                if (blob.state == null) // Initiate first blob
                {
                    lastDateTime = s.DataTime;
                    blob = new PerformanceBlob()
                    {
                        HasStep = false,
                        state = s.Status,
                        startTime = s.DataTime,
                        CycleTime = s.CycleTime
                    };
                    state = s.Status;
                }

                // Check for steps
                // TODO: Check for plateau
                if (blob.state == "Running") // extremely fuzzy logic
                {
                    int calc_parts = (int)(s.DataTime - lastDateTime).TotalSeconds / s.CycleTime;
                    int parts_made = s.PartCountAll - blob.EndQuantity;
                    if (parts_made != 0)
                        CounterLastChanged = s.DataTime;
                    if (parts_made < 0 || parts_made > 1) // If not incrementing by one then check what's going on
                    {
                        if (Math.Abs(parts_made - calc_parts) > 5 || parts_made < 0) // attempt to account for large gap in data (eg if sentry is down) with wiggle room
                        {
                            blob.HasStep = true;
                            blob.endTime = lastDateTime;
                            blob.EndQuantity = s.PartCountAll;
                            blob.duration = blob.endTime - blob.startTime;
                            results.Add(blob);
                            blob = new PerformanceBlob()
                            {
                                HasStep = false,
                                state = s.Status,
                                startTime = lastDateTime,
                                CycleTime = s.CycleTime,
                                StartQuantity = s.PartCountAll
                            };
                            state = s.Status;
                        }
                    }
                    else if (parts_made == 0)
                    {
                        if ((int)(s.DataTime - CounterLastChanged).TotalSeconds > 2 * s.CycleTime) // plateau
                        {
                            blob.HasStep = true;
                            blob.endTime = lastDateTime;
                            blob.EndQuantity = s.PartCountAll;
                            blob.duration = blob.endTime - blob.startTime;
                            results.Add(blob);
                            blob = new PerformanceBlob()
                            {
                                HasStep = false,
                                state = s.Status,
                                startTime = lastDateTime,
                                CycleTime = s.CycleTime,
                                StartQuantity = s.PartCountAll
                            };
                            state = s.Status;
                        }
                    }
                }

                if (state != s.Status) // change of status flag from Sentry
                {
                    blob.endTime = lastDateTime;
                    blob.EndQuantity = s.PartCountAll;
                    blob.duration = blob.endTime - blob.startTime;
                    results.Add(blob);
                    blob = new PerformanceBlob()
                    {
                        HasStep = false,
                        state = s.Status,
                        startTime = lastDateTime,
                        CycleTime = s.CycleTime,
                        StartQuantity = s.PartCountAll
                    };
                    state = s.Status;
                }

                blob.EndQuantity = s.PartCountAll;
                lastDateTime = s.DataTime;
            }

            return results;
            //return DebouncePerformanceBlobs(results);
        }

        private static List<PerformanceBlob> DebouncePerformanceBlobs(List<PerformanceBlob> pbs)
        {
            List<PerformanceBlob> debouncedBlobs = new List<PerformanceBlob>();

            PerformanceBlob absorber = new PerformanceBlob();
            TimeSpan BounceLimit = TimeSpan.FromMinutes(5);

            foreach (PerformanceBlob b in pbs)
            {
                if (absorber.state == null)
                {
                    absorber = b;
                    continue;
                }

                if ((b.duration < BounceLimit || b.state == absorber.state) && !b.HasStep)
                {
                    absorber.duration += b.duration;
                    absorber.endTime = b.endTime;
                }
                else
                {
                    debouncedBlobs.Add(absorber);
                    absorber = b;
                }

            }

            return debouncedBlobs;
        }

        private class PerformanceBlob
        {
            public string state { get; set; }
            public TimeSpan duration { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
            public int QuantityMade { get; set; }
            public int StartQuantity { get; set; }
            public int EndQuantity { get; set; }
            public int CycleTime { get; set; }
            public bool HasStep { get; set; }
        }

        private class PerformanceByMachine
        {
            public string MachineID { get; set; }
            public string MachineMake { get; set; }
            public string MachineModel { get; set; }
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
            public List<PerformanceBlob> performance { get; set; }
        }

        private class Performace
        {
            public List<PerformanceByMachine> summary { get; set; }
        }
    }
}
