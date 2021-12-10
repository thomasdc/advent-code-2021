﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this string[] lines)
    {
        return lines
            .Select(Process)
            .Where(_ => _.IsCorrupt)
            .Select(_ => _.IllegalCharacter switch
            {
                ')' => 3,
                ']' => 57,
                '}' => 1197,
                '>' => 25137,
                _ => 0
            }).Sum();
    }

    private static (bool IsCorrupt, char? IllegalCharacter, Stack<char> stack) Process(this string line)
    {
        var stack = new Stack<char>();
        foreach (var x in line)
        {
            if (x is '(' or '[' or '{' or '<')
            {
                stack.Push(x);
            }
            else if (Matches((stack.Peek(), x)))
            {
                stack.Pop();
            }
            else
            {
                return (true, x, stack);
            }
        }

        return (false, null, stack);
    }

    private static bool Matches((char x, char y) c) => c switch
    {
        ('(', ')') => true,
        ('[', ']') => true,
        ('{', '}') => true,
        ('<', '>') => true,
        _ => false
    };
}

public static class Utils
{
    public static string[] ParseInput(this string fileName) => File.ReadAllLines(fileName);

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(26397);
}
