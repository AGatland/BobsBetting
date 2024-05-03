using Microsoft.EntityFrameworkCore;

namespace BobsBetting.CacheModels 
{
    public enum GameRounds {
        PREFLOP,
        FLOP,
        TURN,
        RIVER
    }

    public class ActiveGameState(List<Player> players, List<Card> communityCards)
    {
        public int GameId { get; set; }
        public GameRounds CurrentRound { get; set; } = GameRounds.PREFLOP;
        public int CurrentPlayerId { get; set; } = 0;
        public int PotSize { get; set; } = 0;
        public List<Card> CommunityCards { get; set; } = communityCards;
        public List<Player> PlayerStates { get; set; } = players;
    }

    public class Player(int userId)
    {

        public int UserId { get; set;} = userId;
        public int CurrentBet { get; set;} = 0;
        public List<Card> Hand { get; set;} = [];
        public bool IsFolded { get; set;} = false;
    }

    public class Card(int rank, string suit)
    {
        public int Rank { get; set;} = rank;
        public string Suit { get; set;} = suit;
    }

}