namespace BobsBetting.Services {
    using BobsBetting.Calculate;
    using BobsBetting.CacheModels;

    public class PokerGameService(List<Player> players, DeckService deck)
    {
        public List<Player> Players { get; set; } = players;
        public DeckService Deck { get; set; } = deck;
        public List<Card> TableCards { get; set; } = [];

        public void RunGame() {
            DealCards();
            PickWinner();
        }

        public void DealCards() {
            foreach (Player player in Players) {
                for (int i = 0; i < 2; i++)
                player.ReceiveCard(Deck.Deal());
            }
            
            for (int i = 0; i < 5; i++) {
                TableCards.Add(Deck.Deal());
            }
        }

        public List<Player> PickWinner() {
            List<Tuple<Player, int>> handGrades = [];
            foreach (Player player in Players) {
                var handGrade = HandGrade.CalcHand(player.Hand, TableCards);
                handGrades.Add(new Tuple<Player, int>(player, handGrade));
            }
            var maxScore = handGrades.Select(i => i.Item2).Max();
            var competitors = handGrades.Where(hands => hands.Item2 == maxScore).Select(h => h.Item1).ToList();

            if (competitors.Count == 1) {
                return competitors;
            }

            var playersComp = competitors.Select(player => new PlayerHandDetails(player, TableCards)).ToList();

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