using SnakeHubServer.Enums;

namespace SnakeHubServer.Model
{
    public class SessionInfo
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int PlayerCount { get; set; }
        public SessionStatus Status { get; set; }
    }
}
