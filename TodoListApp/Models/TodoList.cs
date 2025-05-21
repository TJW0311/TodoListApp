using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class TodoList
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage ="Please enter task name.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter start date.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Please enter due date.")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Please select a priority.")]
        public int PriorityId { get; set; } = 1;
        [ValidateNever]
        public Priority Priority { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; } = 0;
        [ValidateNever]
        public Category Category { get; set; } = null!;

        public int StatusId { get; set; } = 0;
        [ValidateNever]
        public Status Status { get; set; } = null!;

        public int CreatedByUserId { get; set; }
        [ValidateNever]
        public User CreatedByUser { get; set; }

        public int? AssignedToUserId { get; set; }
        [ValidateNever]
        public User? AssignedToUser { get; set; }

        public int ProjectId { get; set; }
        [ValidateNever]
        public Project Project { get; set; }

        public bool Overdue => StatusId == 1 && DueDate < DateTime.Today;        
    }
}
