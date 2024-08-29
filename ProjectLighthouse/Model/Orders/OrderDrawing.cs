using SQLite;
using System;

namespace ProjectLighthouse.Model.Orders
{
    public class OrderDrawing
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int DrawingId { get; set; }

        internal bool IsUpdated(OrderDrawing otherDrawingReference)
        {
            if (otherDrawingReference.Id != Id)
            {
                throw new InvalidOperationException($"Cannot compare drawing reference {Id} with record {otherDrawingReference.Id}");
            }

            if (OrderId != otherDrawingReference.OrderId)
            {
                return true;
            }

            if (DrawingId != otherDrawingReference.DrawingId)
            {
                return true;
            }

            return false;
        }
    }
}
