namespace ProjectLighthouse.Model.Deliveries
{
    public class Delivery
    {
        public DeliveryNote Header { get; set; }
        public DeliveryItem[] Items { get; set; }
    }
}
