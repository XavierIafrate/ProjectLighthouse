using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class MachinePerformanceHelper
    {
        public static List<MachineOperatingBlock> SplitBlocksIntoDays(List<MachineOperatingBlock> data, int hour = 0, int minute = 0)
        {
            List<MachineOperatingBlock> cleanedData = new();

            foreach (MachineOperatingBlock record in data)
            {
                cleanedData.AddRange(SplitBlockIntoDays(record, hour, minute));
            }

            return cleanedData;
        }

        private static List<MachineOperatingBlock> SplitBlockIntoDays(MachineOperatingBlock block, int hour = 0, int minute = 0)
        {
            List<MachineOperatingBlock> result = new();

            block.StateEntered = block.StateEntered.AddHours(-hour).AddMinutes(-minute);
            block.StateLeft = block.StateLeft.AddHours(-hour).AddMinutes(-minute);

            DateTime blockStartDay = block.StateEntered.Date;
            DateTime blockEndDay = block.StateLeft.Date;

            int numberOfDays = Convert.ToInt32((blockEndDay - blockStartDay).TotalDays);

            DateTime newBlockStarts = block.StateEntered;

            for (int i = 0; i < numberOfDays + 1; i++)
            {
                MachineOperatingBlock newBlock = new()
                {
                    State = block.State,
                    MachineID = block.MachineID,
                    MachineName = block.MachineName,
                    StateEntered = newBlockStarts,
                };

                if (i == numberOfDays)
                {
                    newBlock.StateLeft = block.StateLeft;
                }
                else
                {

                    newBlock.StateLeft = newBlock.StateEntered.Date.AddDays(1).AddSeconds(-1);
                    newBlockStarts = newBlock.StateEntered.Date.AddDays(1);
                }

                newBlock.SecondsElapsed = (newBlock.StateLeft - newBlock.StateEntered).TotalSeconds;

                newBlock.StateEntered = newBlock.StateEntered.AddHours(hour).AddMinutes(minute);
                newBlock.StateLeft = newBlock.StateLeft.AddHours(hour).AddMinutes(minute);

                result.Add(newBlock);
            }

            return result;
        }
    }
}
