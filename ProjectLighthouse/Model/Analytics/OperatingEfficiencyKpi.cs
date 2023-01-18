
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model.Analytics
{
    public class OperatingEfficiencyKpi
    {
        public DateTime StartDate;
        public TimeSpan DaysSpan;

        public TimeSpan AvailableTime;
        public TimeSpan UnscheduledTime;
        public TimeSpan DevelopmentTime;
        public TimeSpan PlannedMaintenanceTime;
        public TimeSpan BudgetedSettingTime;
        public TimeSpan BreakdownTime;
        public TimeSpan NonRunningTime;
        public TimeSpan PerformanceDeltaTime; // Positive or Negative
        public TimeSpan QualityLossTime;

        public TimeSpan AccountedFor;

        public TimeSpan PlannedRunTime => AvailableTime - UnscheduledTime - DevelopmentTime - PlannedMaintenanceTime - BudgetedSettingTime;

        public OperatingEfficiencyKpi(DateTime startDate, TimeSpan timeSpan, List<ScheduleItem> itemsWithinSpan)
        {
            StartDate = startDate;
            DaysSpan = timeSpan;    

            CalculateScheduledTime(startDate, timeSpan, itemsWithinSpan);
            //CalculateEquipmentTime()
        }

        private void CalculateScheduledTime(DateTime startDate, TimeSpan timeSpan, List<ScheduleItem> itemsWithinSpan)
        {
            AvailableTime = timeSpan;
            UnscheduledTime = timeSpan;

            itemsWithinSpan = itemsWithinSpan.OrderByDescending(x => x.StartDate).ToList();

            DateTime currentDate = startDate + timeSpan; // Start at the end and work backwards

            for (int i = 0; i < itemsWithinSpan.Count; i++)
            {
                ScheduleItem item = itemsWithinSpan[i];

                // Change currentDate if MachineService?
                (DateTime itemStartsAt, DateTime itemEndsAt, TimeSpan remainderAfter) = GetStartAndEnd(item, startDate, currentDate);
                
                TimeSpan timeForItem = itemEndsAt - itemStartsAt;
                UnscheduledTime -= timeForItem;

                AccountedFor += timeForItem;

                if (item is LatheManufactureOrder order)
                {
                    CalculateTimeForOrder(order, startDate, timeSpan, timeForItem, remainderAfter);
                }
                else if (item is MachineService service)
                {
                    PlannedMaintenanceTime += timeForItem; // TODO override order running and take off time
                }

                currentDate = itemStartsAt;
            }
        }

        private void CalculateTimeForOrder(LatheManufactureOrder order, DateTime startDate, TimeSpan timeSpan, TimeSpan timeForItem, TimeSpan remainderAfter)
        {
            TimeSpan settingTime = TimeSpan.FromHours(6);

            if (order.StartDate.Date == startDate.Date)
            {
                BudgetedSettingTime += settingTime;
                //AccountedFor += settingTime; // Check
                UnscheduledTime -= settingTime;
            }

            if (order.IsResearch)
            {
                DevelopmentTime += timeForItem + remainderAfter + settingTime;
                
                if (order.StartDate.Date == startDate.Date)
                {
                    BudgetedSettingTime -= settingTime;
                }

                UnscheduledTime -= remainderAfter; // Allow downtime to next order
                return;
            }

            double weightedCycleTime = order.CalculateWeightedCycleTime();

            List<Lot> producedInTimeSpan = order.Lots
                .Where(x => 
                    x.DateProduced > startDate && 
                    x.DateProduced < startDate + timeSpan)
                .ToList();

            int rejects = producedInTimeSpan.Where(x => x.IsReject).Sum(x => x.Quantity);
            int total = producedInTimeSpan.Sum(x => x.Quantity);

            // TODO use actual cycle times not weighted
            int theoreticalTotal = (int)timeForItem.TotalSeconds / (int)weightedCycleTime;

            NonRunningTime = TimeSpan.FromSeconds((theoreticalTotal - total) * weightedCycleTime);

            QualityLossTime = TimeSpan.FromSeconds(rejects * weightedCycleTime);

            if (order.TargetCycleTime <= 0 || order.TargetCycleTimeEstimated)
            {
                PerformanceDeltaTime = TimeSpan.FromSeconds(0);
            }
            else
            {
                PerformanceDeltaTime = ((weightedCycleTime - order.TargetCycleTime) / order.TargetCycleTime) * (timeForItem - NonRunningTime);
            }
        }

        private (DateTime, DateTime, TimeSpan) GetStartAndEnd(ScheduleItem item, DateTime min, DateTime max)
        {
            DateTime start;
            DateTime stop;
            TimeSpan remainder;

            if (item is LatheManufactureOrder order)
            {
                start = order.StartDate;
                stop = order.EndsAt();
            }
            else if (item is MachineService service)
            {
                start = service.StartDate;
                stop = service.EndsAt();
            }
            else
            {
                throw new Exception("Invalid type");
            }

            start = start < min
                ? min
                : start;

            stop = stop > max
                ? max
                : stop;

            remainder = max - stop;

            return new(start, stop, remainder);
        }

        public double GetOperationsEfficiency()
        {
            TimeSpan availableTime = AvailableTime;
            TimeSpan losses = UnscheduledTime
                + DevelopmentTime
                + PlannedMaintenanceTime
                + BudgetedSettingTime
                + BreakdownTime
                + NonRunningTime
                + PerformanceDeltaTime
                + QualityLossTime;

            double effectiveness = (availableTime - losses) / availableTime;
            return effectiveness;
        }

        public double GetEquipmentEfficiency()
        {
            TimeSpan plannedRunTime = PlannedRunTime;
            TimeSpan losses = BreakdownTime + NonRunningTime + PerformanceDeltaTime + QualityLossTime;

            double effectiveness = (plannedRunTime - losses) / plannedRunTime;

            return effectiveness;
        }

        public double GetSchedulingEfficiency()
        {
            double effectiveness = (AvailableTime - PlannedMaintenanceTime - DevelopmentTime - UnscheduledTime) /  (AvailableTime - PlannedMaintenanceTime - DevelopmentTime);
            return effectiveness;
        }

        public void Add(OperatingEfficiencyKpi kpi)
        {
            AvailableTime += kpi.AvailableTime;
            UnscheduledTime += kpi.UnscheduledTime;
            DevelopmentTime += kpi.DevelopmentTime;
            PlannedMaintenanceTime += kpi.PlannedMaintenanceTime;
            BudgetedSettingTime += kpi.BudgetedSettingTime;
            BreakdownTime += kpi.BreakdownTime;
            NonRunningTime += kpi.NonRunningTime;
            PerformanceDeltaTime += kpi.PerformanceDeltaTime;
            QualityLossTime += kpi.QualityLossTime;

            AccountedFor += kpi.AccountedFor;
        }

        public void Normalise(int multiplier = 1)
        {
            double coefficient = multiplier / AvailableTime.TotalHours;

            AccountedFor *= coefficient;
            QualityLossTime *= coefficient;
            PerformanceDeltaTime *= coefficient;
            NonRunningTime *= coefficient;
            BreakdownTime *= coefficient;
            BudgetedSettingTime *= coefficient;
            PlannedMaintenanceTime *= coefficient;
            DevelopmentTime *= coefficient;
            UnscheduledTime *= coefficient;
            AvailableTime *= coefficient;
        }
    }
}