namespace BobsBetting.Services {
    using BobsBetting.CacheModels;

    public class DeckService
    {
        readonly List<int> cardRanks = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];
        readonly List<string> cardSuits = ["Diamonds", "Spades", "Hearts", "Clubs"];
        
        public DeckService()
        {
        }

        public List<Card> CreateAndShuffleDeck()
        {
            List<Card> deck = new List<Card>();
            foreach (int cardRank in cardRanks)
            {
                foreach (string cardSuit in cardSuits)
                {
                    deck.Add(new Card(cardRank, cardSuit));
                }
            }

            Shuffle(deck);
            return deck;
        }

        private void Shuffle(List<Card> deck)
        {
            Random rng = new Random();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (deck[n], deck[k]) = (deck[k], deck[n]);
            }
        }
    }
}