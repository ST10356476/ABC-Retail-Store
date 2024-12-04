using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using ABC_Retail.Models;

namespace ABC_Retail
{
    public class AzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ShareServiceClient _shareServiceClient;

        public AzureStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorageConnectionString");

            _blobServiceClient = new BlobServiceClient(connectionString);
            _tableServiceClient = new TableServiceClient(connectionString);
            _queueServiceClient = new QueueServiceClient(connectionString);
            _shareServiceClient = new ShareServiceClient(connectionString);
        }

        #region Azure Table Operations for CustomerProfiles
        public TableClient GetTableClient(string tableName)
        {
            return _tableServiceClient.GetTableClient(tableName);
        }

        public async Task CreateCustomerProfileAsync(string customerId, string name, string email, string address)
        {
            var tableClient = GetTableClient("CustomerProfiles");
            await tableClient.CreateIfNotExistsAsync();

            var customerProfile = new TableEntity("CustomerProfile", customerId)
            {
                {"Name", name},
                {"Email", email},
                {"Address", address}
            };

            await tableClient.AddEntityAsync(customerProfile);
        }

        public async Task<List<TableEntity>> GetAllCustomerProfilesAsync()
        {
            var tableClient = GetTableClient("CustomerProfiles");
            var query = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'CustomerProfile'");
            var customerProfiles = new List<TableEntity>();

            await foreach (var entity in query)
            {
                customerProfiles.Add(entity);
            }

            return customerProfiles;
        }

        public async Task<TableEntity> GetCustomerProfileAsync(string id)
        {
            var tableClient = GetTableClient("CustomerProfiles");
            var response = await tableClient.GetEntityAsync<TableEntity>("CustomerProfile", id);

            return response.Value;
        }

        public async Task UpdateCustomerProfileAsync(string customerId, TableEntity updatedEntity)
        {
            var tableClient = GetTableClient("CustomerProfiles");
            await tableClient.UpdateEntityAsync(updatedEntity, updatedEntity.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerProfileAsync(string id)
        {
            var tableClient = GetTableClient("CustomerProfiles");
            await tableClient.DeleteEntityAsync("CustomerProfile", id, ETag.All);
        }
        #endregion

        #region Azure Table Operations for ProductInfo
        public async Task CreateProductInfoAsync(ProductInfo productInfo)
        {
            var tableClient = GetTableClient("ProductInfo");
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(productInfo.PartitionKey, productInfo.RowKey)
            {
                {"ProductName", productInfo.ProductName},
                {"Category", productInfo.Category},
                {"Price", productInfo.Price}
            };

            await tableClient.AddEntityAsync(entity);
        }

        public async Task<ProductInfo> GetProductInfoByIdAsync(string rowKey)
        {
            var tableClient = GetTableClient("ProductInfo");

            try
            {
                // Ensure the table exists
                await tableClient.CreateIfNotExistsAsync();

                // Retrieve the entity
                var response = await tableClient.GetEntityAsync<TableEntity>("ProductInfo", rowKey);
                var entity = response.Value;

                // Map the TableEntity to ProductInfo
                return new ProductInfo
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    ProductName = entity.GetString("ProductName"),
                    Category = entity.GetString("Category"),
                    Price = entity.GetString("Price"),
                    Timestamp = entity.Timestamp,
                    ETag = entity.ETag
                };
            }
            catch (RequestFailedException ex)
            {
                // Handle exceptions as needed
                Console.WriteLine($"RequestFailedException: {ex.Message}");
                return null;
            }
        }
        public async Task<List<ProductInfo>> GetAllProductInfosAsync()
        {
            var tableClient = GetTableClient("ProductInfo");
            var query = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'ProductInfo'");
            var productInfos = new List<ProductInfo>();

            await foreach (var entity in query)
            {
                var productInfo = new ProductInfo
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    ProductName = entity.GetString("ProductName"),
                    Category = entity.GetString("Category"),
                    Price = entity.GetString("Price"),
                    Timestamp = entity.Timestamp,
                    ETag = entity.ETag
                };
                productInfos.Add(productInfo);
            }

            return productInfos;
        }

        public async Task<ProductInfo> GetProductInfoAsync(string id)
        {
            var tableClient = GetTableClient("ProductInfo");
            var response = await tableClient.GetEntityAsync<TableEntity>("ProductInfo", id);

            if (response == null)
            {
                return null;
            }

            var entity = response.Value;
            return new ProductInfo
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ProductName = entity.GetString("ProductName"),
                Category = entity.GetString("Category"),
                Price = entity.GetString("Price"),
                Timestamp = entity.Timestamp,
                ETag = entity.ETag
            };
        }

        public async Task UpdateProductInfoAsync(ProductInfo productInfo)
        {
            var tableClient = GetTableClient("ProductInfo");

            try
            {
                if (string.IsNullOrWhiteSpace(productInfo.ETag.ToString()))
                {
                    throw new ArgumentException("ETag cannot be null or empty.", nameof(productInfo.ETag));
                }

                var entity = new TableEntity(productInfo.PartitionKey, productInfo.RowKey)
        {
            { "ProductName", productInfo.ProductName },
            { "Category", productInfo.Category },
            { "Price", productInfo.Price }
        };

                await tableClient.UpdateEntityAsync(entity, productInfo.ETag, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                // Handle specific Azure exceptions here
                Console.WriteLine($"RequestFailedException: {ex.Message}");
                throw;
            }
        }


        public async Task DeleteProductInfoAsync(string id)
        {
            var tableClient = GetTableClient("ProductInfo");
            await tableClient.DeleteEntityAsync("ProductInfo", id, ETag.All);
        }
        #endregion

        #region Azure Blob Operations
        public async Task UploadProductImageAsync(string blobContainerName, string blobName, Stream fileStream)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, true);
        }
        #endregion

        #region Azure Queue Operations
        public async Task SendMessageToQueueAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();

            await queueClient.SendMessageAsync(message);
        }
        #endregion

        #region Azure File Operations
        public async Task UploadFileAsync(string shareName, string directoryName, string fileName, Stream fileStream)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);
        }
        #endregion
    }
}
