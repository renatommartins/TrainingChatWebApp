using System.ComponentModel.DataAnnotations;

namespace TrainingChatWebApp
{
    public class User
    {
        public int Id { get; set; }

        [StringLength(100, ErrorMessage = $"Vai tomar no cu tranquilo")]
        public string Username { get; set; }

        [StringLength(100, ErrorMessage = $"Vai tomar no cu tranquilo")]
        public string Email { get; set; }
    }
}
