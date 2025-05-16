using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class Priority
    {
        [Key]
        public int PriorityId { get; set; }
        public string Name { get; set; } = "Low";
    }
}
