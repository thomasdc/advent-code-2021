using Shouldly;
using Xunit;

Console.WriteLine(Solver.Part1(7, 8));

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
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => Solver.Part1(4, 8).ShouldBe(739785);
}
