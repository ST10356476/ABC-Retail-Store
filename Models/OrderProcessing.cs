namespace ABC_Retail.Models
{
    public class OrderProcessinG
    {
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } // e.g., "Processing", "Completed", etc.
        public DateTime OrderDate { get; set; }
    }

}
