using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class MachinePerformanceHelper
    {
        public static List<MachineOperatingBlock> SplitBlocksIntoDays(List<MachineOperatingBlock> data)
        {
            List<MachineOperatingBlock> cleanedData = new();

            foreach (MachineOperatingBlock record in data)
            {
                cleanedData.AddRange(SplitBlockIntoDays(record));
            }

            return cleanedData;
        }

        private static List<MachineOperatingBlock> SplitBlockIntoDays(MachineOperatingBlock block)
        {
            List<MachineOperatingBlock> result = new();

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

                result.Add(newBlock);
            }

            return result;
        }
    }
}
