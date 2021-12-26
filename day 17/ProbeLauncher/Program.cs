using Shouldly;
using System.Text.RegularExpressions;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solve
{
    public static long Part1(this Area target)
    {
        var max = long.MinValue;
        for (var i = 0; i < 100; i++)
        {
            var initial = FindRandomValidTrajectory(target);
            var (optimalTrajectory, maxHeight) = Optimize(initial, target);
            if (maxHeight > max)
            {
                max = maxHeight;
            }
        }
        
        return max;
    }

    // https://en.wikipedia.org/wiki/Hill_climbing
    private static (Trajectory, long maxHeight) Optimize(Trajectory initial, Area target)
    {
        var current = initial;
        var (_, currentEval) = initial.Hits(target);
        while (true)
        {
            var nextEval = long.MinValue;
            Trajectory? nextNode = null;
            foreach (var neighbour in current.Neighbours())
            {
                var (hits, maxHeight) = neighbour.Hits(target);
                if (hits && maxHeight > nextEval)
                {
                    nextNode = neighbour;
                    nextEval = maxHeight;
                }
            }

            if (nextEval <= currentEval)
            {
                return (current, currentEval);
            }

            current = nextNode!;
            currentEval = nextEval;
        }
    }

    private static Trajectory FindRandomValidTrajectory(Area target)
    {
        var random = new Random();
        for (var x = 0; x < 1000; x++)
        {
            var trajectory = new Trajectory(random.Next(50), random.Next(50));
            var (hits, maxHeight) = trajectory.Hits(target);
            if (hits)
            {
                return trajectory;
            }
        }

        throw new Exception("No valid, initial trajectory found");
    }
}

public record Area((int min, int max) X, (int min, int max) Y);

public record Trajectory(int VelocityX, int VelocityY)
{
    public (bool hits, long maxHeight) Hits(Area area)
    {
        var velocityX = VelocityX;
        var velocityY = VelocityY;
        var x = 0L;
        var y = 0L;
        var maxHeight = 0L;
        for (var i = 0; i < 500; i++)
        {
            if (x >= area.X.min && x <= area.X.max && y >= area.Y.min && y <= area.Y.max)
            {
                return (true, maxHeight);
            }

            if (x > area.X.max || y < area.Y.min)
            {
                return (false, maxHeight);
            }

            if (y > maxHeight)
            {
                maxHeight = y;
            }

            x += velocityX;
            y += velocityY;
            if (velocityX != 0)
            {
                velocityX = velocityX > 0 ? velocityX - 1 : velocityX + 1;
            }

            velocityY -= 1;
        }

        return (false, maxHeight);
    }

    private static readonly (int x, int y)[] NeighbourVectors = (
        from x in new[] { -1, 0, 1 }
        from y in new[] { -1, 0, 1 }
        where !(x == 0 && y == 0)
        select (x, y)).ToArray();

    public Trajectory[] Neighbours() => NeighbourVectors
        .Select(_ => new Trajectory(VelocityX + _.x, VelocityY + _.y))                      
        .ToArray();
}

public static class Utils
{
    public static Area ParseInput(this string fileName)
    {
        var match = Regex.Match(
            File.ReadAllText(fileName), "x=(?<minX>-{0,1}\\d+)..(?<maxX>-{0,1}\\d+), y=(?<minY>-{0,1}\\d+)..(?<maxY>-{0,1}\\d+)");
        return new Area((int.Parse(match.Groups["minX"].Value), int.Parse(match.Groups["maxX"].Value)),
            (int.Parse(match.Groups["minY"].Value), int.Parse(match.Groups["maxY"].Value)));
    }

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(45);
}
