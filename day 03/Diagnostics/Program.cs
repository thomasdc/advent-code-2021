using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this bool[][] bits)
    {
        var common = new List<bool>();
        for (int x = 0; x < bits[0].Length; x++)
        {
            var grouped = bits.Select(_ => _[x]).GroupBy(_ => _).ToDictionary(_ => _.Key, _ => _.Count());
            common.Add(grouped[true] > grouped[false]);
        }

        var gamma = common.ToArray();
        var epsilon = common.Select(_ => !_).ToArray();
        return gamma.ToDecimal() * epsilon.ToDecimal();
    }

    public static int ToDecimal(this bool[] bits) => bits.Aggregate(0, (result, bit) => (result << 1) + (bit ? 1 : 0));
}

public static class Utils
{
    public static bool[][] ParseInput(this string fileName) =>
        File.ReadAllLines(fileName).Select(_ => _.Select(_ => _ == '1').ToArray()).ToArray();

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(198);
}
