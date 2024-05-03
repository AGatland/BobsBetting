namespace BobsBetting.Services {
    using BobsBetting.Calculate;
    using BobsBetting.CacheModels;
    using BobsBetting.DBModels;
    using System.Data.Common;

    public class PokerGameService(DeckService deckService, GameCacheService gameCacheService)
    {
        private readonly DeckService _deckService = deckService;
        private readonly GameCacheService _gameCacheService = gameCacheService;

        public void StartGame(Lobby lobby)
        {
            var deck = _deckService.CreateAndShuffleDeck();
            _gameCacheService.SetGameState(lobby.LobbyId, DealCards(lobby.Users, deck));
            
        }

        private ActiveGameState DealCards(List<User> users, List<Card> deck)
        {
            List<Player> players = [];
            foreach (User user in users)
            {
                players.Add(new Player(user.Id));
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
            return new ActiveGameState(players, communityCards);
        }

        public Tuple<List<Player>, int> SettleGame(int gameId) {
            // Get game data from MEM DB
            ActiveGameState activeGameState = _gameCacheService.GetGameState(gameId);
            List<Player> players = activeGameState.PlayerStates.Where(p => !p.IsFolded).ToList();
            List<Card> communityCards = activeGameState.CommunityCards;

            List<Player> winners = PickWinner(players, communityCards);

            // 1 token might be lost?
            int winnings = activeGameState.PlayerStates.Select(p => p.CurrentBet).Sum()/winners.Count;

            return new Tuple<List<Player>, int> (winners, winnings);
        }

        public List<Player> PickWinner(List<Player> players, List<Card> communityCards) {
            List<Tuple<Player, int>> handGrades = [];
            foreach (Player player in players) {
                var handGrade = HandGrade.CalcHand(player.Hand, communityCards);
                handGrades.Add(new Tuple<Player, int>(player, handGrade));
            }
            var maxScore = handGrades.Select(i => i.Item2).Max();
            var competitors = handGrades.Where(hands => hands.Item2 == maxScore).Select(h => h.Item1).ToList();

            if (competitors.Count == 1) {
                return competitors;
            }

            var playersComp = competitors.Select(player => new PlayerHandDetails(player, communityCards)).ToList();

            // Calculate winner when more than one got the top hand grade
            switch(maxScore) {
                case 9:
                    //Console.WriteLine("Straight Flush");
                    return HandCompare.CompareStraightFlushHands(playersComp);
                case 8:
                    //Console.WriteLine("Four of a kind");
                    return HandCompare.CompareFourOfAKindHands(playersComp);
                case 7:
                    //Console.WriteLine("Full House");
                    return HandCompare.CompareFullHouseHands(playersComp);
                case 6:
                    //Console.WriteLine("Flush");
                    return HandCompare.CompareFlushHands(playersComp);
                case 5:
                    //Console.WriteLine("Straight");
                    return HandCompare.CompareStraightHands(playersComp);
                case 4:
                    //Console.WriteLine("Three of a kind");
                    return HandCompare.CompareThreeOfAKindHands(playersComp);
                case 3:
                    //Console.WriteLine("Two Pairs");
                    return HandCompare.CompareTwoPairsHands(playersComp);
                case 2:
                    //Console.WriteLine("Pair");
                    return HandCompare.ComparePairHands(playersComp);
                case 1:
                    //Console.WriteLine("High Card");
                    return HandCompare.CompareHighCardHands(playersComp);
                default:
                    //Console.WriteLine("No Winner? This is an error");
                    return [];
            }
        }

        
    }
}