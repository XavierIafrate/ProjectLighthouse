using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    CycleTime = block.CycleTime,
                    ErrorMessages = block.ErrorMessages,
                    Messages = block.Messages,
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

        public static List<MachineOperatingBlock> Convolute(List<MachineOperatingBlock> input, int resolutionMinutes)
        {
            List<MachineOperatingBlock> result = new();

            input = input.Where(x => x.SecondsElapsed >= resolutionMinutes * 60).OrderBy(x => x.StateEntered).ToList();
            if (input.Count <= 1)
            {
                return input;
            }

            int x = 0;
            string lastState = input[x].State;
            
            for (int i = 1; i < input.Count; i++)
            {
                MachineOperatingBlock curr = input[i];
                MachineOperatingBlock first = input[x];
                if (curr.State != lastState)
                {
                    MachineOperatingBlock newBlock = new() {
                        MachineID = curr.MachineID,
                        MachineName = curr.MachineName,
                        StateEntered = first.StateEntered,
                        StateLeft = input[i - 1].StateLeft,
                        State = first.State,
                        SecondsElapsed = (input[i - 1].StateLeft - first.StateEntered).TotalSeconds,
                        CycleTime=first.CycleTime,
                        Messages = first.Messages,
                        ErrorMessages = first.ErrorMessages,
                    };
                    result.Add(newBlock);
                    x = i;
                    lastState = input[x].State;
                }

                if (i == input.Count - 1)
                {
                    result.Add(input[i]);
                }
            }

            return result;
        }

        public static List<MachineOperatingBlock> Backfill(List<MachineOperatingBlock> blocks, int? clampHour = null)
        {
            List<MachineOperatingBlock> results = new();

            if (blocks == null || blocks.Count == 0)
            {
                return results;
            }

            if (clampHour != null)
            {
                MachineOperatingBlock firstBlock = blocks.First();
                DateTime firstBlockStarts = firstBlock.StateEntered;

                if (firstBlockStarts.Hour != clampHour && firstBlockStarts.Minute != 0 && firstBlockStarts.Second != 0)
                {
                    results.Add(new()
                    {
                        StateEntered = firstBlock.StateEntered.Date.AddHours((double)clampHour),
                        StateLeft = firstBlock.StateEntered,
                        State = "Unknown",
                        MachineID = firstBlock.MachineID,
                        MachineName = firstBlock.MachineName,
                        SecondsElapsed = (firstBlock.StateEntered - firstBlock.StateEntered.Date.AddHours((double)clampHour)).TotalSeconds
                    });
                }
            }
            if (blocks.Count != 1)
            {
                for (int i = 0; i < blocks.Count - 1; i++)
                {
                    results.Add(blocks[i]);

                    if (Math.Abs((blocks[i].StateLeft - blocks[i + 1].StateEntered).TotalSeconds) <= 1)
                    {
                        continue;
                    }

                    results.Add(new()
                    {
                        StateEntered = blocks[i].StateLeft,
                        StateLeft = blocks[i + 1].StateEntered.AddSeconds(-1),
                        State = "Unknown",
                        MachineID = blocks[i].MachineID,
                        MachineName = blocks[i].MachineName,
                        SecondsElapsed = (blocks[i + 1].StateEntered - blocks[i].StateLeft.AddSeconds(1)).TotalSeconds,
                    });
                }

                results.Add(blocks.Last());
            }
            else
            {
                results.Add(blocks.First());
            }

            if (clampHour != null)
            {
                MachineOperatingBlock lastBlock = results.Last();
                if (lastBlock.StateLeft.AddHours(1).Hour != clampHour && lastBlock.StateLeft.Minute != 59 && lastBlock.StateLeft.Second != 59)
                {
                    results.Add(new()
                    {
                        StateEntered = lastBlock.StateLeft.AddSeconds(1),
                        StateLeft = lastBlock.StateLeft.Hour < clampHour ? lastBlock.StateLeft.Date.AddHours((double)clampHour).AddSeconds(-1) : lastBlock.StateLeft.Date.AddDays(1).AddHours((double)clampHour).AddSeconds(-1),
                        State = "Unknown",
                        MachineID = lastBlock.MachineID,
                        MachineName = lastBlock.MachineName,
                        SecondsElapsed = ((lastBlock.StateLeft.Hour < clampHour ? lastBlock.StateLeft.Date.AddHours((double)clampHour).AddSeconds(-1) : lastBlock.StateLeft.Date.AddDays(1).AddHours((double)clampHour).AddSeconds(-1)) - lastBlock.StateLeft.AddSeconds(1)).TotalSeconds
                    });
                }
            }

            return results;
        }
    }
}
