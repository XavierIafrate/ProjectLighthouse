namespace ProjectLighthouse.Model.Scheduling
{
    public class MachineService : ScheduleItem
    {
        internal MachineService Clone()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MachineService>(json);
        }
    }
}
