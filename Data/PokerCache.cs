using BobsBetting.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

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
        public List<LobbyUser> Users { get; set; } = [];
    }

    public class LobbyUser(string connectionId, int userId)
    {
        public string ConnectionId { get; set; } = connectionId;
        public int UserId { get; set; } = userId;
    }

    public class ActiveGameState(List<Player> players, List<PublicPlayer> publicPlayers, List<Card> communityCards, int currentPlayerId)
    {
        //public int GameId { get; set; }
        public GameRounds CurrentRound { get; set; } = GameRounds.PREFLOP;
        public int CurrentPlayerId { get; set; } = currentPlayerId;
        public int CurrentPot { get; set; } = 0;
        public List<Card> CommunityCards { get; set; } = communityCards;
        public List<Player> PlayerStates { get; set; } = players;
        public List<PublicPlayer> PublicPlayerStates { get; set; } = publicPlayers;
        public bool GameEnded { get; set; } = false;
    }

    public class Player(int userId)
    {

        public int UserId { get; set;} = userId;
        public List<Card> Hand { get; set;} = [];
    }

    public class PublicPlayer(int userId, string username, int turnNumber)
    {
        public int UserId { get; set;} = userId;
        public int TurnNumber { get; set;} = turnNumber;
        public string Username { get; set; } = username;
        public int CurrentBet { get; set;} = 0;
        public PlayerAction LastAction { get; set; } = null;
        public bool IsFolded { get; set;} = false;
    }

    public class WinnerData(string username, int winnings, string handGrade)
    {
        public string Username { get; set;} = username;
        public int Winnings { get; set;} = winnings;
        public string HandGrade { get; set; } = handGrade;
    }

    public class Card(int rank, string suit)
    {
        public int Rank { get; set;} = rank;
        public string Suit { get; set;} = suit;
    }

    public enum ActionType
    {
        Fold,
        Call,
        Raise,
        AllIn
    }

    public class PlayerAction(ActionType actionType, int amount)
    {
        public ActionType ActionType { get; set; } = actionType;
        public int Amount { get; set; } = amount; // Amount is relevant for Call, Raise, and AllIn
    }

        public class PlayerActionReq(int actionType, int amount)
    {
        public int ActionType { get; set; } = actionType;
        public int Amount { get; set; } = amount; // Amount is relevant for Call, Raise, and AllIn
    }

}