using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using TodoListApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TodoListApp.Controllers
{
    public class HomeController : Controller
    {
        private TodoDb todoDb_context;

        public HomeController(TodoDb db) => todoDb_context = db;

        public IActionResult Index(string id, string sortBy = "DueDate", string sortDir = "asc")
        {
            var filters = new Filters(id);
            ViewBag.Filters = filters;

            ViewBag.Categories = todoDb_context.categories.ToList();
            ViewBag.Status = todoDb_context.status.ToList();
            ViewBag.DueFilters = Filters.DueFilterValues;

            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth"); // force login if not logged in

            IQueryable<TodoList> query = todoDb_context.todoLists
                .Where(t => t.UserId == userId) // Filter tasks by logged-in user
                .Include(t => t.Category)
                .Include(t => t.Status);

            if (filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId.ToString() == filters.CategoryId);
            }

            if (filters.HasStatus)
            {
                query = query.Where(t => t.StatusId.ToString() == filters.StatusId);
            }

            if (filters.HasDue)
            {
                var today = DateTime.Today;
                if (filters.IsPast)
                {
                    query = query.Where(t => t.DueDate < today);
                }
                else if (filters.IsFuture)
                {
                    query = query.Where(t => t.DueDate > today);
                }
                else if (filters.IsToday)
                {
                    query = query.Where(t => t.DueDate == today);
                }
            }

            // Apply Sorting
            query = (sortBy, sortDir) switch
            {
                ("Name", "desc") => query.OrderByDescending(t => t.Name),
                ("Name", _) => query.OrderBy(t => t.Name),
                ("DueDate", "desc") => query.OrderByDescending(t => t.DueDate),
                ("DueDate", _) => query.OrderBy(t => t.DueDate),
                _ => query.OrderBy(t => t.DueDate)
            };

            string sortLabel = sortBy switch
            {
                "Name" => "Name",
                "DueDate" => "Due Date",
                _ => "Due Date"
            };

            string sortDirection = sortDir == "desc" ? "descending" : "ascending";
            TempData["SortToast"] = $"Sorted by {sortLabel} ({sortDirection})";

            return View(query.ToList());
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = todoDb_context.categories.ToList();
            ViewBag.Status = todoDb_context.status.ToList();

            var tasks = new TodoList
            {
                UserId = HttpContext.Session.GetInt32("UserId") ?? 0,
                StatusId = 1
            };

            return View(tasks);
        }

        [HttpPost]
        public IActionResult Add(TodoList task)
        {
            task.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (ModelState.IsValid)
            {
                todoDb_context.todoLists.Add(task);
                todoDb_context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = todoDb_context.categories.ToList();
            ViewBag.Status = todoDb_context.status.ToList();
            return View(task);
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        { 
            string id = string.Join("-", filter);
            return RedirectToAction("Index", new {ID =id});
        }

        [HttpPost]
        public IActionResult CompleteTask([FromRoute] string id, int todoListId)
        {
            var task = todoDb_context.todoLists.Find(todoListId);
            if (task != null)
            {
                task.StatusId = 2;
                todoDb_context.SaveChanges();
            }
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult Edit(TodoList updatedTask)
        {
            var task = todoDb_context.todoLists.Find(updatedTask.Id);
            if (task != null)
            {
                task.Name = updatedTask.Name;
                task.Description = updatedTask.Description;
                task.CategoryId = updatedTask.CategoryId;
                task.StartDate = updatedTask.StartDate;
                task.DueDate = updatedTask.DueDate;
                task.StatusId = updatedTask.StatusId;

                todoDb_context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var selected = todoDb_context.todoLists.Find(id);
            if (selected != null)
            {
                todoDb_context.todoLists.Remove(selected);
                todoDb_context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
