using Azure;
using Azure.Data.Tables;

namespace ABC_Retail.Models
{
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "CustomerProfile";
        public string RowKey { get; set; } // This can be the CustomerId
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
