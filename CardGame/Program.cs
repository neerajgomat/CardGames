using System;
using System.Collections.Generic;

namespace CardGame
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Please enter atleast 1 Number of Pack -");

                int numberOfPacks = Convert.ToInt32(Console.ReadLine());

                if (numberOfPacks <= 0)
                {
                    Console.WriteLine("You entered 0 Number of Pack.");
                    return;
                }

                Console.WriteLine("Please select match condition 1 for FaceValue, 2 for Suit, 3 for Both -");

                string selectedCondition = Console.ReadLine();

                int.TryParse(selectedCondition, out int resultSelectedCondition);

                if (resultSelectedCondition <= 0 || resultSelectedCondition > 3)
                {
                    Console.WriteLine("You selcted wrong match condition.");
                    return;
                }

                MatchCondition matchCondition = (MatchCondition)Enum.Parse(typeof(MatchCondition), selectedCondition, true);

                //Add Cards
                List<string> cards = PrepareCards(numberOfPacks);

                MatchStrategy strategy = null;

                //Check for selected cards value

                if (matchCondition == MatchCondition.FaceValue)
                    strategy = new MatchFaceValueStrategy(cards);
                else if (matchCondition == MatchCondition.Suit)
                    strategy = new MatchSuitStrategy(cards);
                else if (matchCondition == MatchCondition.Both)
                    strategy = new MatchBothStrategy(cards);

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
            for (int index = 0; index < numberOfPacks; index++)
            {
                foreach (var cardNumber in cardNumbers)
                {
                    foreach (var typeOfCard in typeOfCards)
                    {
                        cards.Add($"{cardNumber}-{typeOfCard}");
                    }
                }
            }

            // Shuffle the cards
            Random rng = new Random();

            for (int index = 0; index < cards.Count; index++)
            {
                int k = rng.Next(index + 1);
                string temp = cards[k];
                cards[k] = cards[index];
                cards[index] = temp;
            }

            return cards;
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
                    { "Player 1", 0 },

                    { "Player 2", 0 }
                };

        public abstract void CheckLogic();

        /// <summary>
        /// Check for result
        /// </summary>
        /// <returns></returns>

        public string GetResult()
        {
            if (scores["Player 1"] > scores["Player 2"])
                return $"Playe 1 score-{scores["Player 1"]} and Player 2 score-{scores["Player 2"]} so Player 1 wins with score : {scores["Player 1"]}";
            else if (scores["Player 1"] < scores["Player 2"])
                return $"Playe 1 score-{scores["Player 1"]} and Player 2 score-{scores["Player 2"]} soPlayer 2 wins with score : {scores["Player 2"]}";
            else if (scores["Player 1"] == scores["Player 2"])
                return $"Playe 1 score-{scores["Player 1"]} and Player 2 score-{scores["Player 2"]} so Draw with score : {scores["Player 1"]}";
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

                currentPlayer = currentPlayer == "Player 1" ? "Player 2" : "Player 1";
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

                currentPlayer = currentPlayer == "Player 1" ? "Player 2" : "Player 1";
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

                currentPlayer = currentPlayer == "Player 1" ? "Player 2" : "Player 1";                
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

