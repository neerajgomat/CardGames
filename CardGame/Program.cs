using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(ConfigurationManager.AppSettings["EnterNumberOfPacks"]);

                var numberOfPacksConverted = Int16.TryParse(Console.ReadLine(), out Int16 numberOfPacks);
                if (!numberOfPacksConverted || numberOfPacks <= 0 || numberOfPacks > 100)
                {
                    Console.WriteLine(ConfigurationManager.AppSettings["InvalidNumberOfPacks"]);
                    return;
                }

                Console.WriteLine(ConfigurationManager.AppSettings["SelectMatchCondition"]);

                var selectedConditionConverted = Int16.TryParse(Console.ReadLine(), out Int16 selectedCondition);
                if (!selectedConditionConverted || selectedCondition <= 0 || selectedCondition > 3)
                {
                    Console.WriteLine(ConfigurationManager.AppSettings["WrongMatchCondition"]);
                    return;
                }

                if (!Enum.TryParse(selectedCondition.ToString(), out MatchCondition matchCondition))
                {
                    Console.WriteLine(ConfigurationManager.AppSettings["InvalidMatchCondition"]);
                    return;
                }

                //Add Cards
                List<string> cards = PrepareCards(numberOfPacks);

                //Check for selected cards value
                MatchStrategy strategy = MatchStrategyFactory.CreateStrategy(matchCondition, cards);

                //Run logic
                strategy.CheckLogic();

                //Get result
                Console.WriteLine(strategy.GetResult());

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - {ex.Message}");
            }
        }

        private static List<string> PrepareCards(int numberOfPacks)
        {
            List<string> cards = new List<string>();

            List<string> typeOfCards = new List<string> { "Hukum", "Diamonds", "Brick", "Chidi" };

            List<string> cardNumbers = new List<string> { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            //Add Cards
            cards = Enumerable.Range(0, numberOfPacks)
                    .AsParallel()
                    .SelectMany(_ =>
                        cardNumbers.SelectMany(cardNumber =>
                            typeOfCards.Select(typeOfCard => $"{cardNumber}-{typeOfCard}")
                        )
                    )
                    .ToList();

            // Shuffle the cards
            var random = new ThreadLocal<Random>(() => new Random());

            var shuffledCards = cards
                .AsParallel()
                .Select(card => new { Card = card, Key = random.Value.Next() })
                .OrderBy(x => x.Key)
                .Select(x => x.Card)
                .ToList();

            return shuffledCards;
        }
    }

    public abstract class MatchStrategy
    {
        public MatchStrategy(List<string> cards)
        {
            this.cards = cards;
        }

        protected List<string> cards = null;

        protected string previousValue = null;

        protected string currentPlayer = "Player 1";

        protected Dictionary<string, int> scores = new Dictionary<string, int>
                {
                    { ConfigurationManager.AppSettings["Player1"], 0 },
                    { ConfigurationManager.AppSettings["Player2"], 0 }
                };
        
        public abstract void CheckLogic();

        /// <summary>
        /// Check for result
        /// </summary>
        /// <returns></returns>
        public string GetResult()
        {
            if (scores[ConfigurationManager.AppSettings["Player1"]] > scores[ConfigurationManager.AppSettings["Player2"]])
                return $"{ConfigurationManager.AppSettings["Player1"]} score-{scores[ConfigurationManager.AppSettings["Player1"]]} and {ConfigurationManager.AppSettings["Player2"]} score-{scores[ConfigurationManager.AppSettings["Player2"]]} so {ConfigurationManager.AppSettings["Player1"]} wins with score : {scores[ConfigurationManager.AppSettings["Player1"]]}";
            else if (scores[ConfigurationManager.AppSettings["Player1"]] < scores[ConfigurationManager.AppSettings["Player2"]])
                return $"{ConfigurationManager.AppSettings["Player1"]} score-{scores[ConfigurationManager.AppSettings["Player1"]]} and {ConfigurationManager.AppSettings["Player2"]} score-{scores[ConfigurationManager.AppSettings["Player2"]]} so {ConfigurationManager.AppSettings["Player2"]} wins with score : {scores[ConfigurationManager.AppSettings["Player2"]]}";
            else if (scores[ConfigurationManager.AppSettings["Player1"]] == scores[ConfigurationManager.AppSettings["Player2"]])
                return $"{ConfigurationManager.AppSettings["Player1"]} score-{scores[ConfigurationManager.AppSettings["Player1"]]} and {ConfigurationManager.AppSettings["Player2"]} score-{scores[ConfigurationManager.AppSettings["Player2"]]} so Draw with score : {scores[ConfigurationManager.AppSettings["Player1"]]}";
            else
                return "No match";
        }     
    }

    public class MatchFaceValueStrategy : MatchStrategy
    {
        public MatchFaceValueStrategy(List<string> cards) : base(cards)
        {
        }
        public override void CheckLogic()
        {
            foreach (string card in cards)
            {
                string cardValue = card.Split('-')[0];

                if (previousValue != null && previousValue == cardValue)
                    scores[currentPlayer]++;

                previousValue = cardValue;
                currentPlayer = currentPlayer == ConfigurationManager.AppSettings["Player1"] ? ConfigurationManager.AppSettings["Player2"] : ConfigurationManager.AppSettings["Player1"];                
            }
        }
    }

    public class MatchSuitStrategy : MatchStrategy
    {
        public MatchSuitStrategy(List<string> cards) : base(cards)
        {
        }

        public override void CheckLogic()
        {
            foreach (string card in cards)
            {
                string cardValue = card.Split('-')[1];

                if (previousValue != null && previousValue == cardValue)
                    scores[currentPlayer]++;

                previousValue = cardValue;
                currentPlayer = currentPlayer == ConfigurationManager.AppSettings["Player1"] ? ConfigurationManager.AppSettings["Player2"] : ConfigurationManager.AppSettings["Player1"];
            }
        }
    }

    public class MatchBothStrategy : MatchStrategy
    {
        public MatchBothStrategy(List<string> cards) : base(cards)
        {
        }

        public override void CheckLogic()
        {
            foreach (string card in cards)
            {
                string cardValue = card;

                if (previousValue != null && previousValue == cardValue)
                    scores[currentPlayer]++;

                previousValue = cardValue;
                currentPlayer = currentPlayer == ConfigurationManager.AppSettings["Player1"] ? ConfigurationManager.AppSettings["Player2"] : ConfigurationManager.AppSettings["Player1"];
            }
        }
    }

    public static class MatchStrategyFactory
    {
        public static MatchStrategy CreateStrategy(MatchCondition condition, List<string> cards)
        {
            switch (condition)
            {
                case MatchCondition.FaceValue:
                    return new MatchFaceValueStrategy(cards);
                case MatchCondition.Suit:
                    return new MatchSuitStrategy(cards);
                case MatchCondition.Both:
                    return new MatchBothStrategy(cards);
                default:
                    throw new ArgumentException(ConfigurationManager.AppSettings["InvalidMatchCondition"]);
            }
        }
    }

    public enum MatchCondition
    {
        FaceValue = 1,
        Suit = 2,
        Both = 3
    }
}





