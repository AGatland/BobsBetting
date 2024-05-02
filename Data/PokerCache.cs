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
        public List<Card> CommunityCards { get; set; } = [];
        public List<Player> PlayerStates { get; set; } = [];
    }

    public class Player
    {
        public int UserId { get; set;}
        public int CurrentBet { get; set;}
        public List<Card> Hand { get; set;} = [];
        public bool IsFolded { get; set;}

        internal void ReceiveCard(Card card)
        {
            Hand.Add(card);
        }

        internal void ClearHand()
        {
            Hand.Clear();
        }
    }

    public class Card(int rank, string suit)
    {
        public int Rank { get; set;} = rank;
        public string Suit { get; set;} = suit;
    }

}