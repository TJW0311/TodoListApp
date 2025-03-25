using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter Username.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter password.")]
        public string Password { get; set; } = string.Empty;
    }
}
