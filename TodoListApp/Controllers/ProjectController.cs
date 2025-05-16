using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TodoListApp.Models;
using TodoListApp.ViewModels;

namespace TodoListApp.Controllers
{
    public class ProjectController : Controller
    {
        private readonly TodoDb _context;

        public ProjectController(TodoDb context)
        {
            _context = context;
        }

        public IActionResult Project()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Dummy or initial values
            var viewModel = new ProjectViewModel
            {
                MyProjectsPage = 1,
                GroupProjectsPage = 1,
                MyProjectsTotalPages = 1,
                GroupProjectsTotalPages = 1,
                MyProjects = new List<Project>(),
                GroupProjects = new List<Project>()
            };

            return View(viewModel); // this must match @model ProjectViewModel in the .cshtml
        }

        public IActionResult LoadProjects(string section, int page = 1)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            const int PageSize = 8;

            if (section == "my")
            {
                var myProjectIds = _context.projectMembers
                    .GroupBy(pm => pm.ProjectId)
                    .Where(g => g.Count() == 1 && g.Any(pm => pm.UserId == userId))
                    .Select(g => g.Key);

                var myProjects = _context.projects
                    .Where(p => myProjectIds.Contains(p.Id))
                    .OrderByDescending(p => p.Id);

                var vm = new ProjectViewModel
                {
                    MyProjects = myProjects.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                    MyProjectsPage = page,
                    MyProjectsTotalPages = (int)Math.Ceiling(myProjects.Count() / (double)PageSize)
                };

                return PartialView("_MyProjectCards", vm);
            }

            if (section == "group")
            {
                var groupProjectIds = _context.projectMembers
                    .GroupBy(pm => pm.ProjectId)
                    .Where(g => g.Count() > 1 && g.Any(pm => pm.UserId == userId))
                    .Select(g => g.Key);

                var groupProjects = _context.projects
                    .Where(p => groupProjectIds.Contains(p.Id))
                    .OrderByDescending(p => p.Id);

                var vm = new ProjectViewModel
                {
                    GroupProjects = groupProjects.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                    GroupProjectsPage = page,
                    GroupProjectsTotalPages = (int)Math.Ceiling(groupProjects.Count() / (double)PageSize)
                };

                return PartialView("_GroupProjectCards", vm);
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult Add(Project project)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                project.CreatedByUserId = userId.Value;
            }

            if (ModelState.IsValid)
            {
                // Save the project
                _context.projects.Add(project);
                _context.SaveChanges();

                // Add the current user as a member of this project
                var member = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = userId.Value,
                    Role = "Manager"
                };

                _context.projectMembers.Add(member);
                _context.SaveChanges();

                return RedirectToAction("Project", "Project");
            }

            TempData["ToastMessage"] = "Failed to create project. Please check inputs.";
            return RedirectToAction("Project", "Project");
        }

    }

}
