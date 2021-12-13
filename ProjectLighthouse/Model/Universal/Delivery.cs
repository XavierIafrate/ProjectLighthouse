namespace ProjectLighthouse.Model.Universal
{
    public class Delivery
    {
        public DeliveryNote Header { get; set; }
        public DeliveryItem[] Items { get; set; }
    }
}
