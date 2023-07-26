using ProjectLighthouse.Model.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public static class MachinePerformanceHelper
    {
        public static List<MachineOperatingBlock> Denoise(this List<MachineOperatingBlock> blocks, DateTime start, DateTime end, int secondsThreshold)
        {
            blocks = FillGaps(blocks, start, end);
            blocks = AmalgamateRapidChanges(blocks, secondsThreshold: secondsThreshold);
            blocks = RemoveSmallBlocks(blocks, secondsThreshold: secondsThreshold);
            blocks = RemoveConsecutiveStatusBlocks(blocks);

            return blocks;
        }

        public static List<MachineOperatingBlock> FillGaps(List<MachineOperatingBlock> input, DateTime start, DateTime end)
        {
            List<MachineOperatingBlock> result = new();

            if (input.Count == 0)
            {
                result.Add(new() { State = "No Data", StateEntered = start, StateLeft = end, SecondsElapsed = (int)(end - start).TotalSeconds });
                return result;
            }

            if (input[0].StateEntered > start)
            {
                result.Add(new() { State = "No Data", StateEntered = start, StateLeft = input[0].StateEntered, SecondsElapsed = (int)(input[0].StateEntered - start).TotalSeconds });
            }
            else if (input[0].StateEntered < start)
            {
                input[0].StateEntered = start;
                input[0].SecondsElapsed = (int)(input[0].StateLeft - input[0].StateEntered).TotalSeconds;
            }

            for (int i = 0; i < input.Count - 1; i++)
            {
                result.Add(input[i]);
                if (input[i + 1].StateEntered > input[i].StateLeft.AddSeconds(1))
                {
                    result.Add(new()
                    {
                        State = "No Data",
                        StateEntered = input[i].StateLeft,
                        StateLeft = input[i + 1].StateEntered,
                        SecondsElapsed = (int)(input[i + 1].StateEntered - input[i].StateLeft).TotalSeconds
                    });
                }

            }

            if (input[^1].StateLeft.AddSeconds(1) < end)
            {
                result.Add(input[^1]);
                result.Add(new() { State = "No Data", StateEntered = input[^1].StateLeft, StateLeft = end, SecondsElapsed = (int)(end - input[^1].StateLeft).TotalSeconds });
            }
            else if (input[^1].StateLeft > end)
            {
                input[^1].StateLeft = end;
                input[^1].SecondsElapsed = (int)(input[^1].StateLeft - input[^1].StateEntered).TotalSeconds;
                result.Add(input[^1]);

            }

            return result;
        }

        public static List<MachineOperatingBlock> AmalgamateRapidChanges(List<MachineOperatingBlock> input, int secondsThreshold)
        {
            if (input.Count == 0) return input;
            if (input.Count == 1) return input;

            List<MachineOperatingBlock> result = new();

            string[] statuses = input.Select(x => x.State).ToArray();
            int i = 0;
            while (i < input.Count)
            {
                if (input[i].SecondsElapsed > secondsThreshold)
                {
                    result.Add(input[i]);
                    i++;
                    continue;
                }

                int j = i + 1;
                while (j < input.Count && secondsThreshold > input[j].SecondsElapsed)
                {
                    j++;
                }

                input[i].State = "Setting";
                input[i].StateLeft = input[j - 1].StateLeft;
                input[i].SecondsElapsed = (int)(input[i].StateLeft - input[i].StateEntered).TotalSeconds;

                result.Add(input[i]);

                i = j;
            }


            return result;
        }

        public static List<MachineOperatingBlock> RemoveSmallBlocks(List<MachineOperatingBlock> input, int secondsThreshold)
        {
            if (input.Count < 2) return input;

            List<MachineOperatingBlock> result = new();

            for (int i = input.Count - 1; i >= 1; i--)
            {
                if (input[i - 1].SecondsElapsed < secondsThreshold && input[i].SecondsElapsed > secondsThreshold)
                {
                    input[i].StateEntered = input[i - 1].StateEntered;
                    input[i].SecondsElapsed = (int)(input[i].StateLeft - input[i].StateEntered).TotalSeconds;

                    result.Add(input[i]);

                    i--;
                    continue;
                }

                result.Add(input[i]);
            }

            result.Add(input[0]);

            result.Reverse();

            return result;
        }

        public static List<MachineOperatingBlock> RemoveConsecutiveStatusBlocks(List<MachineOperatingBlock> input)
        {
            if (input.Count == 0) return input;
            if (input.Count == 1) return input;

            List<MachineOperatingBlock> result = new();

            string[] statuses = input.Select(x => x.State).ToArray();
            int i = 0;
            while (i < input.Count)
            {
                int j = i + 1;
                while (j < input.Count && input[i].State == input[j].State)
                {
                    j++;
                }

                input[i].StateLeft = input[j - 1].StateLeft;
                input[i].SecondsElapsed = (int)(input[i].StateLeft - input[i].StateEntered).TotalSeconds;

                result.Add(input[i]);

                i = j;
            }

            return result;
        }

        public static List<MachineOperatingBlock> Slice(this List<MachineOperatingBlock> list, DateTime dateTime)
        {
            List<MachineOperatingBlock> result = new();

            for (int i = 0; i < list.Count; i++)
            {
                MachineOperatingBlock block = list[i];

                if (block.StateEntered > dateTime || block.StateLeft < dateTime)
                {
                    result.Add(block);
                    continue;
                }

                MachineOperatingBlock firstBlock = (MachineOperatingBlock)block.Clone();
                firstBlock.StateLeft = dateTime;
                firstBlock.SecondsElapsed = (int)(firstBlock.StateLeft - firstBlock.StateEntered).TotalSeconds;

                MachineOperatingBlock secondBlock = (MachineOperatingBlock)block.Clone();
                secondBlock.StateEntered = dateTime;
                secondBlock.SecondsElapsed = (int)(secondBlock.StateLeft - secondBlock.StateEntered).TotalSeconds;

                result.Add(firstBlock);
                result.Add(secondBlock);
            }

            return result;
        }
    }
}
