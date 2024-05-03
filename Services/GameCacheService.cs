using Microsoft.Extensions.Caching.Memory;
using System;

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

        public ActiveGameState GetGameState(int gameId)
        {
            _memoryCache.TryGetValue($"Game_{gameId}", out ActiveGameState gameState);
            return gameState;
        }

        public void SetGameState(int gameId, ActiveGameState gameState)
        {
            _memoryCache.Set($"Game_{gameId}", gameState, TimeSpan.FromHours(1));  // Cache for 1 hour
        }
    }
}