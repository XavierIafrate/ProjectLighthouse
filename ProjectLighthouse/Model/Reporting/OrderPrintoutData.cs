namespace ProjectLighthouse.Model.Reporting
{
    public class OrderPrintoutData
    {
        public LatheManufactureOrder Order { get; set; }
        public LatheManufactureOrderItem[] Items { get; set; }
        public Note[] Notes { get; set; }
    }
}
