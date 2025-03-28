using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter Username.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter email."), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter password."), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
