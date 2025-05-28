using TodoListApp.Models;

namespace TodoListApp.ViewModels
{
    public class ProjectViewModel
    {
        public List<Project> MyProjects { get; set; } = new();
        public List<Project> GroupProjects { get; set; } = new();
        public List<Project> DueProjects { get; set; } = new();

        public int MyProjectsPage { get; set; }
        public int GroupProjectsPage { get; set; }
        public int MyProjectsTotalPages { get; set; }
        public int GroupProjectsTotalPages { get; set; }

        public Dictionary<int, int> ProjectMemberCounts { get; set; } = new();
    }
}
