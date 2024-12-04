using Azure.Data.Tables;
using Azure;

namespace ABC_Retail.Models
{
    public class ProductInfo : ITableEntity
    {
        public string PartitionKey { get; set; } = "ProductInfo";
        public string RowKey { get; set; } // This can be the ProductId
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
