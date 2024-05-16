using Microsoft.Extensions.Caching.Memory;

namespace BobsBetting.Services
{
    using BobsBetting.CacheModels;

    public class GameCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public GameCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public ActiveGameState GetGameState(string gameId)
        {
            _memoryCache.TryGetValue($"Game_{gameId}", out ActiveGameState gameState);
            return gameState;
        }

        public void SetGameState(string gameId, ActiveGameState gameState)
        {
            _memoryCache.Set($"Game_{gameId}", gameState, TimeSpan.FromMinutes(20));  // Cache for 20 min
        }

        public Lobby GetLobbyState(string lobbyId)
        {
            _memoryCache.TryGetValue($"Lobby_{lobbyId}", out Lobby lobbyState);
            return lobbyState;
        }

        public void SetLobbyState(string lobbyId, Lobby lobbyState)
        {
            _memoryCache.Set($"Lobby_{lobbyId}", lobbyState, TimeSpan.FromMinutes(20));  // Cache for 20 min
        }
    }
}