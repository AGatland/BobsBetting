using Microsoft.Extensions.Caching.Memory;
using System;

namespace BobsBetting.Services
{
    using BobsBetting.CacheModels;
    using BobsBetting.DBModels;

    public class GameCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public GameCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public ActiveGameState GetGameState(int gameId)
        {
            _memoryCache.TryGetValue($"Game_{gameId}", out ActiveGameState gameState);
            return gameState;
        }

        public void SetGameState(int gameId, ActiveGameState gameState)
        {
            _memoryCache.Set($"Game_{gameId}", gameState, TimeSpan.FromHours(1));  // Cache for 1 hour
        }

        public Lobby GetLobbyState(int lobbyId)
        {
            _memoryCache.TryGetValue($"Lobby_{lobbyId}", out Lobby lobbyState);
            return lobbyState;
        }

        public void SetLobbyState(int lobbyId, Lobby lobbyState)
        {
            _memoryCache.Set($"Lobby_{lobbyId}", lobbyState, TimeSpan.FromHours(1));  // Cache for 1 hour
        }
    }
}