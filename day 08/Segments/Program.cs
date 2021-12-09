using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this (string[] input, string[] output)[] signals) =>
        signals.SelectMany(_ => _.output).Count(_ => _.Length is 2 or 4 or 3 or 7);
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
}
