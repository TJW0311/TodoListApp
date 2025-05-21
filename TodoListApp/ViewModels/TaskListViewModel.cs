using TodoListApp.Models;

namespace TodoListApp.ViewModels
{
    public class TaskListViewModel
    {
        public List<TodoList> Tasks { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
