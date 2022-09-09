namespace ProjectLighthouse.Model.Reporting
{
    public class DeliveryData
    {
        public DeliveryNote Header { get; set; }
        public DeliveryItem[] Lines { get; set; }
    }
}
