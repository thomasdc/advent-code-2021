using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this (string from, string to)[] input)
    {
        var nodes = input
            .Select(_ => _.from)
            .Union(input.Select(_ => _.to))
            .Distinct()
            .Select(_ => new Node(_))
            .ToDictionary(_ => _.Name, _ => _);
        foreach (var (from, to) in input)
        {
            nodes[from].Add(new Connection(nodes[from], nodes[to]));
        }

        var start = nodes.Values.Where(_ => _.IsStart);
        var currentPath = new Stack<Node>(start);
        var paths = Step(currentPath).ToArray();
        return paths.Length;
    }

    private static IEnumerable<Stack<Node>> Step(Stack<Node> currentPath)
    {
        var rear = currentPath.Peek();
        if (rear.IsEnd)
        {
            yield return currentPath;
        }
        else
        {
            foreach (var next in rear.Neighbours.Where(_ => currentPath.Where(x => x.IsSmall).All(x => x != _)))
            {
                var newStack = new Stack<Node>(currentPath.Reverse());
                newStack.Push(next);
                foreach (var path in Step(newStack))
                {
                    yield return path;
                }
            }
        }
    }
}

public record Node(string Name)
{
    public bool IsStart => Name == "start";
    public bool IsEnd => Name == "end";
    public bool IsSmall => Name == Name.ToLowerInvariant();

    private readonly HashSet<Node> _neighbours = new();
    public IReadOnlySet<Node> Neighbours => _neighbours;

    public void Add(Connection connection)
    {
        if (connection.From == this)
        {
            _neighbours.Add(connection.To);
            connection.To._neighbours.Add(this);
        }
        else if (connection.To == this)
        {
            _neighbours.Add(connection.From);
            connection.From._neighbours.Add(this);
        }
    }
}

public record Connection(Node From, Node To);

public static class Utils
{
    public static (string from, string to)[] ParseInput(this string fileName) => File.ReadAllLines(fileName)
        .Select(_ => _.Split('-')).Select(_ => (_[0], _[1])).ToArray();

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example1() => "example1.txt".ParseInput().Part1().ShouldBe(10);
    
    [Fact]
    public void ValidatePart1Example2() => "example2.txt".ParseInput().Part1().ShouldBe(19);
    
    [Fact]
    public void ValidatePart1Example3() => "example3.txt".ParseInput().Part1().ShouldBe(226);
}
