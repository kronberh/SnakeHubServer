using System.ComponentModel.DataAnnotations;

namespace SnakeHubServer.Model.Request
{
    public class LoginRequest
    {
        [Required]
        public string Login { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
