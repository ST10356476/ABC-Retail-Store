using Azure;
using Azure.Data.Tables;
using ABC_Retail.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABC_Retail.Controllers
{
    public class CustomerProfilesController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public CustomerProfilesController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        // GET: CustomerProfiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CustomerProfiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile model)
        {
            if (ModelState.IsValid)
            {
                await _azureStorageService.CreateCustomerProfileAsync(model.RowKey, model.Name, model.Email, model.Address);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: CustomerProfiles/Index
        public async Task<IActionResult> Index()
        {
            var tableClient = _azureStorageService.GetTableClient("CustomerProfiles");
            var query = tableClient.QueryAsync<CustomerProfile>(filter: $"PartitionKey eq 'CustomerProfile'");
            var customerProfiles = new List<CustomerProfile>();

            await foreach (var customerProfile in query)
            {
                customerProfiles.Add(customerProfile);
            }

            return View(customerProfiles);
        }

        // GET: CustomerProfiles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var tableClient = _azureStorageService.GetTableClient("CustomerProfiles");
            var response = await tableClient.GetEntityAsync<CustomerProfile>("CustomerProfile", id);

            // If the entity does not exist, return NotFound
            if (response == null || response.Value == null)
            {
                return NotFound();
            }

            // Pass the retrieved entity to the view
            var customerProfile = response.Value;
            return View(customerProfile);
        }

        // POST: CustomerProfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomerProfile model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tableClient = _azureStorageService.GetTableClient("CustomerProfiles");

                    // Retrieve the existing entity to get the ETag
                    var response = await tableClient.GetEntityAsync<CustomerProfile>("CustomerProfile", id);

                    // If the entity does not exist, return NotFound
                    if (response == null || response.Value == null)
                    {
                        return NotFound();
                    }

                    // Set the ETag on the model
                    model.ETag = response.Value.ETag;

                    // Update the entity
                    await tableClient.UpdateEntityAsync(model, model.ETag, TableUpdateMode.Replace);
                    return RedirectToAction("Index");
                }
                catch (RequestFailedException ex) when (ex.Status == 412)
                {
                    // Handle the precondition failed error (ETag mismatch)
                    ModelState.AddModelError("", "The entity has been updated by another user. Please reload and try again.");
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    ModelState.AddModelError("", "An error occurred while updating the entity. Please try again.");
                }
            }
            return View(model);
        }

        // GET: CustomerProfiles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var tableClient = _azureStorageService.GetTableClient("CustomerProfiles");
            var response = await tableClient.GetEntityAsync<CustomerProfile>("CustomerProfile", id);

            // If the entity does not exist, return NotFound
            if (response == null || response.Value == null)
            {
                return NotFound();
            }

            // Pass the retrieved entity to the view
            var customerProfile = response.Value;
            return View(customerProfile);
        }

        // POST: CustomerProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tableClient = _azureStorageService.GetTableClient("CustomerProfiles");

            try
            {
                // Assuming 'PartitionKey' is 'CustomerProfile' and 'RowKey' is the id
                await tableClient.DeleteEntityAsync("CustomerProfile", id, ETag.All);
                return RedirectToAction("Index");
            }
            catch (RequestFailedException ex)
            {
                // Handle errors (e.g., entity not found)
                ModelState.AddModelError("", "An error occurred while deleting the entity. Please try again.");
                return View("Delete");
            }
        }
    }
}
