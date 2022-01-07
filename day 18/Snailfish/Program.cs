using Shouldly;
using Xunit;

"input.txt".Part2().Print();

public static class Solve
{
    public static long Part1(this IEnumerable<Snailfish> homework) => 
        homework.Skip(1).Aggregate(homework.ElementAt(0), (acc, cur) => acc.Add(cur)).Root.Magnitude;

    public static long Part2(this string fileName) =>
            (from left in File.ReadAllLines(fileName)
             from right in File.ReadAllLines(fileName)
             where !left.Equals(right)
             let l = Utils.ParseLine(left)
             let r = Utils.ParseLine(right)
             select Part1(new[] { l, r })).Max();
}

public class Snailfish
{
    public Node Root { get; set; }
    public Number Head { get; set; }
    public Number Tail { get; set; }

    public Snailfish Add(Snailfish other)
    {
        var newRoot = new Pair();
        newRoot.Left = Root;
        Root.Parent = newRoot;
        newRoot.Right = other.Root;
        other.Root.Parent = newRoot;
        Tail.Next = other.Head;
        other.Head.Previous = Tail;
        var snailfish = new Snailfish
        {
            Root = newRoot,
            Head = Head,
            Tail = other.Tail
        };

        var reducible = true;
        while (reducible)
        {
            reducible = snailfish.Reduce();
        }

        return snailfish;
    }

    public bool Reduce()
    {
        var explodingPair = Root.Pairs().FirstOrDefault(_ => _.NestingLevel == 4);
        if (explodingPair != null)
        {
            explodingPair.Explode(this);
            return true;
        }

        var splitNumber = Root.Numbers().FirstOrDefault(_ => _.Value >= 10);
        if (splitNumber != null)
        {
            splitNumber.Split(this);
            return true;
        }

        return false;
    }
}

public abstract class Node
{
    public Node? Parent { get; set; }
    public int NestingLevel => Parent == null ? 0 : Parent.NestingLevel + 1;
    public abstract IEnumerable<Pair> Pairs();
    public abstract IEnumerable<Number> Numbers();
    public abstract int Magnitude { get; }
}

public class Pair : Node
{
    public Node Left { get; set; } = null!;
    public Node Right { get; set; } = null!;

    public override int Magnitude => 3 * Left.Magnitude + 2 * Right.Magnitude;

    public override string ToString() => $"[{Left},{Right}]";

    public void Explode(Snailfish snailfish)
    {
        var left = (Number)Left;
        var right = (Number)Right;

        if (left.Previous != null)
        {
            left.Previous.Value += left.Value;
        }

        if (right.Next != null)
        {
            right.Next.Value += right.Value;
        }

        var parent = (Pair)Parent!;
        var newNumber = new Number
        {
            Parent = parent,
            Next = right.Next,
            Previous = left.Previous
        };

        if (right.Next != null)
        {
            right.Next.Previous = newNumber;
        }

        if (left.Previous != null)
        {
            left.Previous.Next = newNumber;
        }

        if (parent.Left == this)
        {
            parent.Left = newNumber;
            if (snailfish.Head == left)
            {
                snailfish.Head = newNumber;
            }
        }

        if (parent.Right == this)
        {
            parent.Right = newNumber;
            if (snailfish.Tail == right)
            {
                snailfish.Tail = newNumber;
            }
        }
    }

    public override IEnumerable<Pair> Pairs()
    {
        foreach (var pair in Left.Pairs())
        {
            yield return pair;
        }

        foreach (var pair in Right.Pairs())
        {
            yield return pair;
        }

        yield return this;
    }

    public override IEnumerable<Number> Numbers()
    {
        foreach (var number in Left.Numbers())
        {
            yield return number;
        }

        foreach (var number in Right.Numbers())
        {
            yield return number;
        }
    }
}

public class Number : Node
{
    public int Value { get; set; }
    public Number? Previous { get; set; }
    public Number? Next { get; set; }

    public override int Magnitude => Value;

    public override IEnumerable<Number> Numbers()
    {
        yield return this;
    }

    public override IEnumerable<Pair> Pairs()
    {
        yield break;
    }

    public override string ToString() => $"{Value}";

    public void Split(Snailfish snailfish)
    {
        var newLeft = new Number
        {
            Value = Value / 2,
            Previous = Previous
        };

        if (Previous != null)
        {
            Previous.Next = newLeft;
        }

        var newRight = new Number
        {
            Value = Value - newLeft.Value,
            Next = Next
        };

        if (Next != null)
        {
            Next.Previous = newRight;
        }

        newLeft.Next = newRight;
        newRight.Previous = newLeft;

        var newPair = new Pair
        {
            Left = newLeft,
            Right = newRight,
            Parent = Parent
        };

        newLeft.Parent = newPair;
        newRight.Parent = newPair;

        var parent = (Pair)Parent!;
        if (parent.Left == this)
        {
            parent.Left = newPair;
            if (snailfish.Head == this)
            {
                snailfish.Head = newLeft;
            }
        }

        if (parent.Right == this)
        {
            parent.Right = newPair;
            if (snailfish.Tail == this)
            {
                snailfish.Tail = newRight;
            }
        }
    }
}

public static class Utils
{
    public static IEnumerable<Snailfish> ParseInput(this string fileName) => File.ReadAllLines(fileName).Select(ParseLine);

    public static Snailfish ParseLine(string line)
    {
        var snailfish = new Snailfish();
        var enumerator = line.GetEnumerator();
        snailfish.Root = Parse(enumerator, snailfish, null);
        return snailfish;
    }

    private static Node Parse(IEnumerator<char> chars, Snailfish snailfish, Node? parent)
    {
        chars.MoveNext();
        var next = chars.Current;
        if (int.TryParse(next.ToString(), out var value))
        {
            var number = new Number
            {
                Value = value,
                Parent = parent
            };

            snailfish.Head ??= number;
            number.Previous = snailfish.Tail;
            if (number.Previous != null)
            {
                number.Previous.Next = number;
            }

            snailfish.Tail = number;
            return number;
        }

        var pair = new Pair { Parent = parent };
        pair.Left = Parse(chars, snailfish, pair);
        chars.MoveNext();
        pair.Right = Parse(chars, snailfish, pair);
        chars.MoveNext();
        return pair;
    }

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Theory]
    [InlineData("example1.txt", 3488)]
    [InlineData("example2.txt", 4140)]
    public void ValidatePart1Examples(string fileName, int expectedMagnitude) => fileName.ParseInput().Part1().ShouldBe(expectedMagnitude);
    
    [Theory]
    [InlineData("example2.txt", 3993)]
    public void ValidatePart2Examples(string fileName, int expectedMagnitude) => fileName.Part2().ShouldBe(expectedMagnitude);
}
