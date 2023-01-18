using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Reporting.Internal;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Model.Analytics
{
    public class KpiReport
    {
        public KpiReport(DateTime start, int daysSpan)
        {
            if (daysSpan <= 0)
            {
                throw new Exception("daysSpan must be greater than zero.");
            }

            DateTime reportStartDate = start.Date.AddHours(6);

            Document pdfReport = GetReport(reportStartDate, daysSpan);

            ExportPdf(pdfReport, @"C:\Users\x.iafrate\Desktop\kpi.pdf");
        }

        private Document GetReport(DateTime start, int span)
        {
            Document doc = new();
            CustomStyles.Define(doc);

            OperatingData baseData = new(start, span);
            baseData.GetData();

            for (int i = 0; i < span; i++)
            {
                OperatingData.DayOperatingData day = baseData.Days[start.AddDays(i)];

                Section daySection = day.ToReportSection();
                doc.Add(daySection);
            }

            return doc;
        }

        public class OperatingData
        {
            public List<Lathe> Lathes = new();
            public List<ScheduleItem> ScheduleItems = new();

            public Dictionary<DateTime, DayOperatingData> Days = new();

            public DateTime FromDate;
            public int DaysSpan;

            public OperatingData(DateTime fromDate, int daysSpan)
            {
                FromDate = fromDate.Date.AddHours(6);
                DaysSpan = daysSpan;
            }

            public void GetData()
            {
                Lathes = DatabaseHelper.Read<Lathe>().Where(x => !x.OutOfService).ToList();

                List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>()
                    .Where(x => !x.IsCancelled).ToList();

                List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
                List<Lot> lots = DatabaseHelper.Read<Lot>().ToList();

                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].OrderItems = items.Where(x => x.AssignedMO == orders[i].Name).ToList();
                    orders[i].Lots = lots.Where(x => x.Order == orders[i].Name).ToList();
                }

                List<MachineService> maintenance = DatabaseHelper.Read<MachineService>().ToList();

                ScheduleItems.AddRange(orders);
                ScheduleItems.AddRange(maintenance);

                for (int i = 0; i < DaysSpan; i++)
                {
                    DateTime date = FromDate.AddDays(i);

                    Days.Add(date, new DayOperatingData(date, Lathes, ScheduleItems));
                }
            }

            public OperatingEfficiencyKpi GetKpi(DateTime date, int span)
            {
                OperatingEfficiencyKpi kpi = null;

                for (int i = 0; i < Days.Count; i++)
                {
                    DateTime keyDate = Days.ElementAt(i).Key;
                    if (keyDate < date || keyDate > date.AddDays(span))
                    {
                        continue;
                    }

                    OperatingData.DayOperatingData day = Days.ElementAt(i).Value;
                    for (int j = 0; j < day.MachineSchedules.Count; j++)
                    {
                        List<ScheduleItem> items = day.MachineSchedules.ElementAt(j).Value;

                        OperatingEfficiencyKpi temporalKpi = new(date, new(24, 0, 0), items);

                        Debug.WriteLine($"i:{i} j:{j} nonRunning:{temporalKpi.NonRunningTime} {keyDate:dd/MM/yyyy HHmm}");


                        if (kpi is null)
                        {
                            kpi = temporalKpi;
                        }
                        else
                        {
                            kpi.Add(temporalKpi);
                        }
                    }
                }

                return kpi;
            }

            public class DayOperatingData
            {
                public DateTime Date;
                public List<Lathe> Lathes = new();
                public Dictionary<Lathe, List<ScheduleItem>> MachineSchedules = new();
                public Dictionary<Lathe, OperatingEfficiencyKpi> Kpis = new();

                public DayOperatingData(DateTime date, List<Lathe> machines, List<ScheduleItem> scheduleItems)
                {
                    Date = date;
                    Lathes = machines;

                    for (int i = 0; i < Lathes.Count; i++)
                    {
                        Lathe lathe = Lathes[i];
                        List<ScheduleItem> machineSchedule = WorkloadCalculationHelper.GetItemsForDayForMachine(scheduleItems, date, lathe.Id);
                        MachineSchedules.Add(lathe, machineSchedule);
                    }
                }

                public Section ToReportSection()
                {
                    Section newSection = new();
                    newSection = SetupSection(newSection);
                    newSection.AddParagraph($"{Date:ddd dd/MM/yy HHmm} to {Date.AddDays(1):ddd dd/MM/yyyy HHmm}");

                    for (int i = 0; i < Lathes.Count; i++)
                    {
                        WriteSectionForLathe(newSection, i);
                    }

                    return newSection;
                }

                private void WriteSectionForLathe(Section newSection, int i)
                {
                    Lathe lathe = Lathes[i];
                    newSection.AddParagraph($"\n\t{lathe.FullName}");
                    List<ScheduleItem> items = MachineSchedules[lathe];

                    for (int j = 0; j < items.Count; j++)
                    {
                        ScheduleItem scheduleEvent = items[j];

                        if (scheduleEvent.StartDate.Date == Date.Date)
                        {
                            newSection.AddParagraph($"\t\t[START]");
                        }
                        if (scheduleEvent is MachineService maint)
                        {
                            newSection.AddParagraph($"\t\tMaintenance Event: {maint.Name} @ {maint.StartDate:dd/MM HH:mm} to {maint.EndsAt():dd/MM HH:mm}");
                        }
                        else if (scheduleEvent is LatheManufactureOrder order)
                        {
                            newSection.AddParagraph($"\t\t{(order.IsResearch ? "R&D" : "Order")}: {order.Name} @ {order.StartDate:dd/MM HH:mm} to {order.AnticipatedEndDate():dd/MM HH:mm}");
                        }
                    }

                    if (items.Count == 0)
                    {
                        newSection.AddParagraph($"\t\tNo items today");
                    }

                    OperatingEfficiencyKpi kpi = new(Date, new(24, 0, 0), items);
                    Kpis.Add(lathe, kpi);


                    newSection.AddParagraph($"\n\t\t{kpi.AvailableTime.TotalHours:0.00} Available");
                    newSection.AddParagraph($"\t\t{kpi.UnscheduledTime.TotalHours:0.00} Unscheduled");
                    newSection.AddParagraph($"\t\t{kpi.PlannedRunTime.TotalHours:0.00} Planned Production\n");
                    newSection.AddParagraph($"\t\t{kpi.AccountedFor.TotalHours:0.00} Accounted For");

                    //newSection.AddParagraph($"OOE");

                    //newSection.AddParagraph($"\t{kpi.DevelopmentTime.TotalHours:0.00} Research & Development");
                    //newSection.AddParagraph($"\t{kpi.PlannedMaintenanceTime.TotalHours:0.00} Planned Maintenance");
                    //newSection.AddParagraph($"\t{kpi.BudgetedSettingTime.TotalHours:0.00} Budgeted Setting");


                    //newSection.AddParagraph($"OEE");

                    //newSection.AddParagraph($"\t{kpi.BreakdownTime.TotalHours:0.00} Breakdown");
                    newSection.AddParagraph($"\t\t{kpi.NonRunningTime.TotalHours:0.00} Not Running");
                    newSection.AddParagraph($"\t\t{kpi.PerformanceDeltaTime.TotalHours:0.00} Cycle Time difference [-ve = time saved]");
                    //newSection.AddParagraph($"\t{kpi.QualityLossTime.TotalHours:0.00} Quality Loss");


                    //newSection.AddParagraph($"\n\t{kpi.GetOperationsEfficiency() * 100:0.00}% Operations Efficiency");
                    newSection.AddParagraph($"\n\t{kpi.GetSchedulingEfficiency() * 100:0.00}% Scheduling Efficiency");
                    //newSection.AddParagraph($"\t{kpi.GetEquipmentEfficiency() * 100:0.00}% Equipment Efficiency");


                }
            }
        }

        public static Section SetupSection(Section section)
        {
            section.PageSetup.PageFormat = PageFormat.A4;

            section.PageSetup.LeftMargin = Size.LeftRightPageMargin;
            section.PageSetup.TopMargin = Size.TopBottomPageMargin;
            section.PageSetup.RightMargin = Size.LeftRightPageMargin;
            section.PageSetup.BottomMargin = Size.TopBottomPageMargin;

            section.PageSetup.HeaderDistance = Size.HeaderFooterMargin;
            section.PageSetup.FooterDistance = Size.HeaderFooterMargin;

            return section;
        }

        private void ExportPdf(Document report, string path)
        {
            PdfDocumentRenderer pdfRenderer = new();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            pdfRenderer.Document = report;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(path);
        }
    }
}
