using ProjectLighthouse.Model;
using System.Collections.Generic;

namespace ProjectLighthouse.ViewModel
{
    public static class SchedulingEngine
    {
        public static int RateOrderCompatibility(LatheManufactureOrder order1, LatheManufactureOrder order2)
        {
            int score = 0;

            score += order1.BarID == order2.BarID ? 1 : 0; // change to bar diameter
            // bar material 
            // tooling group
            // 

            return score;
        }

        public static void AutoSchedule()
        {
            // do cool stuff
        }

        public static List<ScheduleWarning> DetectProblems()
        {
            // steal from schedule view model
            return new();
        }
    }
}
