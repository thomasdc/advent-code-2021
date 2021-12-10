using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this int[][] heights)
    {
        var lows = new List<int>();
        var height = heights.Length;
        var width = heights[0].Length;
        var neighbourVectors = new (int x, int y)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var currentHeight = heights[y][x];
                var neighbours = neighbourVectors
                    .Select(_ => (x: x + _.x, y: y + _.y))
                    .Where(_ => !OutOfBounds(_))
                    .Select(_ => heights[_.y][_.x])
                    .ToArray();
                if (!neighbours.Any(_ => _ <= currentHeight))
                {
                    lows.Add(currentHeight);
                }
            }
        }
        
        bool OutOfBounds((int x, int y) i) => i.x < 0 || i.x >= width || i.y < 0 || i.y >= height;
        return lows.Sum(_ => _ + 1);
    }
}

public static class Utils
{
    public static int[][] ParseInput(this string fileName) => File.ReadAllLines(fileName)
        .Select(line => line.Select(_ => int.Parse(_.ToString())).ToArray()).ToArray();

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(15);
}
