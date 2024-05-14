namespace BobsBetting.Services {
    using BobsBetting.GameMechanics;
    using BobsBetting.CacheModels;
    using BobsBetting.DBModels;

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

        public ActiveGameState HandleTurn(string LobbyId, User user, PlayerActionReq action) {
            ActiveGameState gameState = _gameCacheService.GetGameState(LobbyId);
            if (gameState.CurrentPlayerId != gameState.PublicPlayerStates.Find(p => p.UserId == user.Id).TurnNumber || (gameState.GameEnded && !gameState.PublicPlayerStates.Any(p => p.LastAction.ActionType == ActionType.Raise || p.LastAction.ActionType == ActionType.AllIn)) || (gameState.GameEnded &&(action.ActionType == 2 || action.ActionType == 3))) {
                return new([],[],[],0);
            }
            switch (action.ActionType)
                    {
                        case (int) ActionType.Fold:
                            gameState = HandlePlayerAction.HandleFold(user.Id, gameState);
                            break;
                        case (int) ActionType.Call:
                            gameState = HandlePlayerAction.HandleBetAndCall(user.Id, 0, user, ActionType.Call, gameState);
                            break;
                        case (int) ActionType.Raise:
                            gameState = HandlePlayerAction.HandleBetAndCall(user.Id, action.Amount, user, ActionType.Raise, gameState);
                            break;
                        case (int) ActionType.AllIn:
                            gameState = HandlePlayerAction.HandleAllIn(user.Id, user, gameState);
                            break;
                        default:
                            Console.WriteLine("Action not found");
                            break;
                    }

            gameState = HandlePlayerAction.HandleTurnChange(gameState);
            _gameCacheService.SetGameState(LobbyId, gameState);
            return gameState;
        }

        public List<WinnerData> SettleGame(string gameId) {
            // Get game data from MEM DB
            ActiveGameState activeGameState = _gameCacheService.GetGameState(gameId);
            List<PublicPlayer> publicPlayers = activeGameState.PublicPlayerStates.Where(p => !p.IsFolded).ToList();
            List<Player> players = activeGameState.PlayerStates.Where(p => publicPlayers.Select(pp => pp.UserId).Contains(p.UserId)).ToList();
            List<Card> communityCards = activeGameState.CommunityCards;

            List<Tuple<int, string>> winners = HandleGameEnd.PickWinner(players, communityCards);

            // some tokens might be lost
            int winnings = activeGameState.PublicPlayerStates.Select(p => p.CurrentBet).Sum() / winners.Count;

            //TODO: Need to persistenly save winning data.
            List<WinnerData> winnerDatas = [];
            foreach (Tuple<int, string> winner in winners) {
                winnerDatas.Add(new WinnerData(winner.Item1, publicPlayers.Find(p => p.UserId == winner.Item1).Username, winnings, winner.Item2));
            }

            return winnerDatas;
        }
        
    }
}