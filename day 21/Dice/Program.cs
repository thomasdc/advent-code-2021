using Shouldly;
using Xunit;

Console.WriteLine(Solver.Part2(7, 8));

public static class Solver
{
    public static int Part1(int player1StartingPosition, int player2StartingPosition)
    {
        var numberOfPlays = 0;
        var player1Score = 0;
        var player2Score = 0;
        var player1Position = player1StartingPosition;
        var player2Position = player2StartingPosition;
        while (true)
        {
            // player 1 plays
            player1Position = (((3 * (1 + numberOfPlays * 3)) + 3 + player1Position - 1) % 10) + 1;
            player1Score += player1Position;
            numberOfPlays++;

            if (player1Score >= 1000)
            {
                return player2Score * numberOfPlays * 3;
            }

            // player 2 plays
            player2Position = (((3 * (1 + numberOfPlays * 3)) + 3 + player2Position - 1) % 10) + 1;
            player2Score += player2Position;
            numberOfPlays++;

            if (player2Score >= 1000)
            {
                return player1Score * numberOfPlays * 3;
            }
        }
    }

    public static long Part2(int player1StartingPosition, int player2StartingPosition)
    {
        var finishedGames = new Dictionary<GameState, long>();
        var pendingGames = new Dictionary<GameState, long>
        {
            [new GameState(player1StartingPosition, 0, player2StartingPosition, 0, true)] = 1
        };

        while (pendingGames.Any())
        {
            var newPendingGames = new Dictionary<GameState, long>();
            foreach (var game in pendingGames)
            {
                var nextStates = game.Key.NextStates()
                    .GroupBy(_ => _)
                    .Select(_ => (_.Key, _.Count() * game.Value))
                    .GroupBy(_ => _.Key.Player1Score >= 21 || _.Key.Player2Score >= 21)
                    .ToArray();
                
                foreach (var (gameState, universeCount) in nextStates.Where(_ => !_.Key).SelectMany(_ => _))
                {
                    newPendingGames[gameState] = newPendingGames.GetValueOrDefault(gameState, 0) + universeCount;
                }

                foreach (var (gameState, universeCount) in nextStates.Where(_ => _.Key).SelectMany(_ => _))
                {
                    finishedGames[gameState] = finishedGames.GetValueOrDefault(gameState, 0) + universeCount;
                }
            }

            pendingGames = newPendingGames;
        }

        return finishedGames
            .GroupBy(_ => _.Key.Player1Score > _.Key.Player2Score)
            .Select(_ => _.Sum(x => x.Value))
            .Max();
    }
}

public record struct GameState(
    int Player1Position,
    int Player1Score,
    int Player2Position,
    int Player2Score,
    bool Player1IsNext)
{
    private static readonly (int diceRoll1, int diceRoll2, int diceRoll3)[] DiceRolls = (
        from diceRoll1 in new[] { 1, 2, 3 }
        from diceRoll2 in new[] { 1, 2, 3 }
        from diceRoll3 in new[] { 1, 2, 3 }
        select (diceRoll1, diceRoll2, diceRoll3)).ToArray();

    public readonly IEnumerable<GameState> NextStates()
    {
        foreach (var (diceRoll1, diceRoll2, diceRoll3) in DiceRolls)
        {
            var currentPosition = Player1IsNext ? Player1Position : Player2Position;
            var newPosition = (currentPosition + diceRoll1 + diceRoll2 + diceRoll3 - 1) % 10 + 1;
            yield return Player1IsNext switch
            {
                true => this with
                {
                    Player1Position = newPosition,
                    Player1Score = Player1Score + newPosition,
                    Player1IsNext = false
                },
                false => this with
                {
                    Player2Position = newPosition,
                    Player2Score = Player2Score + newPosition,
                    Player1IsNext = true
                }
            };
        }
    }
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => Solver.Part1(4, 8).ShouldBe(739785);

    [Fact]
    public void ValidatePart2Example() => Solver.Part2(4, 8).ShouldBe(444356092776315);
}
