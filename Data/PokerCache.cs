using BobsBetting.DBModels;
using Microsoft.EntityFrameworkCore;

namespace BobsBetting.CacheModels 
{
    public enum GameRounds {
        PREFLOP,
        FLOP,
        TURN,
        RIVER
    }

    public class Lobby
    {
        public int LobbyId { get; set; } = DateTime.Now.Ticks.GetHashCode();
        public int LobbyLeader { get; set; }
        public List<User> Users { get; set; } = [];
    }

    public class ActiveGameState(List<Player> players, List<Card> communityCards)
    {
        public int GameId { get; set; }
        public GameRounds CurrentRound { get; set; } = GameRounds.PREFLOP;
        public int CurrentPlayerId { get; set; } = 0;
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