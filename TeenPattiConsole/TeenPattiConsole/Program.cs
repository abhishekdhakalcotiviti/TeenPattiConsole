
do
{
    Console.Clear();
    Console.WriteLine("🎴 Welcome to Teen Patti (Faras) Game 🎴\n");

    // 1. Get number of players
    Console.Write("Enter number of players (2-10): ");
    int playerCount;
    while (!int.TryParse(Console.ReadLine(), out playerCount) || playerCount < 2 || playerCount > 10)
    {
        Console.Write("Invalid input. Enter number of players (2-10): ");
    }

    // 2. Get player names
    List<string> players = new List<string>();
    for (int i = 1; i <= playerCount; i++)
    {
        Console.Write($"Enter name for Player {i}: ");
        string name = Console.ReadLine();
        players.Add(string.IsNullOrWhiteSpace(name) ? $"Player{i}" : name);
    }

    // 3. Generate and shuffle the deck
    List<string> deck = GenerateDeck();
    Shuffle(deck);

    // 4. Deal 3 cards to each player
    Dictionary<string, List<string>> hands = new Dictionary<string, List<string>>();

    foreach (string player in players)
    {
        hands[player] = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            hands[player].Add(deck[0]);
            deck.RemoveAt(0);
        }
    }

    // 5. Show each player's hand
    Console.WriteLine("\n🃏 Cards Dealt:\n");
    foreach (var kvp in hands)
    {
        Console.WriteLine($"{kvp.Key}'s Hand: {string.Join(", ", kvp.Value)}");
    }

    // 6. Evaluate hands
    Dictionary<string, (int rank, List<int> values)> evaluatedHands = new Dictionary<string, (int, List<int>)>();

    foreach (var kvp in hands)
    {
        evaluatedHands[kvp.Key] = EvaluateHand(kvp.Value);
    }

    // 7. Find winner(s)
    var bestRank = evaluatedHands.Values.Max(h => h.rank);
    var bestHands = evaluatedHands
        .Where(h => h.Value.rank == bestRank)
        .OrderByDescending(h => h.Value.values, new ListComparer())
        .ToList();

    var winner = bestHands.First();
    var winners = bestHands.Where(h => new ListComparer().Compare(h.Value.values, winner.Value.values) == 0)
                           .Select(h => h.Key)
                           .ToList();

    // 8. Show winner(s)
    Console.WriteLine("\n🏆 Winner(s):");
    foreach (var w in winners)
    {
        Console.WriteLine($"{w} with hand: {string.Join(", ", hands[w])}");
    }

    // 9. Ask to play again
    Console.WriteLine("\nPress Enter to play again, or any other key + Enter to exit...");
}
while (string.IsNullOrWhiteSpace(Console.ReadLine()));


// Generate standard 52-card deck
static List<string> GenerateDeck()
{
    string[] suits = { "♠", "♥", "♦", "♣" };
    string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    List<string> deck = new List<string>();

    foreach (string suit in suits)
    {
        foreach (string rank in ranks)
        {
            deck.Add($"{rank}{suit}");
        }
    }

    return deck;
}

// Fisher-Yates Shuffle
static void Shuffle(List<string> deck)
{
    Random rng = new Random();
    int n = deck.Count;

    while (n > 1)
    {
        n--;
        int k = rng.Next(n + 1);
        var value = deck[k];
        deck[k] = deck[n];
        deck[n] = value;
    }
}

// Evaluate a hand
static (int rank, List<int> values) EvaluateHand(List<string> hand)
{
    var cardValues = new Dictionary<string, int>
            {
                {"2", 2}, {"3", 3}, {"4", 4}, {"5", 5}, {"6", 6},
                {"7", 7}, {"8", 8}, {"9", 9}, {"10", 10},
                {"J", 11}, {"Q", 12}, {"K", 13}, {"A", 14}
            };

    List<int> values = hand.Select(card => cardValues[GetRank(card)]).OrderByDescending(v => v).ToList();
    List<string> suits = hand.Select(GetSuit).ToList();
    List<string> ranks = hand.Select(GetRank).ToList();

    bool isTrail = ranks.Distinct().Count() == 1;
    bool isColor = suits.Distinct().Count() == 1;
    bool isSequence = IsSequence(values);
    bool isPureSequence = isColor && isSequence;
    bool isPair = ranks.GroupBy(r => r).Any(g => g.Count() == 2);

    if (isTrail) return (6, values);               // Trail
    if (isPureSequence) return (5, values);        // Pure sequence
    if (isSequence) return (4, values);            // Sequence
    if (isColor) return (3, values);               // Color
    if (isPair)
    {
        // Put pair values first for tie-breaking
        var grouped = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key);
        List<int> pairOrder = grouped.SelectMany(g => g).ToList();
        return (2, pairOrder);
    }

    return (1, values); // High card
}

// Check if hand is a valid sequence (including A-2-3)
static bool IsSequence(List<int> values)
{
    values = values.OrderBy(v => v).ToList();

    // Special case: A-2-3
    if (values.SequenceEqual(new List<int> { 2, 3, 14 }))
        return true;

    return values[1] == values[0] + 1 && values[2] == values[1] + 1;
}

// Extract rank part (e.g., "A" from "A♠")
static string GetRank(string card)
{
    return new string(card.TakeWhile(c => char.IsLetterOrDigit(c)).ToArray());
}

// Extract suit part (e.g., "♠" from "A♠")
static string GetSuit(string card)
{
    return new string(card.SkipWhile(c => char.IsLetterOrDigit(c)).ToArray());
}

// Custom comparer to compare list of integers
class ListComparer : IComparer<List<int>>
{
    public int Compare(List<int> x, List<int> y)
    {
        for (int i = 0; i < Math.Min(x.Count, y.Count); i++)
        {
            int comp = x[i].CompareTo(y[i]);
            if (comp != 0)
                return comp;
        }
        return x.Count.CompareTo(y.Count);
    }
}
