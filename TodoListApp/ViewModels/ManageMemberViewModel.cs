using TodoListApp.Models;

namespace TodoListApp.ViewModels
{
    public class ManageMemberViewModel
    {
        public int ProjectId { get; set; }

        // List of current project members
        public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        // The role of the currently logged-in user (Manager or Member)
        public string CurrentUserRole { get; set; } = "Member";
    }
}
