using Microsoft.EntityFrameworkCore;

namespace BobsBetting.CacheModels 
{
    public enum GameRounds {
        PREFLOP,
        FLOP,
        TURN,
        RIVER
    }

    public class ActiveGameState
    {
        public int GameId { get; set; }
        public GameRounds CurrentRound { get; set; }
        public int CurrentPlayerId { get; set; }
        public int PotSize { get; set; }
        public List<Card> CommunityCards { get; set; }
        public List<PlayerState> PlayerStates { get; set; }
    }

    public class PlayerState
    {
        public int UserId { get; set;}
        public int CurrentBet { get; set;}
        public List<Card> HandCards { get; set;}
        public bool IsFolded { get; set;}
    }

    public class Card
    {
        public int Rank { get; set;}
        public string Suit { get; set;}
    }

}