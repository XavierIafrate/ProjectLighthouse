using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public interface ISchedulableObject : IHasFirebaseId
    {
        int Id { get; set; }
        string Name { get; set; }
        int TimeToComplete { get; set; }
        DateTime StartDate { get; set; }
        string AllocatedMachine { get; set; }
    }
}
