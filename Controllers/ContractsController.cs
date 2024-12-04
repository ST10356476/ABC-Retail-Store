using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ContractsController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public ContractsController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        // GET: Contracts/UploadContract
        public IActionResult UploadContract()
        {
            return View();
        }

        // POST: Contracts/UploadContract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    await _azureStorageService.UploadFileAsync("contracts", "documents", file.FileName, stream);
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Contracts/Index
        public IActionResult Index()
        {
            // Implement a view to list contracts if needed
            return View();
        }
    }
}
