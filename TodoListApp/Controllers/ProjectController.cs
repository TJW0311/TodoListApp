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
                DueProjectsPage = 1,
                DueProjectsTotalPages = 1,
                MyProjects = new List<Project>(),
                GroupProjects = new List<Project>(),
                DueProjects = new List<Project>()
            };

            return View(viewModel); // this must match @model ProjectViewModel in the .cshtml
        }

        public IActionResult LoadProjects(string section, int page = 1)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            const int PageSize = 8;

            var memberCount = _context.projectMembers
                    .GroupBy(pm => pm.ProjectId)
                    .ToDictionary(g => g.Key, g => g.Count());

            if (section == "almost-due")
            {
                var ProjectIds = _context.projectMembers
                    .GroupBy(pm => pm.ProjectId)
                    .Where(g => g.Any(pm => pm.UserId == userId))
                    .Select(g => g.Key);

                var top4DueProject = _context.todoLists
                    .Where(t => ProjectIds.Contains(t.ProjectId) && t.DueDate != null && t.StatusId != 6 && t.StatusId != 5)
                    .GroupBy(t => t.ProjectId)                    
                    .Select(g => new
                    {
                        ProjectId = g.Key,
                        NearestDueDate = g.Min(t => t.DueDate)
                    })
                    .OrderBy(g => g.NearestDueDate)
                    .Take(4)
                    .ToList();

                var top4DueProjectIds = top4DueProject.Select(g => g.ProjectId).ToList();
                var projectDueDates = top4DueProject.ToDictionary(g => g.ProjectId, g => g.NearestDueDate);

                var Projects = _context.projects
                    .Where(p => top4DueProjectIds.Contains(p.Id))
                    .ToList();

                Projects = top4DueProjectIds
                    .Select(id => Projects.First(p => p.Id == id))
                    .ToList();

                var vm = new ProjectViewModel
                {
                    DueProjects = Projects.ToList(),
                    //DueProjects = Projects.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                    //DueProjectsPage = page,
                    //DueProjectsTotalPages = (int)Math.Ceiling(Projects.Count() / (double)PageSize),
                    ProjectMemberCounts = memberCount,
                    ProjectDueDates = projectDueDates
                };

                return PartialView("_DueProjectCards", vm);
            }

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
                    MyProjectsTotalPages = (int)Math.Ceiling(myProjects.Count() / (double)PageSize),
                    ProjectMemberCounts = memberCount
                };

                return PartialView("_MyProjectCards", vm);
            }

            if (section == "group")
            {
                var groupProjectIds = _context.projectMembers
                    .GroupBy(pm => pm.ProjectId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                var validProjectIds = _context.projectMembers
                    .Where(pm => groupProjectIds.Contains(pm.ProjectId) && pm.UserId == userId)
                    .Select(pm => new
                    {
                        pm.ProjectId,
                        pm.Role,
                        HasAssignedTasks = _context.todoLists.Any(t =>
                            t.ProjectId == pm.ProjectId &&
                            t.AssignedToUserId == userId)
                    })
                    .Where(x =>
                        x.Role == "Manager" || (x.Role == "Member" && x.HasAssignedTasks))
                    .Select(x => x.ProjectId)
                    .Distinct();

                var groupProjects = _context.projects
                    .Where(p => validProjectIds.Contains(p.Id))
                    .OrderByDescending(p => p.Id);
               
                var vm = new ProjectViewModel
                {
                    GroupProjects = groupProjects.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                    GroupProjectsPage = page,
                    GroupProjectsTotalPages = (int)Math.Ceiling(groupProjects.Count() / (double)PageSize),
                    ProjectMemberCounts = memberCount
                };

                return PartialView("_GroupProjectCards", vm);
            }
            /*if (section == "group")
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
                    GroupProjectsTotalPages = (int)Math.Ceiling(groupProjects.Count() / (double)PageSize),
                    ProjectMemberCounts = memberCount
                };

                return PartialView("_GroupProjectCards", vm);
            }*/

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

        [HttpPost]
        public IActionResult UpdateProjectName([FromBody] ProjectUpdateDto dto)
        {
            var project = _context.projects.Find(dto.Id);
            if (project == null || string.IsNullOrWhiteSpace(dto.NewName) || dto.NewName.Length > 25)
                return BadRequest("Invalid request");

            project.Name = dto.NewName;
            _context.SaveChanges();

            return Ok("Updated");
        }

        [HttpGet]
        public IActionResult GetActiveTaskCount(int projectId)
        {
            var count = _context.todoLists
                .Count(t => t.ProjectId == projectId && t.StatusId != 5); // 5 = Completed

            return Ok(count);
        }

        [HttpPost]
        public IActionResult DeleteProject([FromBody] ProjectDeleteDto dto)
        {
            var project = _context.projects.Find(dto.Id);
            if (project == null) return NotFound();

            _context.projects.Remove(project);
            _context.SaveChanges();

            return Ok("Deleted");
        }

        public class ProjectUpdateDto 
        { 
            public int Id { get; set; } 
            public string NewName { get; set; } 
        }
        public class ProjectDeleteDto { public int Id { get; set; } }

    }
}
