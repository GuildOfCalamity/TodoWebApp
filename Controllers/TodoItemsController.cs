using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TodoWebApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TodoWebApp.Controllers
{
    public class TodoItemsController : Controller
    {
        readonly AppDbContext _db;
        readonly ILogger<TodoItemsController> _logger;

        /// <summary>
        /// Constructor for the TodoItemsController.
        /// </summary>
        public TodoItemsController(AppDbContext db, ILogger<TodoItemsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // HttpGet: /TodoItems?sortOrder=Due or ?sortOrder=due_desc or ?sortOrder=Added or ?sortOrder=added_desc
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DueSortParm"] = sortOrder == "Due" ? "due_desc" : "Due";
            ViewData["AddedSortParm"] = sortOrder == "Added" ? "added_desc" : "Added";

            IQueryable<TodoItem> items = _db.TodoItems;

            switch (sortOrder)
            {
                case "Due":
                    items = items.OrderBy(t => t.DueDate);
                    break;
                case "due_desc":
                    items = items.OrderByDescending(t => t.DueDate);
                    break;
                case "Added":
                    items = items.OrderBy(t => t.EntryDate);
                    break;
                case "added_desc":
                    items = items.OrderByDescending(t => t.EntryDate);
                    break;
                default:
                    items = items.OrderBy(t => t.Id);
                    break;
            }

            var list = await items.AsNoTracking().ToListAsync();
            return View(list);
        }

        // HttpGet: /TodoItems
        public async Task<IActionResult> IndexOriginal()
        {
            var items = await _db.TodoItems.ToListAsync();
            return View(items);
        }

        // HttpGet: /TodoItems/Create
        public IActionResult Create() => View();

        // HttpPost: /TodoItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem item)
        {
            if (item.EntryDate is null)
            {
                item.EntryDate = DateTime.Now;
            }

            if (!ModelState.IsValid)
            {
                #region [Show modal/popup dialog if issue detected]
                // gather all error messages
                var messages = ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage);

                _logger.LogError("Create failed: {Errors}", string.Join("; ", messages));

                // set the show flag
                TempData["ShowMessagePopup"] = true;
                // pass the messages joined by <br> so we can render HTML
                TempData["PopupMessage"] = string.Join("<br/>", messages);
                // return the same view, so TempData lives through this request
                return View(item);
                #endregion
            }
            else // clear the TempData if no errors
            {
                TempData.Remove("ShowMessagePopup");
                TempData.Remove("PopupMessage");
            }

            try
            {
                _db.Add(item);
                await _db.SaveChangesAsync();
                _logger.LogInformation($"{item}");
            }
            catch (Exception ex)
            {
                // catch DB exceptions, etc.
                _logger.LogError(ex, "Exception while saving new item");
                ModelState.AddModelError("", "Unable to save changes.");
                return View(item);
            }

            return RedirectToAction(nameof(Index));
        }

        // HttpPost: /TodoItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOriginal(TodoItem item)
        {
            item.EntryDate = DateTime.Now;
            //item.Details ??= string.Empty; // ensure Details is not null

            if (!ModelState.IsValid)
                return View(item);

            _db.Add(item);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // HttpGet: /TodoItems/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            
            if (item == null) 
                return NotFound();

            //TempData["ShowMessagePopup"] = true; // set the show flag
            //TempData["PopupMessage"] = "Test message here.<br/>"; // pass the messages joined by <br> so we can render HTML

            return View(item);
        }

        // HttpPost: /TodoItems/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TodoItem item)
        {
            if (id != item.Id) 
                return BadRequest();

            if (!ModelState.IsValid) 
                return View(item);

            // Update the entry date if null
            if (item.EntryDate is null)
                item.EntryDate = DateTime.Now;

            _db.Update(item);
            
            await _db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // HttpGet: /TodoItems/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            
            if (item == null) 
                return NotFound();
            
            return View(item);
        }

        // HttpPost: /TodoItems/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            
            _db.TodoItems.Remove(item!);
            
            await _db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}