namespace SnakeHubServer.Model.Game
{
    public class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
        public static Color FromColor(System.Drawing.Color color)
        {
            return new()
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A
            };
        }
    }
}
