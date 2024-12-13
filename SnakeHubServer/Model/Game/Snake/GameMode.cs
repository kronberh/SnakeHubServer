using SnakeHubServer.Interfaces;
using System.Drawing;
using Timer = System.Timers.Timer;

namespace SnakeHubServer.Model.Game.Snake
{
    public class GameMode : IGameMode
    {
        public GameMode()
        {
            Timer = new(250);
            GameObjectOnCollisionedActions = [];
            Players = [];
            PlayerSettings = [];
            PlayerMoveActions = [];
            PlayerColors =
                [
                    Color.FromColor(System.Drawing.Color.Orange),
                    Color.FromColor(System.Drawing.Color.Yellow),
                    Color.FromColor(System.Drawing.Color.Lime),
                    Color.FromColor(System.Drawing.Color.LightBlue),
                    Color.FromColor(System.Drawing.Color.Blue),
                    Color.FromColor(System.Drawing.Color.Purple),
                    Color.FromColor(System.Drawing.Color.White),
                    Color.FromColor(System.Drawing.Color.Black)
                ];
            GameState = new GameObject[16][];
            GameObject field = new() { Color = Color.FromColor(System.Drawing.Color.ForestGreen) };
            GameObject food = new() { Color = Color.FromColor(System.Drawing.Color.Red) };
            GameObjects = new()
            {
                { "field", field },
                { "food", food }
            };
            for (int i = 0; i < GameState.Length; i++)
            {
                GameState[i] = new GameObject[16];
                for (int j = 0; j < GameState[0].Length; j++)
                {
                    GameState[i][j] = GameObjects["field"];
                }
            }
            GameObjectOnCollisionedActions.Add(food, player =>
            {
                Point head = PlayerSettings[player].Body.FirstOrDefault();
                GameState[head.X][head.Y] = GameObjects[player.Id];
                PlayerSettings[player].Body.AddLast(PlayerSettings[player].Body.Last!.Value);
                if (!GameState.SelectMany(x => x).Any(x => x == food))
                {
                    SpawnFood();
                }
                Timer.Interval *= 0.98;
            });
            GameObjectOnCollisionedActions.Add(field, null);
            OnPlayerAction += (player, action) =>
            {
                switch (action)
                {
                    case Enums.Action.UP:
                        if (PlayerSettings[player].Body.FirstOrDefault().Y - (PlayerSettings[player].Body.First?.Next?.Value.Y ?? PlayerSettings[player].Body.FirstOrDefault().Y) != 1)
                        {
                            PlayerMoveActions[player] = PlayerMoveUp;
                        }
                        break;
                    case Enums.Action.DOWN:
                        if ((PlayerSettings[player].Body.First?.Next?.Value.Y ?? PlayerSettings[player].Body.FirstOrDefault().Y) - PlayerSettings[player].Body.FirstOrDefault().Y != 1)
                        {
                            PlayerMoveActions[player] = PlayerMoveDown;
                        }
                        break;
                    case Enums.Action.LEFT:
                        if (PlayerSettings[player].Body.FirstOrDefault().X - (PlayerSettings[player].Body.First?.Next?.Value.X ?? PlayerSettings[player].Body.FirstOrDefault().X) != 1)
                        {
                            PlayerMoveActions[player] = PlayerMoveLeft;
                        }
                        break;
                    case Enums.Action.RIGHT:
                        if ((PlayerSettings[player].Body.First?.Next?.Value.X ?? PlayerSettings[player].Body.FirstOrDefault().X) - PlayerSettings[player].Body.FirstOrDefault().X != 1)
                        {
                            PlayerMoveActions[player] = PlayerMoveRight;
                        }
                        break;
                }
            };
            OnGameTick += () =>
            {
                List<Player> PlayersCopy = [.. Players];
                if (PlayersCopy.Count == 0)
                {
                    Timer.Stop();
                    Timer.Dispose();
                }
                foreach (Player player in PlayersCopy)
                {
                    PlayerMoveActions[player](player);
                }
                foreach (Player player in PlayersCopy)
                {
                    GameObject collisionedObject = GameState[PlayerSettings[player].Body.FirstOrDefault().X][PlayerSettings[player].Body.FirstOrDefault().Y];
                    GameObjectOnCollisionedActions[collisionedObject]?.Invoke(player);
                }
            };
            Timer.Elapsed += (source, e) => OnGameTick();
        }
        ~GameMode()
        {
            Timer.Stop();
            Timer.Dispose();
        }
        public GameObject[][] GameState { get; }
        public List<Player> Players { get; }
        public Action<Player, Enums.Action> OnPlayerAction { get; }
        public int GetPlayerPoints(Player player)
        {
            if (Players.Contains(player))
            {
                try
                {
                    return PlayerSettings[player].Body.Count;
                }
                catch
                {
                    return 0;
                }
            }
            throw new InvalidOperationException("You lost!");
        }
        public void StartGame()
        {
            foreach (Player player in Players)
            {
                PlayerSettings.Add(player, new() { Color = new Random().GetItems(PlayerColors, 1)[0] });
                GameObject obj = new() { Color = PlayerSettings[player].Color };
                GameObjects.Add(player.Id, obj);
                GameObjectOnCollisionedActions.Add(obj, player =>
                {
                    if (obj == GameObjects[player.Id] && PlayerSettings[player].Body.Count == PlayerSettings[player].Body.Distinct().Count())
                    {
                        return;
                    }
                    foreach (Point point in PlayerSettings[player].Body.Skip(1))
                    {
                        GameState[point.X][point.Y] = GameObjects["food"];
                    }
                    Players.Remove(player);
                    GameObjects.Remove(player.Id);
                    PlayerSettings.Remove(player);
                    PlayerMoveActions.Remove(player);
                });
                Point startPoint;
                do
                {
                    startPoint = new(0, new Random().Next(GameState[0].Length));
                }
                while (GameState[startPoint.X][startPoint.Y] != GameObjects["field"]);
                PlayerSettings[player].Body.AddLast(startPoint);
                GameState[startPoint.X][startPoint.Y] = GameObjects[player.Id];
                PlayerMoveActions.Add(player, PlayerMoveRight);
            }
            SpawnFood();
            Timer.Start();
        }
        /*-----------------------------------------------------------------------------*/
        private readonly Timer Timer;
        private readonly Action OnGameTick;
        private readonly Dictionary<string, GameObject> GameObjects;
        private readonly Dictionary<GameObject, Action<Player>?> GameObjectOnCollisionedActions;
        private readonly Dictionary<Player, PlayerSettings> PlayerSettings;
        private readonly Dictionary<Player, Action<Player>> PlayerMoveActions;
        private readonly Color[] PlayerColors;
        private void SpawnFood()
        {
            Point foodPoint;
            do
            {
                foodPoint = new(new Random().Next(GameState.Length), new Random().Next(GameState[0].Length));
            }
            while (GameState[foodPoint.X][foodPoint.Y] != GameObjects["field"]);
            GameState[foodPoint.X][foodPoint.Y] = GameObjects["food"];
        }
        private void PlayerMoveUp(Player player)
        {
            Point head = PlayerSettings[player].Body.FirstOrDefault();
            Point tail = PlayerSettings[player].Body.LastOrDefault();
            int newY = head.Y < 1 ? GameState[0].Length - 1 : head.Y - 1;
            PlayerSettings[player].Body.AddFirst(new Point(head.X, newY));
            PlayerSettings[player].Body.RemoveLast();
            GameState[tail.X][tail.Y] = GameObjects["field"];
            if (GameState[head.X][newY] == GameObjects["field"])
            {
                GameState[head.X][newY] = GameObjects[player.Id];
            }
        }
        private void PlayerMoveDown(Player player)
        {
            Point head = PlayerSettings[player].Body.FirstOrDefault();
            Point tail = PlayerSettings[player].Body.LastOrDefault();
            int newY = head.Y >= GameState[0].Length - 1 ? 0 : head.Y + 1;
            PlayerSettings[player].Body.AddFirst(new Point(head.X, newY));
            PlayerSettings[player].Body.RemoveLast();
            GameState[tail.X][tail.Y] = GameObjects["field"];
            if (GameState[head.X][newY] == GameObjects["field"])
            {
                GameState[head.X][newY] = GameObjects[player.Id];
            }
        }
        private void PlayerMoveLeft(Player player)
        {
            Point head = PlayerSettings[player].Body.FirstOrDefault();
            Point tail = PlayerSettings[player].Body.LastOrDefault();
            int newX = head.X < 1 ? GameState.Length - 1 : head.X - 1;
            PlayerSettings[player].Body.AddFirst(new Point(newX, head.Y));
            PlayerSettings[player].Body.RemoveLast();
            GameState[tail.X][tail.Y] = GameObjects["field"];
            if (GameState[newX][head.Y] == GameObjects["field"])
            {
                GameState[newX][head.Y] = GameObjects[player.Id];
            }
        }
        private void PlayerMoveRight(Player player)
        {
            Point head = PlayerSettings[player].Body.FirstOrDefault();
            Point tail = PlayerSettings[player].Body.LastOrDefault();
            int newX = head.X >= GameState.Length - 1 ? 0 : head.X + 1;
            PlayerSettings[player].Body.AddFirst(new Point(newX, head.Y));
            PlayerSettings[player].Body.RemoveLast();
            GameState[tail.X][tail.Y] = GameObjects["field"];
            if (GameState[newX][head.Y] == GameObjects["field"])
            {
                GameState[newX][head.Y] = GameObjects[player.Id];
            }
        }
    }
    public class PlayerSettings
    {
        public LinkedList<Point> Body { get; } = [];
        public Color Color { get; set; } = null!;
    }
}
