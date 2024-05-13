
using BobsBetting.CacheModels;
using BobsBetting.Calculate;

namespace BobsBetting.GameMechanics;

public class HandleGameEnd() {
    static Dictionary<int, string> handGradesDic = new Dictionary<int, string> {
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


        public static List<Tuple<int, string>> PickWinner(List<Player> players, List<Card> communityCards) {
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