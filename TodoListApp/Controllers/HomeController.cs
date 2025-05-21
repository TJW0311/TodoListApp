using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using TodoListApp.Models;
using TodoListApp.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing;
using ClosedXML.Excel;

namespace TodoListApp.Controllers
{
    public class HomeController : Controller
    {
        private TodoDb todoDb_context;

        public HomeController(TodoDb db) => todoDb_context = db;

        public IActionResult Index(int projId, string id, string sortBy, string sortDir, string searchType, string searchValue, int page = 1)
        {
            int pageSize = 10;
            if (projId != 0)
            {
                HttpContext.Session.SetInt32("ProjId", projId);
            }
            else
            {
                int? sessionProjId = HttpContext.Session.GetInt32("ProjId");

                if (sessionProjId.HasValue)
                {
                    projId = sessionProjId.Value;
                }
                else
                {
                    // Handle null: redirect or throw error depending on your app logic
                    return RedirectToAction("Project", "Project");
                }
            }

            var project = todoDb_context.projects.FirstOrDefault(p => p.Id == projId);
            ViewBag.ProjectName = project?.Name ?? "Project";
            @ViewBag.ProjectId = project?.Id;


            var filters = new Filters(id);
            ViewBag.Filters = filters;

            ViewBag.Categories = todoDb_context.categories.ToList();
            ViewBag.Status = todoDb_context.status.ToList();
            ViewBag.Priority = todoDb_context.priorities.ToList();
            ViewBag.DueFilters = Filters.DueFilterValues;

            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;

            ViewBag.SearchType = searchType;
            ViewBag.SearchValue = searchValue;

            var members = todoDb_context.projectMembers
                .Where(pm => pm.ProjectId == projId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToList();

            ViewBag.Members = new SelectList(members, "UserId", "Name");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth"); // force login if not logged in
            }
            else
            {
                var role = todoDb_context.projectMembers
                    .Where(pm => pm.ProjectId == projId)
                    .Include(pm => pm.User);
                ViewBag.CurrentUserRole = role.FirstOrDefault(m => m.UserId == userId)?.Role ?? "Member";
            }

            IQueryable<TodoList> query = todoDb_context.todoLists
                .Where(t => t.ProjectId == projId)
                .Include(t => t.Category)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchType) && !string.IsNullOrEmpty(searchValue))
            {
                if (searchType == "name")
                    query = query.Where(t => t.Name.Contains(searchValue));
                else if (searchType == "start" && DateTime.TryParse(searchValue, out DateTime startDate))
                    query = query.Where(t => t.StartDate != null && t.StartDate.Value.Date == startDate.Date);
                else if (searchType == "end" && DateTime.TryParse(searchValue, out DateTime endDate))
                    query = query.Where(t => t.DueDate != null && t.DueDate.Value.Date == endDate.Date);
            }

            if (filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId.ToString() == filters.CategoryId);
            }

            if (filters.HasPriority)
            {
                query = query.Where(t => t.PriorityId.ToString() == filters.PriorityId);
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
                ("Status","desc") => query.OrderByDescending(s => s.StatusId),
                ("Status", _) => query.OrderBy(_ => _.StatusId),
                ("Priority", "desc") => query.OrderByDescending(s => s.PriorityId),
                ("Priority", _) => query.OrderBy(_ => _.PriorityId),
                ("StartDate", "desc") => query.OrderByDescending(t => t.StartDate),
                ("StartDate", _) => query.OrderBy(t => t.StartDate),
                ("DueDate", "desc") => query.OrderByDescending(t => t.DueDate),
                ("DueDate", _) => query.OrderBy(t => t.DueDate),
                _ => query.OrderByDescending(t => t.Id)
            };

            string sortLabel = sortBy switch
            {
                "Name" => "Name",
                "Status" => "Status",
                "StartDate" => "Start Date",
                "DueDate" => "Due Date",
                "Priority" => "Priority",
                _ => "Due Date"
            };

            string sortDirection = sortDir == "desc" ? "descending" : "ascending";
            if(sortLabel == "Status" || sortLabel == "Priority")
            {
                TempData["SortToast"] = $"Sorted by {sortLabel} Hierarchy ({sortDirection})";
            }
            else {
                TempData["SortToast"] = $"Sorted by {sortLabel} ({sortDirection})";
            }
            
            int totalTasks = query.Count();
            var tasks = query
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            var vm = new TaskListViewModel
            {
                Tasks = tasks,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalTasks / (double)pageSize)
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Return only partial for AJAX calls
                return PartialView("_TaskTable", vm);
            }

            // Return full page normally
            return View(vm);
        }

        //Call Need to load Add New Task Page
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = todoDb_context.categories.ToList();
            ViewBag.Status = todoDb_context.status.ToList();
            ViewBag.Priority = todoDb_context.priorities.ToList();

            int? projId = HttpContext.Session.GetInt32("ProjId");


            if (!projId.HasValue)
            { 
                // Handle null: redirect or throw error depending on your app logic
                return RedirectToAction("Project", "Project");
            }

            ViewBag.Members = todoDb_context.projectMembers
                .Where(pm => pm.ProjectId == projId)
                .Include(pm => pm.User)
                .Select(pm => new SelectListItem
                {
                    Value = pm.UserId.ToString(),
                    Text = pm.User.Name
                }).ToList();

            var tasks = new TodoList
            {
                UserId = HttpContext.Session.GetInt32("UserId") ?? 0,
                StatusId = 1,
            };

            return View(tasks);
        }

        [HttpPost]
        public IActionResult Add(TodoList task)
        {
            if (string.IsNullOrWhiteSpace(task.Description))
                ModelState.AddModelError("Description", "Please enter description.");            
            if (task.CategoryId == 0)
                ModelState.AddModelError("CategoryId", "Please select a category.");
            if (!task.AssignedToUserId.HasValue)
                ModelState.AddModelError("AssignedToUserId", "Please assign to a member.");
            if (task.StartDate.HasValue && task.DueDate.HasValue && task.DueDate < task.StartDate)
                ModelState.AddModelError("DueDate", "Due date must be after start date.");
            if (task.PriorityId == 0)
                ModelState.AddModelError("PriorityId", "Please select a priority.");
            if (task.StatusId == 0)
                ModelState.AddModelError("StatusId", "Please select a status.");

            task.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            task.ProjectId = HttpContext.Session.GetInt32("ProjId") ?? 0;
            task.CreatedByUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (ModelState.IsValid)
            {
                todoDb_context.todoLists.Add(task);
                todoDb_context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = todoDb_context.categories.ToList();
            ViewBag.Status = todoDb_context.status.ToList();
            ViewBag.Priority = todoDb_context.priorities.ToList();

            int? projId = HttpContext.Session.GetInt32("ProjId");


            if (!projId.HasValue)
            {
                // Handle null: redirect or throw error depending on your app logic
                return RedirectToAction("Project", "Project");
            }

            ViewBag.Members = todoDb_context.projectMembers
                .Where(pm => pm.ProjectId == projId)
                .Include(pm => pm.User)
                .Select(pm => new SelectListItem
                {
                    Value = pm.UserId.ToString(),
                    Text = pm.User.Name
                }).ToList();
            return View(task);
        }

        [HttpPost]
        public IActionResult Filter(string[] filter, 
            string searchType,
            string searchValue,
            string sortBy,
            string sortDir)
        { 
            string id = string.Join("-", filter);
            return RedirectToAction("Index", new {ID =id, sortBy, sortDir, searchType, searchValue });
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

        [HttpGet]
        public JsonResult GetTaskDetails(int id)
        {
            var task = todoDb_context.todoLists
                .Include(t => t.Category)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.AssignedToUser)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return Json(null);

            return Json(new
            {
                task.Id,
                task.Name,
                task.Description,
                task.CategoryId,
                StartDate = task.StartDate?.ToString("yyyy-MM-dd"),
                DueDate = task.DueDate?.ToString("yyyy-MM-dd"),
                task.PriorityId,
                task.StatusId,
                task.AssignedToUserId
            });
        }

        [HttpPost]
        public IActionResult Edit(TodoList updatedTask, string id, string filterId, string sortBy, string sortDir, string searchType, string searchValue)
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
                task.PriorityId = updatedTask.PriorityId;
                task.AssignedToUserId = updatedTask.AssignedToUserId;

                todoDb_context.SaveChanges();
            }
            return RedirectToAction("Index", new { ID = filterId, sortBy, sortDir, searchType, searchValue });
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


        //Load Manage Member Popup
        [HttpGet]
        public async Task<IActionResult> ManageMembersPartial(int projectId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth"); // force login if not logged in

            var currentUser = await todoDb_context.users.FirstOrDefaultAsync(u => u.UserId == userId);

            var members = await todoDb_context.projectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.User)
                .ToListAsync();

            var vm = new ManageMemberViewModel
            {
                ProjectId = projectId,
                Members = members,
                CurrentUserRole = members.FirstOrDefault(m => m.UserId == currentUser.UserId)?.Role ?? "Member"
            };

            return PartialView("_ManageMemberModal", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SearchUserByEmail(string email)
        {
            var users = await todoDb_context.users
                .Where(u => u.Email.Contains(email)) // you can use .ToLower() for case-insensitive match
                .ToListAsync();

            if (!users.Any())
                return Json(new { found = false });

            return Json(new { found = true, users });
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int projectId, int userId)
        {
            if (todoDb_context.projectMembers.Any(pm => pm.ProjectId == projectId && pm.UserId == userId))
                return BadRequest("User already a member");

            var projectMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = "Member"
            };

            todoDb_context.projectMembers.Add(projectMember);
            await todoDb_context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(int projectId, int userId, string newRole)
        {
            var member = await todoDb_context.projectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
                return Json(new { success = false, message = "Member not found." });

            if (member.Role == "Manager" && newRole != "Manager")
            {
                int otherManagers = await todoDb_context.projectMembers
                    .CountAsync(pm => pm.ProjectId == projectId && pm.Role == "Manager" && pm.UserId != userId);

                if (otherManagers < 1)
                {
                    return Json(new { success = false, message = "At least one member must have the Manager role." });
                }
            }

            member.Role = newRole;
            await todoDb_context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int projectId, int userId)
        {
            var member = await todoDb_context.projectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
                return Json(new { success = false, message = "Member not found." });

            if (member.Role == "Manager")
            {
                int otherManagers = await todoDb_context.projectMembers
                    .CountAsync(pm => pm.ProjectId == projectId && pm.Role == "Manager" && pm.UserId != userId);

                if (otherManagers < 1)
                {
                    return Json(new { success = false, message = "Cannot remove the only Manager in the project." });
                }
            }

            todoDb_context.projectMembers.Remove(member);
            await todoDb_context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTasksExist(int projectId)
        {
            var hasTasks = await todoDb_context.todoLists.AnyAsync(t => t.ProjectId == projectId);
            return Json(new { exists = hasTasks });
        }

        //Export all tasks to excel
        [HttpPost]
        public async Task<IActionResult> ExportTasksToExcel(int projectId)
        {
            var project = await todoDb_context.projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return Json(new { success = false, message = "Project not found." });

            var tasks = await todoDb_context.todoLists
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Category)
                .ToListAsync();

            if (!tasks.Any())
                return Json(new { success = false, message = "No tasks found to export." });

            var members = await todoDb_context.projectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.User)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Project Tasks");

            // Project name header
            ws.Range("A1:J1").Merge();
            ws.Cell("A1").Value = $"Project Name: {project.Name}";
            ws.Cell("A1").Style.Font.SetBold().Font.FontSize = 14;
            ws.Cell("A1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws.Cell("A1").Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            // Task headers
            string[] headers = { "No", "Name", "Description", "Category", "Start Date", "Due Date", "Priority", "Status", "Created By", "Assign To" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(3, i + 1).Value = headers[i];

            var headerRange = ws.Range("A3:J3");
            headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
            headerRange.Style.Font.SetBold();
            headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Task rows
            int row = 4, no = 1;
            foreach (var task in tasks)
            {
                ws.Cell(row, 1).Value = no++;
                ws.Cell(row, 2).Value = task.Name;
                ws.Cell(row, 3).Value = task.Description;
                ws.Cell(row, 4).Value = task.Category?.Name;
                ws.Cell(row, 5).Value = task.StartDate?.ToString("yyyy-MM-dd");
                ws.Cell(row, 6).Value = task.DueDate?.ToString("yyyy-MM-dd");
                ws.Cell(row, 7).Value = task.Priority?.Name;
                ws.Cell(row, 8).Value = task.Status?.Name;
                ws.Cell(row, 9).Value = task.CreatedByUser?.Name;
                ws.Cell(row, 10).Value = task.AssignedToUser?.Name;
                row++;
            }

            if (!tasks.Any())
            {
                ws.Cell("A4").Value = "No tasks found for this project.";
                ws.Range("A4:J4").Merge();
                ws.Range("A4:J4").Style
                    .Font.SetItalic()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            // Export on
            ws.Cell("L1").Value = "Export On:";
            ws.Cell("M1").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            ws.Range("L1:M1").Style.Font.SetBold();

            // Statistics
            // Expected order
            var expectedStatuses = new[] {
                "Draft", "Waiting Approval", "Approved", "In Progress", "Completed", "Overdue"
            };
            var expectedPriorities = new[] { "High", "Medium", "Low" };

            // Count logic
            var fullStatusCounts = expectedStatuses.ToDictionary(s => s,
               s => s == "Overdue"
                    ? tasks.Count(t => t.Status?.Name != "Completed" && t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Today)
                    : tasks.Count(t => t.Status?.Name == s)
            );

            var fullPriorityCounts = expectedPriorities.ToDictionary(p => p,
                p => tasks.Count(t => t.Priority?.Name == p)
            );

            // Write statistics to worksheet
            int statRow = 4;
            ws.Range("L3:M3").Merge().Value = "Statistics";
            ws.Range("L3:M3").Style.Font.SetBold().Font.FontSize = 12;
            ws.Range("L3:M3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(statRow, 12).Value = "Status";
            ws.Cell(statRow, 13).Value = "Count";
            ws.Range(statRow, 12, statRow, 13).Style.Font.SetBold();
            statRow++;

            foreach (var kv in fullStatusCounts)
            {
                ws.Cell(statRow, 12).Value = kv.Key;
                ws.Cell(statRow, 13).Value = kv.Value;
                statRow++;
            }

            // Leave one row
            statRow++;

            ws.Cell(statRow, 12).Value = "Priority";
            ws.Cell(statRow, 13).Value = "Count";
            ws.Range(statRow, 12, statRow, 13).Style.Font.SetBold();
            statRow++;

            foreach (var kv in fullPriorityCounts)
            {
                ws.Cell(statRow, 12).Value = kv.Key;
                ws.Cell(statRow, 13).Value = kv.Value;
                statRow++;
            }

            // Draw final border around whole statistics section
            int statStartRow = 4;
            int statEndRow = statRow - 1;
            var fullStatRange = ws.Range(statStartRow, 12, statEndRow, 13);
            fullStatRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            fullStatRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Members table
            int memberCol = 16;
            int memberRow = 3;
            ws.Range(memberRow, memberCol, memberRow, memberCol + 2).Merge().Value = "Members";
            ws.Range(memberRow, memberCol, memberRow, memberCol + 2).Style.Font.SetBold().Font.FontSize = 12;
            ws.Range(memberRow, memberCol, memberRow, memberCol + 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            memberRow++;

            ws.Cell(memberRow, memberCol).Value = "Email";
            ws.Cell(memberRow, memberCol + 1).Value = "Name";
            ws.Cell(memberRow, memberCol + 2).Value = "Role";
            ws.Range(memberRow, memberCol, memberRow, memberCol + 2).Style.Font.SetBold();
            memberRow++;

            foreach (var m in members)
            {
                ws.Cell(memberRow, memberCol).Value = m.User.Email;
                ws.Cell(memberRow, memberCol + 1).Value = m.User.Name;
                ws.Cell(memberRow, memberCol + 2).Value = m.Role;
                memberRow++;
            }

            ws.Cell(memberRow, memberCol + 1).Value = "Total";
            ws.Cell(memberRow, memberCol + 2).Value = members.Count;
            ws.Range(memberRow, memberCol + 1, memberRow, memberCol + 2).Style.Font.SetBold();
            int memberTableStartRow = 4; // Member header starts at P4
            int memberTableEndRow = memberRow; // Includes "Total"
            var memberTableRange = ws.Range(memberTableStartRow, memberCol, memberTableEndRow, memberCol + 2);
            memberTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            memberTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            memberTableRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            // Final formatting
            ws.Columns("A:R").AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Project_{project.Name}_Tasks_{DateTime.Now:yyyyMMddHHmm}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
