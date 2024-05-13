namespace BobsBetting.Services {
    using BobsBetting.Calculate;
    using BobsBetting.CacheModels;
    using BobsBetting.DBModels;
    using System.Data.Common;
    using System.Diagnostics.Eventing.Reader;

    public class PokerGameService(DeckService deckService, GameCacheService gameCacheService)
    {
        private readonly DeckService _deckService = deckService;
        private readonly GameCacheService _gameCacheService = gameCacheService;

        public ActiveGameState StartPokerGame(string lobby, List<User> users)
        {
            var deck = _deckService.CreateAndShuffleDeck();
            ActiveGameState activeGameState = DealCards(users, deck);
            _gameCacheService.SetGameState(lobby, activeGameState);
            
            return activeGameState;
        }

        private ActiveGameState DealCards(List<User> users, List<Card> deck)
        {
            List<Player> players = [];
            List<PublicPlayer> publicPlayers = [];
            for (int i = 0; i < users.Count; i++)
            {
                players.Add(new Player(users[i].Id));
                publicPlayers.Add(new PublicPlayer(users[i].Id, users[i].Username, i));
            }
            foreach (Player player in players)
            {
                if (deck.Count >= 2)
                    player.Hand.Add(deck[0]);  // Deal one card
                    player.Hand.Add(deck[1]);  // Deal Second card
                    deck.RemoveAt(0);  // Remove the dealt card
                    deck.RemoveAt(0);  // Remove the dealt card
            }
            List<Card> communityCards = [];
            if (deck.Count >= 5) {
                for (int i = 0; i < 5; i++) {
                    communityCards.Add(deck[0]);
                    deck.RemoveAt(0);
                }
            }
            return new ActiveGameState(players, publicPlayers, communityCards, 0);
        }

        public ActiveGameState HandleBetAndCall(string LobbyId, int UserId, int Amount, User user, ActionType action, ActiveGameState activeGameState) {

            int currentHighBid = activeGameState.PublicPlayerStates.Select(p => p.CurrentBet).Max();

            PublicPlayer publicPlayerState = activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId);
            if (user.Chips - (currentHighBid + Amount) < 0) {
                activeGameState = HandleBetHelper(UserId, user.Chips - publicPlayerState.CurrentBet, activeGameState, action);
            } else {
                activeGameState = HandleBetHelper(UserId, currentHighBid + Amount - publicPlayerState.CurrentBet, activeGameState, action);
            }
            activeGameState = HandleTurnChange(activeGameState);
            _gameCacheService.SetGameState(LobbyId, activeGameState);
            return activeGameState;
        }

        public ActiveGameState HandleBetHelper(int UserId, int Amount, ActiveGameState activeGameState, ActionType action) {
            activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).LastAction = new(action, Amount);
            activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).CurrentBet += Amount;
            return activeGameState;
        }

        public ActiveGameState HandleAllIn(string LobbyId, int UserId, User user, ActiveGameState activeGameState) {

            PublicPlayer publicPlayerState = activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId);

            activeGameState = HandleBetHelper(UserId, user.Chips - publicPlayerState.CurrentBet, activeGameState, ActionType.AllIn);
            activeGameState = HandleTurnChange(activeGameState);
            _gameCacheService.SetGameState(LobbyId, activeGameState);
            return activeGameState;
        }

        public ActiveGameState HandleFold(string LobbyId, int UserId, ActiveGameState activeGameState) {

            activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).IsFolded = true;
            activeGameState.PublicPlayerStates.Find(p => p.UserId == UserId).LastAction = new(ActionType.Fold, 0);

            activeGameState = HandleTurnChange(activeGameState);
            _gameCacheService.SetGameState(LobbyId, activeGameState);
            return activeGameState;
        }
public ActiveGameState HandleTurn(string LobbyId, User user, PlayerActionReq action) {
    ActiveGameState gameState = _gameCacheService.GetGameState(LobbyId);
    if (gameState.CurrentPlayerId != gameState.PublicPlayerStates.Find(p => p.UserId == user.Id).TurnNumber || gameState.GameEnded) {
        return new([],[],[],0);
    }
    switch (action.ActionType)
            {
                case (int) ActionType.Fold:
                    gameState = HandleFold(LobbyId, user.Id, gameState);
                    break;
                case (int) ActionType.Call:
                    gameState = HandleBetAndCall(LobbyId, user.Id, 0, user, ActionType.Call, gameState);
                    break;
                case (int) ActionType.Raise:
                    gameState = HandleBetAndCall(LobbyId, user.Id, action.Amount, user, ActionType.Raise, gameState);
                    break;
                case (int) ActionType.AllIn:
                    gameState = HandleAllIn(LobbyId, user.Id, user, gameState);
                    break;
                default:
                    Console.WriteLine("Action not found");
                    break;
            }
    return gameState;
}
public ActiveGameState HandleTurnChange(ActiveGameState activeGameState) {
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
            return activeGameState;
        } else {
            activeGameState.CurrentRound++; // Moves to the next round enum value
        }
    }

    // Set the next player as the current player unless the game ends
    activeGameState.CurrentPlayerId = nextPlayer.TurnNumber;

    return activeGameState;
}

        public List<WinnerData> SettleGame(string gameId) {
            // Get game data from MEM DB
            ActiveGameState activeGameState = _gameCacheService.GetGameState(gameId);
            List<PublicPlayer> publicPlayers = activeGameState.PublicPlayerStates.Where(p => !p.IsFolded).ToList();
            List<Player> players = activeGameState.PlayerStates.Where(p => publicPlayers.Select(pp => pp.UserId).Contains(p.UserId)).ToList();
            List<Card> communityCards = activeGameState.CommunityCards;

            List<Tuple<int, string>> winners = PickWinner(players, communityCards);

            // some tokens might be lost
            int winnings = activeGameState.PublicPlayerStates.Select(p => p.CurrentBet).Sum() / winners.Count;

            //TODO: Need to persistenly save winning data.
            List<WinnerData> winnerDatas = [];
            foreach (Tuple<int, string> winner in winners) {
                winnerDatas.Add(new WinnerData(publicPlayers.Find(p => p.UserId == winner.Item1).Username, winnings, winner.Item2));
            }

            return winnerDatas;
        }

        Dictionary<int, string> handGradesDic = new Dictionary<int, string> {
            { 1, "High Card" },
            { 2, "Pair" },
            { 3, "Two Pairs" },
            { 4, "Three Of A Kind" },
            { 5, "Straight" },
            { 6, "Flush" },
            { 7, "Full House" },
            { 8, "Four Of A Kind" },
            { 9, "Straight Flush" },
        };


        public List<Tuple<int, string>> PickWinner(List<Player> players, List<Card> communityCards) {
            List<Tuple<Player, int>> handGrades = [];
            foreach (Player player in players) {
                var handGrade = HandGrade.CalcHand(player.Hand, communityCards);
                handGrades.Add(new Tuple<Player, int>(player, handGrade));
            }
            var maxScore = handGrades.Select(i => i.Item2).Max();
            var competitors = handGrades.Where(hands => hands.Item2 == maxScore).Select(h => h.Item1).ToList();
            
            List<Tuple<int, string>> winnerTuples = [];
            if (competitors.Count == 1) {
                winnerTuples.Add(new(competitors[0].UserId, handGradesDic[maxScore]));
                return winnerTuples;
            }

            var playersComp = competitors.Select(player => new PlayerHandDetails(player, communityCards)).ToList();

            List<Player> winners;
            // Calculate winner when more than one got the top hand grade
            switch(maxScore) {
                case 9:
                    //Console.WriteLine("Straight Flush");
                    winners = HandCompare.CompareStraightFlushHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 8:
                    //Console.WriteLine("Four of a kind");
                    winners = HandCompare.CompareFourOfAKindHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 7:
                    //Console.WriteLine("Full House");
                    winners = HandCompare.CompareFullHouseHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 6:
                    //Console.WriteLine("Flush");
                    winners = HandCompare.CompareFlushHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 5:
                    //Console.WriteLine("Straight");
                    winners = HandCompare.CompareStraightHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 4:
                    //Console.WriteLine("Three of a kind");
                    winners = HandCompare.CompareThreeOfAKindHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 3:
                    //Console.WriteLine("Two Pairs");
                    winners = HandCompare.CompareTwoPairsHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 2:
                    //Console.WriteLine("Pair");
                    winners = HandCompare.ComparePairHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                case 1:
                    //Console.WriteLine("High Card");
                    winners = HandCompare.CompareHighCardHands(playersComp);
                    foreach (Player player in winners) {
                        winnerTuples.Add(new(player.UserId, handGradesDic[maxScore]));
                    }
                    return winnerTuples;
                default:
                    //Console.WriteLine("No Winner? This is an error");
                    return [];
            }
        }

        
    }
}