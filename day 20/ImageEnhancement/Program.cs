using Shouldly;
using System.Collections;
using Xunit;

"input.txt".ParseInput().Part2().Print();

public static class Solver
{
    private static readonly (int x, int y)[] NeighbourVectors = (
        from y in new[] { -1, 0, 1 }
        from x in new[] {-1, 0, 1}
        select (x, y)).ToArray();

    public static int Part1(this (BitArray algorithm, HashSet<(int x, int y)> image) input, int numberOfIterations = 2)
    {
        var image = input.image;
        var minX = image.MinBy(_ => _.x).x;
        var maxX = image.MaxBy(_ => _.x).x;
        var minY = image.MinBy(_ => _.y).y;
        var maxY = image.MaxBy(_ => _.y).y;
        var alternates = input.algorithm[0];
        var fallback = false;
        for (var i = 0; i < numberOfIterations; i++)
        {
            var newImage = new HashSet<(int x, int y)>();
            var newMinX = int.MaxValue;
            var newMaxX = int.MinValue;
            var newMinY = int.MaxValue;
            var newMaxY = int.MinValue;
            for (var y = minY - 1; y <= maxY + 1; y++)
            {
                for (var x = minX - 1; x <= maxX + 1; x++)
                {
                    var array = (x, y).Neighbours().Select(_ =>
                    {
                        if (_.x < minX) return fallback;
                        if (_.y < minY) return fallback;
                        if (_.x > maxX) return fallback;
                        if (_.y > maxY) return fallback;
                        return image.Contains(_);
                    }).ToArray();
                    var index = array.ToDecimal();
                    var isLightPixel = input.algorithm[index];
                    if (isLightPixel)
                    {
                        newImage.Add((x, y));
                    }

                    if (isLightPixel == !fallback)
                    {
                        newMinX = Math.Min(newMinX, x);
                        newMaxX = Math.Max(newMaxX, x);
                        newMinY = Math.Min(newMinY, y);
                        newMaxY = Math.Max(newMaxY, y);
                    }
                }
            }

            image = newImage;
            minX = newMinX;
            maxX = newMaxX;
            minY = newMinY;
            maxY = newMaxY;
            if (alternates)
            {
                fallback = !fallback;
            }
        }

        return image.Count;
    }

    public static int Part2(this (BitArray algorithm, HashSet<(int x, int y)> image) input) => input.Part1(50);

    private static (int x, int y)[] Neighbours(this (int x, int y) point) => NeighbourVectors.Select(_ => (x: point.x + _.x, y: point.y + _.y)).ToArray();
    private static int ToDecimal(this bool[] bits) => bits.Aggregate(0, (result, bit) => (result << 1) + (bit ? 1 : 0));
}

public static class Utils
{
    public static (BitArray algorithm, HashSet<(int x, int y)> image) ParseInput(this string fileName)
    {
        var lines = File.ReadAllLines(fileName);
        var algorithm = new BitArray(lines[0].Select(_ => _ == '#').ToArray());

        var image = new HashSet<(int x,int y)>();
        var y = 0;
        foreach (var line in lines.Skip(2))
        {
            var x = 0;
            foreach (var cell in line)
            {
                if (cell == '#')
                {
                    image.Add((x, y));
                }

                x++;
            }

            y++;
        }

        return (algorithm, image);
    }

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(35);
    
    [Fact]
    public void ValidatePart2Example() => "example.txt".ParseInput().Part2().ShouldBe(3351);
}
