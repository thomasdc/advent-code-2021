using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

"input.txt".ParseInput().Part2().Print();

public static class Solver
{
    private static readonly List<char> Segments = new() { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };

    public static int Part1(this (string[] input, string[] output)[] signals) =>
        signals.SelectMany(_ => _.output).Count(_ => _.Length is 2 or 4 or 3 or 7);

    public static int Part2(this (string[] input, string[] output)[] signals)
    {
        var allCombinations = Combinations(Enumerable.Range(0, 7).ToArray()).ToArray();
        return -1;
    }

    public static int Decode(string[] output, int[] mapping) =>
        output.Select((value, i) => (value, i)).Sum(x =>
            Map(output[x.i].Select(_ => Segments[mapping[Segments.IndexOf(_)]])) *
            (int)Math.Pow(10, output.Length - 1 - x.i));

    private static int Map(IEnumerable<char> x) => new string(x.OrderBy(_ => _).ToArray()) switch
    {
        "abcefg" => 0,
        "cf" => 1,
        "acdeg" => 2,
        "acdfg" => 3,
        "bcdf" => 4,
        "abdfg" => 5,
        "abdefg" => 6,
        "acf" => 7,
        "abcdefg" => 8,
        "abcdfg" => 9,
        _ => -1
    };

    private static int[] ExtractMapping(string[] input)
    {
        return new int[0];
    }

    private static IEnumerable<int[]> Combinations(int[] input)
    {
        if (input.Length == 1)
        {
            yield return input;
        } 
        else
        {
            foreach (var i in input)
            {
                foreach (var combination in Combinations(input.Except(new []{i}).ToArray()))
                {
                    yield return new List<int> { i }.Union(combination).ToArray();
                }
            }
        }
    }
}

public static class Utils
{
    public static (string[], string[])[] ParseInput(this string fileName) => (
        from line in File.ReadAllLines(fileName)
        let parts = line.Split('|')
        select (parts[0].Split(' '), parts[1].Split(' '))).ToArray();

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(26);

    [Theory]
    [InlineData(new[] { "cdfeb", "fcadb", "cdfeb", "cdbaf" }, new[] { 2, 5, 6, 0, 1, 3, 4 }, 5353)]
    public void ValidateDecoding(string[] output, int[] mapping, int expectedDecodedValue) =>
        Solver.Decode(output, mapping).ShouldBe(expectedDecodedValue);
}
