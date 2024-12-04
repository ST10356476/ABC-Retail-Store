using Microsoft.AspNetCore.Mvc;
using ABC_Retail.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABC_Retail.Controllers
{
    public class ProductInfoController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public ProductInfoController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        // GET: ProductInfo/Index
        public async Task<IActionResult> Index()
        {
            var ProductInfo = await _azureStorageService.GetAllProductInfosAsync();
            return View(ProductInfo);
        }

        // GET: ProductInfo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductInfo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInfo model)
        {
            if (ModelState.IsValid)
            {
                await _azureStorageService.CreateProductInfoAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: ProductInfo/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var productInfo = await _azureStorageService.GetProductInfoByIdAsync(id);
            if (productInfo == null)
            {
                return NotFound();
            }
            return View(productInfo);
        }

        // POST: ProductInfo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductInfo model)
        {
            if (id != model.RowKey)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _azureStorageService.UpdateProductInfoAsync(model);
                }
                catch (Exception ex)
                {
                    // Handle the exception, e.g., log it or return an error view
                    Console.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", "Unable to update the product information.");
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: ProductInfo/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var productInfo = await _azureStorageService.GetProductInfoAsync(id);
            if (productInfo == null)
            {
                return NotFound();
            }
            return View(productInfo);
        }

        // POST: ProductInfo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _azureStorageService.DeleteProductInfoAsync(id);
            return RedirectToAction("Index");
        }
    }
}
