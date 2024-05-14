
using BobsBetting.CacheModels;
using BobsBetting.DBModels;

namespace BobsBetting.GameMechanics;

public class HandlePlayerAction() 
{
    
    public static ActiveGameState HandleBetAndCall(int UserId, int Amount, User user, ActionType action, ActiveGameState activeGameState) 
    {
        int currentHighBid = activeGameState.PublicPlayerStates.Select(p => p.CurrentBet).Max();

        PublicPlayer publicPlayerState = activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId);
        if (user.Chips - (currentHighBid + Amount) < 0) {
            activeGameState = HandleBetHelper(UserId, user.Chips - publicPlayerState.CurrentBet, activeGameState, action);
        } else {
            activeGameState = HandleBetHelper(UserId, currentHighBid + Amount - publicPlayerState.CurrentBet, activeGameState, action);
        }
        return activeGameState;
    }

    public static ActiveGameState HandleBetHelper(int UserId, int Amount, ActiveGameState activeGameState, ActionType action) {
        activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).LastAction = new(action, Amount);
        activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).CurrentBet += Amount;
        return activeGameState;
    }

    public static ActiveGameState HandleAllIn(int UserId, User user, ActiveGameState activeGameState) 
    {
        PublicPlayer publicPlayerState = activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId);

        activeGameState = HandleBetHelper(UserId, user.Chips - publicPlayerState.CurrentBet, activeGameState, ActionType.AllIn);

        return activeGameState;
    }

    public static ActiveGameState HandleFold(int UserId, ActiveGameState activeGameState) 
    {
        activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).IsFolded = true;
        activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).LastAction = new(ActionType.Fold, 0);
        return activeGameState;
    }

    public static ActiveGameState HandleTurnChange(ActiveGameState activeGameState) {
        var players = activeGameState.PublicPlayerStates;
        int currentPlayerIndex = players.FindIndex(p => p.TurnNumber == activeGameState.CurrentPlayerId);
        int countChecked = 0;

        // Determine the next player index
        PublicPlayer nextPlayer = null;
        do {
        currentPlayerIndex++;
        int nextPlayerIndex = currentPlayerIndex % players.Count;
        nextPlayer = players[nextPlayerIndex];
        countChecked++;
        } while(nextPlayer.IsFolded && countChecked < players.Count);

        // Update the current pot
        activeGameState.CurrentPot = players.Sum(p => p.CurrentBet);

        if (countChecked >= players.Count || players.Where(p => !p.IsFolded).ToList().Count == 1) {
            Console.WriteLine("Game end: All players except one is folded");
            activeGameState.GameEnded = true;
            return activeGameState;
        }

        // Check if we need to move to the next round
        if (nextPlayer.TurnNumber <= activeGameState.CurrentPlayerId) {
            if (activeGameState.CurrentRound == GameRounds.RIVER) {
                Console.WriteLine("Game end: River complete");
                activeGameState.GameEnded = true;
                if (activeGameState.GameEnded && !activeGameState.PublicPlayerStates.Any(p => p.LastAction.ActionType == ActionType.Raise || p.LastAction.ActionType == ActionType.AllIn)) {
                    return activeGameState;
                }
            } else {
                activeGameState.CurrentRound++; // Moves to the next round enum value
            }
        }

        // Set the next player as the current player unless the game ends
        activeGameState.CurrentPlayerId = nextPlayer.TurnNumber;

        return activeGameState;
    }
}