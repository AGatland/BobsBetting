namespace BobsBetting.Services {
    using BobsBetting.CacheModels;

    public class DeckService
    {
        public List<Card> Deck { get; set; } = [];
        readonly List<int> cardRanks = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];
        readonly List<string> cardSuits = ["Diamonds", "Spades", "Hearts", "Clubs"];

        public DeckService()
        {
            CreateDeck();
            Shuffle();
        }

        public void CreateDeck()
        {
            foreach (int cardRank in cardRanks)
            {
                foreach (string cardSuit in cardSuits)
                {
                    Deck.Add(new Card(cardRank, cardSuit));
                }
            }
        }

        public void Shuffle()
        {
            Random rng = new();
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (Deck[n], Deck[k]) = (Deck[k], Deck[n]);
            }
        }

        public Card Deal() 
        {
            if (Deck.Count == 0)
            {
                return new Card(0, string.Empty); // Return an empty CardItem if the deck is empty
            }
            else
            {
                // Deal from the top of the deck
                Card card = Deck[0]; // Access the first item
                Deck.RemoveAt(0); // Remove the card from the deck
                return card; // Return the dealt card
            }
        }
    }
}