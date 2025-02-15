﻿using System;

namespace ProjectLighthouse.Model.Scheduling
{
    public interface ISchedulableObject
    {
        int Id { get; set; }
        string Name { get; set; }
        int TimeToComplete { get; set; }
        DateTime StartDate { get; set; }
        string AllocatedMachine { get; set; }
    }
}
