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
        bool _logContext = false;
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
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder)
        {
            if (_logContext)
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
                    // Incomplete (IsDone=false) first, then by DueDate ascending
                    items = items
                      .OrderBy(t => t.IsDone)         // false before true
                      .ThenBy(t => t.DueDate);
                    break;
            }

            var model = await items.AsNoTracking().ToListAsync();
            return View(model);
        }

        // HttpGet: /TodoItems
        public async Task<IActionResult> IndexOriginal()
        {
            var items = await _db.TodoItems.ToListAsync();
            return View(items);
        }

        // HttpGet: /TodoItems/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new TodoItem
            {
                // default the DueDate to tomorrow
                DueDate = DateTime.Today.AddDays(1),
                EntryDate = DateTime.Today
            };
            return View(model);
        }

        // HttpGet: /TodoItems/Create
        public IActionResult CreateOriginal()
        {
            return View(); // this will result in the Model being null when the Create.cshtml page is loaded.
        }


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
        [HttpGet]
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
        [HttpGet]
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


        [HttpGet]
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

        [HttpGet]
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Logs the features of the current HTTP context.
        /// </summary>
        public static void ForEach(this Microsoft.AspNetCore.Http.Features.IFeatureCollection features)
        {
            foreach (KeyValuePair<Type, object> feature in features)
            {
                Debug.WriteLine($"[DEBUG] FeatureType: {feature.Key},  Value: {feature.Value}");
            }
        }

        /// <summary>
        /// Parses an endpoint string (e.g., "127.0.0.1:63908") and formats it.
        /// </summary>
        /// <param name="endPointString">The endpoint string to parse.</param>
        /// <returns>A formatted string, or "Invalid endpoint/IP/port format" if parsing fails.</returns>
        public static string FormatEndPoint(this string endPointString)
        {
            if (string.IsNullOrWhiteSpace(endPointString))
                return "Invalid endpoint format";
            try
            {
                if (endPointString.StartsWith("::1"))
                    return "localhost";

                string[] parts = endPointString.Split(':');
                if (parts.Length < 2)
                    return "Invalid endpoint format";

                string ipAddressString = parts[0];
                string portString = parts[1];

                if (!System.Net.IPAddress.TryParse(ipAddressString, out _))
                    return "Invalid IP address format";

                if (!int.TryParse(portString, out int port))
                    return "Invalid port format";

                return $"IP {ipAddressString}, port {port}";
            }
            catch (Exception)
            {
                return "Invalid endpoint format";
            }
        }
    }
}