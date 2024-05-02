using System.Formats.Asn1;

namespace BobsBetting.Calculate {
    using BobsBetting.CacheModels;

    public class HandGrade() {
        public static int CalcHand(List<Card> hand, List<Card> CardsOnTable) {
            List<Card> cards = new(CardsOnTable);
            cards.AddRange(hand);
            cards = [.. cards.OrderBy(c => c.Rank)];

            var suiteGroups = cards.GroupBy(card => card.Suit)
                            .Select(group => new { Suite = group.Key, Count = group.Count() })
                            .ToList();

            var groups = cards.GroupBy(card => card.Rank)
                            .Select(group => new { Rank = group.Key, Count = group.Count() })
                            .ToList();
            var counts = groups.OrderByDescending(group => group.Count).ToList();

            // 9 - Straight flush
            if (suiteGroups.Any(c => c.Count >= 5) && IsStraight(cards)) {
                return 9;
            }
            // 8 - Four of a kind
            else if (counts.Any(g => g.Count == 4)) {
                return 8;
            }

            // 7 - Full House
            else if (counts.Any(g => g.Count == 3) && counts.Any(g => g.Count == 2)) {
                return 7;
            }

            // 6 - Flush
            else if (suiteGroups.Any(c => c.Count >= 5)) {
                return 6;
            }

            // 5 - Straight
            else if (IsStraight(cards)) {
                return 5;
            }

            // 4 - Three of a kind
            else if (counts.Any(g => g.Count == 3)) {
                return 4;
            }

            // 3 - Two Pairs
            else if (counts.Count(g => g.Count == 2) == 2) {
                return 3;
            }

            // 2 - Pair
            else if (counts.Any(g => g.Count == 2)) {
                return 2;
            }
            
            // 1 - High Card
            else return 1;
        }

        private static bool IsStraight(List<Card> hand) {
            List<int> cards = hand.Select(c => c.Rank).Distinct().ToList();
            int counter = 1;
            for (int i = 0; i < cards.Count - 1; i++) {
                if (counter == 5) {
                    return true;
                }

                if (cards[i + 1] != cards[i] + 1) {
                    counter = 1;
                } else {
                    counter++;
                }
            }
            if (counter < 5) {
                return false;
            }
            return true;
        }
    }
}