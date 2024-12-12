using System.ComponentModel.DataAnnotations;

namespace SnakeHubServer.Model.Request
{
    public class RegisterRequest
    {
        [Required]
        public string Login { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
