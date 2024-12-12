using Microsoft.AspNetCore.Identity;

namespace SnakeHubServer.Model
{
    public class User: IdentityUser
    {
        public int TotalGames { get; set; }
        public int TotalScore { get; set; }
    }
}
