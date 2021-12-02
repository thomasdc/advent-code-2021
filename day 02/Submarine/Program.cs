using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this IEnumerable<Instruction> instructions) => instructions
        .Aggregate(new Position(), (position, instruction) => position.Execute(instruction))
        .Multiply();
}

public record struct Position(int HorizontalPosition, int Depth)
{
    public Position Execute(Instruction instruction) => instruction.Direction switch
    {
        "forward" => this with { HorizontalPosition = HorizontalPosition + instruction.Units },
        "up" => this with { Depth = Depth + instruction.Units },
        "down" => this with { Depth = Depth - instruction.Units },
        _ => throw new NotImplementedException()
    };

    public int Multiply() => HorizontalPosition * -Depth;
}

public record struct Instruction(string Direction, int Units);

public static class Utils
{
    public static IEnumerable<Instruction> ParseInput(this string fileName) =>
        from line in File.ReadAllLines(fileName)
        let split = line.Split(' ')
        select new Instruction(split[0], int.Parse(split[1]));

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(150);
}
