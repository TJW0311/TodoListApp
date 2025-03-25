using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class Status
    {
        [Key]
        public int StatusId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
