using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class ProjectInvitation
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string Token { get; set; }

        public int ProjectId { get; set; }

        public DateTime Expiration { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
