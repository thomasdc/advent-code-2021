using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    public static int Part1(this int[] timers)
    {
        var groups = timers
            .GroupBy(_ => _)
            .Select(_ => new { timer = _.Key, numberOfFish = _.Count() })
            .Union(Enumerable.Range(0, 9).Select(_ => new { timer = _, numberOfFish = 0 }))
            .GroupBy(_ => _.timer)
            .ToDictionary(_ => _.Key, _ => _.Select(x => x.numberOfFish).Sum());
        for (var iteration = 1; iteration <= 80; iteration++)
        {
            var numberOfHatches = groups[0];
            for (var i = 0; i < 8; i++)
            {
                groups[i] = groups[i + 1];
            }

            groups[6] += numberOfHatches;
            groups[8] = numberOfHatches;
        }
        
        return groups.Sum(_ => _.Value);
    }
}

public static class Utils
{
    public static int[] ParseInput(this string fileName) =>
        File.ReadAllLines(fileName).Single().Split(',').Select(int.Parse).ToArray();

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(5934);
}
