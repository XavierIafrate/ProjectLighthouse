
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Analytics
{
    public class OperatingEfficiencyKpi
    {

        public TimeSpan AvailableTime;
        public TimeSpan OperationsTime;
        public TimeSpan MaintenanceLoss;
        public TimeSpan DevelopmentLoss;
        public TimeSpan ScheduleLoss;
        public TimeSpan ChangeoverLoss;
        public TimeSpan AvailabilityLoss;
        public TimeSpan PerformanceChange;
        public TimeSpan QualityLoss;

        //public void Add(OperatingEfficiencyKpi kpi)
        //{
        //    AvailableTime += kpi.AvailableTime;
        //    UnscheduledTime += kpi.UnscheduledTime;
        //    DevelopmentTime += kpi.DevelopmentTime;
        //    PlannedMaintenanceTime += kpi.PlannedMaintenanceTime;
        //    BudgetedSettingTime += kpi.BudgetedSettingTime;
        //    BreakdownTime += kpi.BreakdownTime;
        //    NonRunningTime += kpi.NonRunningTime;
        //    PerformanceDeltaTime += kpi.PerformanceDeltaTime;
        //    QualityLossTime += kpi.QualityLossTime;

        //    AccountedFor += kpi.AccountedFor;
        //}

        public void Normalise(int multiplier = 1)
        {
            double coefficient = multiplier / AvailableTime.TotalHours;

            QualityLoss *= coefficient;
            PerformanceChange *= coefficient;
            AvailabilityLoss *= coefficient;
            ChangeoverLoss *= coefficient;
            ScheduleLoss *= coefficient;
            DevelopmentLoss *= coefficient;
            MaintenanceLoss *= coefficient;
            OperationsTime *= coefficient;
            AvailableTime *= coefficient;
        }

        public List<(string, double, double, double)> GetWaterfall()
        {
            List<(string, double, double, double)> result = new();

            TimeSpan maxTime = OperationsTime > (OperationsTime - MaintenanceLoss - DevelopmentLoss - ScheduleLoss - ChangeoverLoss - AvailabilityLoss - PerformanceChange)
                ? OperationsTime
                : (OperationsTime - MaintenanceLoss - DevelopmentLoss - ScheduleLoss - ChangeoverLoss - AvailabilityLoss - PerformanceChange);
            
            TimeSpan offset = maxTime - OperationsTime;

            result.Add(new("Total Operations Time", 0, OperationsTime / maxTime, offset / maxTime));

            result.Add(new("Planned Maintenance", 1-(MaintenanceLoss + offset)/maxTime, MaintenanceLoss / maxTime, offset / maxTime));
            offset += MaintenanceLoss;

            result.Add(new("Development", 1 - (DevelopmentLoss + offset) / maxTime, DevelopmentLoss / maxTime, offset / maxTime));
            offset += DevelopmentLoss;

            result.Add(new("Schedule Loss", 1 - (ScheduleLoss + offset) / maxTime, ScheduleLoss / maxTime, offset / maxTime));
            offset += ScheduleLoss;
            TimeSpan equipmentStart = offset;

            result.Add(new("Changeover Loss", 1 - (ChangeoverLoss + offset) / maxTime, ChangeoverLoss / maxTime, offset / maxTime));
            offset += ChangeoverLoss;

            result.Add(new("Availability Loss", 1 - (AvailabilityLoss + offset) / maxTime, AvailabilityLoss / maxTime, offset / maxTime));
            offset += AvailabilityLoss;

            if (PerformanceChange.TotalSeconds > 0)
            {
                result.Add(new("Performance Change", 1 - (PerformanceChange + offset) / maxTime, PerformanceChange / maxTime, offset / maxTime));
            }
            else
            {
                result.Add(new("Performance Change", 1 - Math.Abs(PerformanceChange / maxTime) - (offset + PerformanceChange) / maxTime, Math.Abs(PerformanceChange / maxTime), (offset + PerformanceChange) / maxTime));
            }
            offset += PerformanceChange;

            result.Add(new("Quality Loss", 1 - (QualityLoss + offset) / maxTime, QualityLoss / maxTime, offset / maxTime));
            offset += QualityLoss;


            result.Add(new("OOE", 1-offset / maxTime, offset / maxTime, 0));

            if (offset > equipmentStart) // Under 100%
            {
                result.Add(new("OEE", 1-offset / maxTime, (offset - equipmentStart) / maxTime, equipmentStart / maxTime));
            }
            else
            {
                result.Add(new("OEE", 1-offset / maxTime, 0, offset/maxTime));
            }

            return result;
        }

    }
}