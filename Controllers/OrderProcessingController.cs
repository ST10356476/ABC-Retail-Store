using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class OrderProcessingController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public OrderProcessingController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        // GET: OrderProcessing/ProcessOrder
        public IActionResult ProcessOrder()
        {
            return View();
        }

        // POST: OrderProcessing/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                await _azureStorageService.SendMessageToQueueAsync("orderprocessingqueue", $"Processing order {orderId}");
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: OrderProcessing/Index
        public IActionResult Index()
        {
            // Implement a view to list orders or queue messages if needed
            return View();
        }
    }
}
