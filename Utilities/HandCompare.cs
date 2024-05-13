namespace BobsBetting.Calculate
{
    using BobsBetting.CacheModels;

    public class HandCompare() {
        public static List<Player> CompareHighCardHands(List<PlayerHandDetails> players)
        {

            int cardCount = players[0].Hand.Count;  // Assume all hands have the same number of cards

            for (int i = 0; i < cardCount; i++) {
                var highestCard = players.Max(player => player.Hand[i].Rank); // Compare based on card value
                var highestCardPlayers = players.Where(player => player.Hand[i].Rank == highestCard).ToList();

                if (highestCardPlayers.Count == 1) {
                    return [highestCardPlayers[0].Player]; // Return single winner
                }

                // If tied, continue to the next card
                players = highestCardPlayers;
            }

            // If still tied after all cards, return all tied players
            return players.Select(player => player.Player).ToList();
        }

        public static List<Player> CompareFlushHands(List<PlayerHandDetails> players)
        {
            var playerHands = players.Select(player => new
            {
                player.Player,
                FlushCards = player.CombinedHand.Where(
                        c => c.Suit == player.CombinedHand.GroupBy(g => g.Suit).Select(g => g.Key).Where(g => g.Length >= 5).First()
                    ).OrderByDescending(c => c.Rank).ToList()
                
                ,
            }).ToList();

            for (int i = 0; i < 5; i++) {
                var highestCard = playerHands.Max(player => player.FlushCards[i].Rank); // Compare based on card value
                var highestCardPlayers = playerHands.Where(player => player.FlushCards[i].Rank == highestCard).ToList();

                if (highestCardPlayers.Count == 1) {
                    return [highestCardPlayers[0].Player]; // Return single winner
                }

                // If tied, continue to the next card
                playerHands = highestCardPlayers;
            }

            // If still tied after all cards, return all tied players
            return playerHands.Select(player => player.Player).ToList();
        }

        public static List<Player> ComparePairHands(List<PlayerHandDetails> players)
        {

            // Extract pairs and kickers for each player
            var playerHands = players.Select(player => new
            {
                player.Player,
                Pair = player.CombinedHand
                            .GroupBy(card => card.Rank)
                            .Where(group => group.Count() == 2)
                            .Select(group => group.Key)
                            .OrderByDescending(value => value)
                            .First(),
                Kickers = player.CombinedHand
                            .GroupBy(card => card.Rank)
                            .Where(group => group.Count() == 1)
                            .Select(group => group.Key)
                            .OrderByDescending(value => value)
                            .ToList()
            }).ToList();

            // Determine the highest pair value
            var highestPair = playerHands.Max(hand => hand.Pair);

            // Filter players with the highest pair
            var contenders = playerHands.Where(hand => hand.Pair == highestPair).ToList();

            if (contenders.Count == 1)
            {
                return [contenders[0].Player];
            }

            // Loop through each kicker position
            for (int i = 0; i < 3; i++) {
                var highestKicker = contenders.Max(hand => hand.Kickers.ElementAtOrDefault(i));
                contenders = contenders.Where(hand => hand.Kickers.ElementAtOrDefault(i) == highestKicker).ToList();

                if (contenders.Count == 1) {
                    return [contenders[0].Player];
                }
            }

            // If still tied after all kickers, return all tied players
            return contenders.Select(hand => hand.Player).ToList();
        }


        // TODO: If there are three pairs total, the logic breaks down :(
        public static List<Player> CompareTwoPairsHands(List<PlayerHandDetails> players)
        {

            // Extract pairs and kickers for each player
            var playerHands = players.Select(player => new
            {
                player.Player,
                Pairs = player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() == 2)
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .ToList(),
                Kickers = player.CombinedHand
                            .GroupBy(card => card.Rank)
                            .Where(group => group.Count() == 1)
                            .Select(group => group.Key)
                            .OrderByDescending(value => value)
                            .ToList()
            }).ToList();

            // Determine the highest pair value
            var highestPair = playerHands.Max(hand => hand.Pairs[0]);

            // Filter players with the highest first pair
            var contenders = playerHands.Where(hand => hand.Pairs[0] == highestPair).ToList();

            if (contenders.Count == 1)
            {
                return [contenders[0].Player];
            }

            // Determine the highest second pair among contenders
            var highestSecondPair = contenders.Max(hand => hand.Pairs[1]);

            // Filter players with the highest second pair
            contenders = contenders.Where(hand => hand.Pairs[1] == highestSecondPair).ToList();

            if (contenders.Count == 1)
            {
                return [contenders[0].Player];
            }

            // Determine highest kicker, and filter players
            var highestKicker = contenders.Max(hand => hand.Kickers.First());
            contenders = contenders.Where(hand => hand.Kickers.First() == highestKicker).ToList();

            // If still tied after kicker, return all tied players
            return contenders.Select(hand => hand.Player).ToList();
        }

        public static List<Player> CompareFourOfAKindHands(List<PlayerHandDetails> players)
        {
            // Extract pairs and kickers for each player
            var playerHands = players.Select(player => new
            {
                player.Player,
                Quad = player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() == 4)
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .First(),
                Kicker = player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() != 4)
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .First(),
            }).ToList();

            int highestQuad = playerHands.Max(p => p.Quad);

            var contenders = playerHands.Where(player => 
                player.Quad == highestQuad)
                .ToList();
            
            if (contenders.Count == 1) {
                return [contenders[0].Player];
            }

            var highestKicker = contenders.Max(hand => hand.Kicker);
            contenders = contenders.Where(hand => hand.Kicker == highestKicker).ToList();

            // If still tied after all kickers, return all tied players
            return contenders.Select(hand => hand.Player).ToList();
        }

        public static List<Player> CompareThreeOfAKindHands(List<PlayerHandDetails> players)
        {
            // Extract pairs and kickers for each player
            var playerHands = players.Select(player => new
            {
                player.Player,
                Tripple = player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() == 3)
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .First(),
                Kickers = player.CombinedHand
                                .Where(card => card.Rank != player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() == 3)
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .First())
                                .OrderByDescending(card => card.Rank).Select(card => card.Rank)
                                .ToList()
            }).ToList();

            int highestThree = playerHands.Max(hand => hand.Tripple);

            var contenders = playerHands.Where(player => 
                player.Tripple == highestThree)
                .ToList();
            
            if (contenders.Count == 1) {
                return contenders.Select(hand => hand.Player).ToList();
            }

            // Loop through each kicker position
            for (int i = 0; i < 2; i++) {
                var highestKicker = contenders.Max(hand => hand.Kickers.ElementAtOrDefault(i));
                contenders = contenders.Where(hand => hand.Kickers.ElementAtOrDefault(i) == highestKicker).ToList();

                if (contenders.Count == 1) {
                    return [contenders[0].Player];
                }
            }

            // If still tied after all kickers, return all tied players
            return contenders.Select(hand => hand.Player).ToList();
        }

        public static List<Player> CompareFullHouseHands(List<PlayerHandDetails> players)
        {
            // Extract pairs and kickers for each player
            var playerHands = players.Select(player => new
            {
                player.Player,
                Tripple = player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() == 3)
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .First(),
                Double = player.CombinedHand
                                .GroupBy(card => card.Rank)
                                .Where(group => group.Count() >= 2)
                                .Where(group => group.Key != player.CombinedHand
                                    .GroupBy(card => card.Rank)
                                    .Where(group => group.Count() == 3)
                                    .Select(group => group.Key)
                                    .OrderByDescending(value => value)
                                    .First())
                                .Select(group => group.Key)
                                .OrderByDescending(value => value)
                                .First(),
            });

            int highestThree = playerHands.Max(hand => hand.Tripple);

            var contenders = playerHands.Where(player => 
                player.Tripple == highestThree)
                .ToList();

            int highestTwo = contenders.Max(hand => hand.Double);

            contenders = contenders.Where(player => 
                player.Double == highestTwo)
                .ToList();

            return contenders.Select(p => p.Player).ToList();
        }

        public static List<Player> CompareStraightHands(List<PlayerHandDetails> players)
        {
            var playerHands = players.Select(player => new
            {
                player.Player,
                StraightStrength = StraightStrength(player.CombinedHand),
            }).ToList();

            int highestStraight = playerHands.Max(p => p.StraightStrength);

            // If still tied after straight evaluation, return all tied players
            return playerHands.Where(player => 
                player.StraightStrength == highestStraight).Select(p => p.Player)
                .ToList();;
        }

        public static List<Player> CompareStraightFlushHands(List<PlayerHandDetails> players)
        {
            var playerHands = players.Select(player => new
            {
                player.Player,
                StraightStrength = StraightStrength(
                    player.CombinedHand.Where(c => c.Suit == player.CombinedHand.GroupBy(g => g.Suit).Select(g => g.Key).Where(g => g.Length >= 5).First()).ToList()
                
                ),
            }).ToList();

            int highestStraight = playerHands.Max(p => p.StraightStrength);

            // If still tied after straight evaluation, return all tied players
            return playerHands.Where(player => 
                player.StraightStrength == highestStraight).Select(p => p.Player)
                .ToList();;
        }

        private static int StraightStrength(List<Card> hand) {
            List<int> cards = hand.Select(c => c.Rank).Distinct().OrderByDescending(c => c).ToList();
            int counter = 1;
            for (int i = 0; i < cards.Count - 1; i++) {
                if (counter == 5) {
                    return cards[i-4];
                }

                if (cards[i + 1] != cards[i] - 1) {
                    counter = 1;
                } else {
                    counter++;
                }
            }
            if (counter < 5) {
                return 0;
            }
            return cards[^5];
        }
    }
}