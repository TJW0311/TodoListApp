using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a Project Name.")]
        public string Name { get; set; }

        public int CreatedByUserId { get; set; }
        [ValidateNever]
        public User CreatedByUser { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
