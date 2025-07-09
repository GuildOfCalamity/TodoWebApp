// Ignore Spelling: Todo

using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TodoWebApp.Models;

namespace TodoWebApp.Controllers
{
    /// <summary>
    /// NOTE: TempData["PropName"] is part of the <see cref="Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary"/>.
    ///       ViewData["PropName"] is part of the <see cref="Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary"/>.
    /// </summary>
    public class TodoItemsController : Controller
    {
        bool _logHttpContext = false;
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

        [HttpGet] // GET: /TodoItems?sortOrder=Due or ?sortOrder=due_desc or ?sortOrder=Added or ?sortOrder=added_desc
        public async Task<IActionResult> Index(string sortOrder)
        {
            if (_logHttpContext)
            {
                #region [Basic trace data for logging]
                try
                {
                    //HttpContext.Features.ForEach();

                    Debug.WriteLine($"[DEBUG] HttpContextRequestMethod..: {HttpContext.Request.Method}");
                    Debug.WriteLine($"[DEBUG] HttpContextRequestProtocol: {HttpContext.Request.Protocol}");
                    Debug.WriteLine($"[DEBUG] HttpContextRequestHost....: {HttpContext.Request.Host}");
                    foreach (var hdr in HttpContext.Request.Headers)
                    {
                        Debug.WriteLine($"Header '{hdr.Key}'");
                        foreach (var shdr in hdr.Value)
                        {
                            Debug.WriteLine($"   ⇒ {shdr}");
                        }
                    }
                    Debug.WriteLine($"[INFO] Activity.Current.Id........: {Activity.Current?.Id} ({Activity.Current?.Kind})");

                    var connId = HttpContext.Connection.Id;
                    ViewData["StatusMessage"] = $"Connection Id: {connId}";
                    _logger.LogInformation($"Connection Id is {connId}");

                    var localIp = $"{HttpContext.Connection.LocalIpAddress}".FormatEndPoint();
                    var remoteIp = $"{HttpContext.Connection.RemoteIpAddress}".FormatEndPoint();
                    Debug.WriteLine($"[INFO] Connection.LocalIpAddress..: {localIp}");
                    _logger.LogInformation($"Local connection is {localIp} on port {HttpContext.Connection.LocalPort}");
                    Debug.WriteLine($"[INFO] Connection.RemoteIpAddress.: {remoteIp}");
                    _logger.LogInformation($"Remote connection is {remoteIp} on port {HttpContext.Connection.RemotePort}");
                }
                catch (Exception)
                {
                    Debug.WriteLine($"[WARNING] Error while logging connection details");
                }
                #endregion
            }
            else
            {
                // This can be done in the cshtml, just an example of a different way to implant view data back to the page.
                ViewData["StatusMessage"] = $"Today is {DateTime.Now.ToString("dddd, dd MMMM yyyy")}";
            }
            
            ViewData["AppBuild"] = $"build {Constants.AppBuild}";
            ViewData["AppVersion"] = $"version {Constants.GetCurrentAssemblyVersion()}";

            #region [Sorting by requested order]
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DueSortParm"] = sortOrder == "Due" ? "due_desc" : "Due";
            ViewData["AddedSortParm"] = sortOrder == "Added" ? "added_desc" : "Added";
            ViewData["CompletedSortParm"] = sortOrder == "Completed" ? "completed_desc" : "Completed";

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
                case "Completed":
                    items = items.OrderBy(t => t.CompletedDate);
                    break;
                case "completed_desc":
                    items = items.OrderByDescending(t => t.CompletedDate);
                    break;
                default:
                    // Incomplete (IsDone=false) first, then by DueDate ascending
                    items = items
                      .OrderBy(t => t.IsDone)         // false before true
                      .ThenBy(t => t.DueDate);
                    break;
            }
            #endregion

            // Use AsNoTracking() for read-only queries to improve performance.
            var model = await items.AsNoTracking().ToListAsync();

            return View(model);
        }
        
        [HttpGet] // GET: /TodoItems/Create
        /// <summary>
        ///   This method is used to display the Create.cshtml page.
        /// </summary>
        public IActionResult Create()
        {
            var model = new TodoItem
            {
                // default the DueDate to tomorrow
                DueDate = DateTime.Today.AddDays(1),
                EntryDate = DateTime.Today
            };

            ViewData["StatusMessage"] = $"Today is {DateTime.Now.ToString("dddd, dd MMMM yyyy")}";
            ViewData["AppBuild"] = $"build {Constants.AppBuild}";
            ViewData["AppVersion"] = $"version {Constants.GetCurrentAssemblyVersion()}";

            return View(model);
        }

        [HttpPost] // POST: /TodoItems/Create
        [ValidateAntiForgeryToken]
        /// <summary>
        ///   This method is used to save changes from the Create.cshtml page.
        /// </summary>
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
            else if (item.DueDate < item.EntryDate) // Don't allow past due dates.
            {
                // set the show flag
                TempData["ShowMessagePopup"] = true;
                // pass the messages joined by <br> so we can render HTML
                TempData["PopupMessage"] = $"The due date ({item.DueDate?.ToString("dddd, MMMM d yyyy")}) cannot be less than today.<br/>Please select a future date on the 📆 and try again.<br/>";
                // return the same view, so TempData lives through this request
                return View(item);
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
                // catch DB exceptions and add to modal error display
                _logger.LogError(ex, "Exception while saving new item");
                ModelState.AddModelError("", "Unable to save changes.");
                return View(item);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet] // GET: /TodoItems/Edit/5
        /// <summary>
        ///   This method is used to display the Edit.cshtml page.
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            
            if (item == null) 
                return NotFound();

            //TempData["ShowMessagePopup"] = true; // set the show flag
            //TempData["PopupMessage"] = "Test message here.<br/>"; // pass the messages joined by <br> so we can render HTML

            ViewData["StatusMessage"] = $"Today is {DateTime.Now.ToString("dddd, dd MMMM yyyy")}";
            ViewData["AppBuild"] = $"build {Constants.AppBuild}";
            ViewData["AppVersion"] = $"version {Constants.GetCurrentAssemblyVersion()}";

            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken] // POST: /TodoItems/Edit/5
        /// <summary>
        ///   This method is used to save the changes from the Edit.cshtml page.
        /// </summary>
        public async Task<IActionResult> Edit(int id, TodoItem item)
        {
            // This method is used to save the changes from the Edit.cshtml page.

            if (id != item.Id) 
                return BadRequest();

            if (!ModelState.IsValid) 
                return View(item);

            // Update the entry date if null
            if (item.EntryDate is null)
                item.EntryDate = DateTime.Now;

            // set or clear CompletedDate
            if (item.IsDone && item.CompletedDate is null)
                item.CompletedDate = DateTime.Now;
            else if (!item.IsDone)
                item.CompletedDate = null;

            // Don't allow past due dates.
            if (item.DueDate < item.EntryDate)
            {
                // set the show flag
                TempData["ShowMessagePopup"] = true;
                // pass the messages joined by <br> so we can render HTML
                TempData["PopupMessage"] = $"The due date ({item.DueDate?.ToString("dddd, MMMM d yyyy")}) cannot be less than today.<br/>Please select a future date on the 📆 and try again.<br/>";
                
                // return the same view, so TempData lives through this request
                return View(item);
            }
            else // clear the TempData if no errors
            {
                TempData.Remove("ShowMessagePopup");
                TempData.Remove("PopupMessage");
            }

            try
            {
                _db.Update(item);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // catch DB exceptions and add to modal error display
                _logger.LogError(ex, "Exception while saving new item");
                ModelState.AddModelError("", "Unable to save changes.");
                return View(item);
            }

            return RedirectToAction(nameof(Index));
        }

        
        [HttpGet] // GET: /TodoItems/Delete/5
        /// <summary>
        ///   This method was used to display the Delete.cshtml page.
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            
            if (item == null) 
                return NotFound();

            ViewData["StatusMessage"] = $"Today is {DateTime.Now.ToString("dddd, dd MMMM yyyy")}";
            ViewData["AppBuild"] = $"build {Constants.AppBuild}";
            ViewData["AppVersion"] = $"version {Constants.GetCurrentAssemblyVersion()}";

            return View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken] // HttpPost: /TodoItems/Delete/5
        /// <summary>
        ///   This method was used to remove the item from the DB.
        /// </summary>
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.TodoItems.FindAsync(id);
            
            _db.TodoItems.Remove(item!);
            
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Item as deleted: {item}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet] // GET: /TodoItems/Search
        public async Task<IActionResult> Search(string term, bool includeDetails = false)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            IQueryable<TodoItem> query = _db.TodoItems;

            if (includeDetails)
            {
                query = query.Where(t =>
                    t.Title.Contains(term) ||
                    t.Details.Contains(term));
            }
            else
            {
                query = query.Where(t =>
                    t.Title.Contains(term));
            }

            var results = await query
                .OrderBy(t => t.Title)
                .Select(t => new { t.Id, t.Title })
                .Take(5)
                .ToListAsync();

            return Json(results);
        }

        [HttpGet] // GET: /TodoItems/Statistics
        public async Task<IActionResult> Statistics()
        {
            var (fastest, slowest, average) = await GetCompletionTimeStatsAsync();

            var total = await _db.TodoItems.CountAsync();
            var completed = await _db.TodoItems.CountAsync(t => t.IsDone);
            var pending = total - completed;
            var vm = new StatisticsViewModel
            {
                CompletedCount = completed,
                PendingCount = pending,
                FastestCompletion = fastest,
                SlowestCompletion = slowest,
                AverageCompletion = average
            };
            return View(vm);
        }

        /// <summary>
        /// Returns a tuple representing the fastest and slowest time between <see cref="TodoItem.EntryDate"/>
        /// and <see cref="TodoItem.DueDate"/>. Also <see cref="TodoItem.CompletedDate"/> will be returned as an average.
        /// </summary>
        async Task<(string? Fastest, string? Slowest, string? Average)> GetCompletionTimeStatsAsync()
        {
            #region [Previous]
            // Fetch only completed items with non-null CompletedDate.
            // Returns a List<new { DateTime? Created, DateTime? Due, DateTime? Completed}>?
            //var items = await _db.TodoItems
            //    .Where(t => t.IsDone && t.CompletedDate.HasValue)
            //    .Select(t => new {
            //        Created = t.EntryDate,
            //        Due = t.DueDate,
            //        Completed = t.CompletedDate,
            //    }).ToListAsync();

            // Verify we have completed items.
            //if (!items.Any())
            //    return (string.Empty, string.Empty, string.Empty);

            // Project durations.
            //var durations = items.Select(x => x.Completed - x.Created);

            //var fast = durations.Min();
            //var slow = durations.Max();
            #endregion

            // Only grab completed items with a timestamp.
            var durations = await _db.TodoItems
                .Where(t => t.IsDone && t.CompletedDate.HasValue)
                .Select(t => t.CompletedDate.Value - t.EntryDate)
                .ToListAsync();

            // Make sure we have some results.
            if (!durations.Any())
                return (string.Empty, string.Empty, string.Empty);

            // min / max
            var fast = durations.Min();
            var slow = durations.Max();

            // calculate average using ticks
            var avgTicks = durations.Where(ts => ts.HasValue).Average(ts => ts.Value.Ticks);
            if (avgTicks.IsInvalid())
                avgTicks = 0d;

            var average = TimeSpan.FromTicks(Convert.ToInt64(avgTicks));

            // return min, max, and avg
            return (Fastest: fast.ToReadableTime(), Slowest: slow.ToReadableTime(), Average: ((TimeSpan?)average).ToReadableTime());
        }

        #region [Original/Default Methods]
        [HttpGet] // GET: /TodoItems
        public async Task<IActionResult> IndexOriginal()
        {
            var items = await _db.TodoItems.ToListAsync();
            return View(items);
        }

        // HttpGet: /TodoItems/Create
        public IActionResult CreateOriginal()
        {
            return View(); // this will result in the Model being null when the Create.cshtml page is loaded.
        }

        [HttpPost] // POST: /TodoItems/Create
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

        [HttpGet] // GET: /TodoItems/Search
        public async Task<IActionResult> SearchOriginal(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            // find up to 5 items whose title contains the term
            var results = await _db.TodoItems
                .Where(t => t.Title.Contains(term))
                .OrderBy(t => t.Title)
                .Select(t => new { t.Id, t.Title })
                .Take(5)
                .ToListAsync();

            return Json(results);
        }
        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}