using SnakeHubServer.Interfaces;
using SnakeHubServer.Model;
using SnakeHubServer.Model.Game;
using SnakeHubServer.Model.Game.Snake;
using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace SnakeHubServer.Service
{
    public class GameSessionsService
    {
        private readonly ConcurrentDictionary<string, (IGameMode Game, SessionInfo Info)> _games = [];
        private readonly ConcurrentDictionary<string, Player> _players = [];
        private readonly ILogger<GameSessionsService> _logger;
        private readonly Timer _timer = new(TimeSpan.FromHours(1));
        public GameSessionsService(ILogger<GameSessionsService> logger)
        {
            _logger = logger;
            _timer.Elapsed += async (sender, e) =>
            {
                (int games, int players) = await ClearCacheAsync();
                _logger.LogInformation("Sessions cache was cleared: total {games} games and {players} players removed.", games, players);
            };
        }
        public async Task<IEnumerable<SessionInfo>> GetSessionsAsync()
        {
            return await Task.FromResult(_games.Where(x => x.Value.Game.Players.Count != 0).Select(x => x.Value.Info));
        }
        public async Task<string> HostGameAsync(string playerId, string modeId)
        {
            string gameId = Guid.NewGuid().ToString();
            GameMode game = new(); // todo create fitting instance by modeId
            Player newPlayer = new() { Id = playerId };
            game.Players.Add(newPlayer);
            _players[playerId] = newPlayer;
            _games[gameId] = (game, new()
            {
                Id = gameId,
                Name = "Snake", // todo adjuct name
                PlayerCount = 1,
                Status = Enums.SessionStatus.OPEN
            });
            _logger.LogInformation("New game was hosted with id={gameId} by player with id={playerId}.", gameId, playerId);
            return await Task.FromResult(gameId);
        }
        public async Task StartGameAsync(string gameId)
        {
            if (_games.TryGetValue(gameId, out (IGameMode Game, SessionInfo Info) game))
            {
                game.Info.Status = Enums.SessionStatus.INPROGRESS;
                await Task.Run(game.Game.StartGame);
                return;
            }
            throw new InvalidOperationException("Game not found.");
        }
        public async Task JoinGameAsync(string gameId, string playerId)
        {
            if (_games.TryGetValue(gameId, out (IGameMode Game, SessionInfo Info) game))
            {
                Player newPlayer = new() { Id = playerId };
                await Task.Run(() => game.Game.Players.Add(newPlayer));
                _players[playerId] = newPlayer;
                _logger.LogInformation("Player with id={playerId} has joined the game with id={gameId}.", playerId, gameId);
                return;
            }
            throw new InvalidOperationException("Game not found.");
        }
        public async Task<GameObject[][]> GetGameStateAsync(string gameId)
        {
            if (_games.TryGetValue(gameId, out (IGameMode Game, SessionInfo Info) game))
            {
                return await Task.FromResult(game.Game.GameState);
            }
            throw new InvalidOperationException("Game not found.");
        }
        public async Task<int> GetPlayerScoreAsync(string gameId, string playerId)
        {
            if (_games.TryGetValue(gameId, out (IGameMode Game, SessionInfo Info) game) && _players.TryGetValue(playerId, out Player? player))
            {
                return await Task.FromResult(game.Game.GetPlayerPoints(player));
            }
            throw new InvalidOperationException("Game not found.");
        }
        public async Task<string> GetPlayerGameAsync(string playerId)
        {
            if (_players.TryGetValue(playerId, out Player? player))
            {
                return await Task.FromResult(_games.First(x => x.Value.Game.Players.Contains(player)).Key);
            }
            throw new InvalidOperationException("Player not found.");
        }
        public async Task<bool> PlayerActionAsync(string gameId, string playerId, Enums.Action action)
        {
            if (_games.TryGetValue(gameId, out (IGameMode Game, SessionInfo Info) game) && _players.TryGetValue(playerId, out Player? player))
            {
                IGameMode snakeGame = game.Game;
                snakeGame.OnPlayerAction(player, action);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }
        private async Task<(int games, int players)> ClearCacheAsync()
        {
            int gamesCount = 0;
            int playersCount = 0;
            foreach (var game in _games)
            {
                if (game.Value.Game.Players.Count == 0)
                {
                    _games.Remove(game.Key, out _);
                    gamesCount++;
                }
            }
            foreach (var player in _players)
            {
                if (!_games.Values.Select(x => x.Game.Players).Any(x => x.Contains(player.Value)))
                {
                    _players.Remove(player.Key, out _);
                    playersCount++;
                }
            }
            return await Task.FromResult((gamesCount, playersCount));
        }
    }
}
