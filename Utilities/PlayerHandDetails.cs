namespace BobsBetting.Calculate {

    using BobsBetting.CacheModels;

    public class PlayerHandDetails
    {
        public Player Player { get; set; }
        public List<Card> CombinedHand { get; set; }
        public List<Card> Hand { get; set; }

        public PlayerHandDetails(Player player, List<Card> tableCards)
        {
            Player = player;
            Hand = player.Hand.OrderByDescending(card => card.Rank).ToList();
            CombinedHand = new List<Card>(player.Hand);
            CombinedHand.AddRange(tableCards);
            CombinedHand = CombinedHand.OrderByDescending(card => card.Rank).ToList();
        }
    }
}