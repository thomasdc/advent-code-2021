using Shouldly;
using System.Text.RegularExpressions;
using Xunit;

"example.txt".ParseInput().ToList().Part1().Print();

public static class Solver
{
    public static int Part1(this List<Scanner> scanners)
    {
        var root = scanners[0];
        scanners.Remove(root);

        while (scanners.Any())
        {
            var matches = (
                from to in scanners
                let o = root.HasOverlappingDetectionRegion(to)
                where o.overlaps
                select (to, o.matchingPairs)).ToArray();
            foreach (var (to, matchingPairs) in matches)
            {
                var transformation = (
                    from trans in matchingPairs[0].right.Coordinate.Transformations()
                    let delta = matchingPairs[0].left.Coordinate - trans.Transformed
                    where delta == matchingPairs[1].left.Coordinate - matchingPairs[1].right.Coordinate.Transform(trans.TransformationIndex)
                    select (trans.TransformationIndex, delta)).Single();
                Console.WriteLine($"merging S{to} (transformation index={transformation.TransformationIndex}, delta={transformation.delta})");
                var rebased = (
                    from b in to.BeaconReports
                    select transformation.delta + b.Coordinate.Transform(transformation.TransformationIndex)).ToArray();
                root.Merge(rebased);
                scanners.Remove(to);
            }
        }

        return root.BeaconReports.Count;
    }
}

public class Scanner
{
    public int Id { get; }
    private List<BeaconReport> _beaconReports = new();
    public IReadOnlyCollection<BeaconReport> BeaconReports => _beaconReports.AsReadOnly();

    public Scanner(int id)
    {
        Id = id;
    }

    public void Add(IEnumerable<BeaconReport> reports)
    {
        _beaconReports.AddRange(reports);
        foreach (var report in _beaconReports)
        {
            report.CalculateDistances();
        }
    }

    public (bool overlaps, (BeaconReport left, BeaconReport right)[] matchingPairs) HasOverlappingDetectionRegion(Scanner other)
    {
        var matchingPairs = (
            from left in _beaconReports
            from right in other._beaconReports
            where left.IsEquivalentTo(right)
            select (left, right)).ToArray();
        return (matchingPairs.Length >= 12, matchingPairs);
    }

    public void Merge(Coordinate[] coordinates)
    {
        Add(coordinates
            .Where(_ => BeaconReports.All(x => x.Coordinate != _))
            .Select(_ => new BeaconReport(this, _)));
    }

    public override string ToString() => $"{Id}";
}

public class BeaconReport
{
    private Dictionary<BeaconReport, int[]> _manhattanDistances = new();
    private readonly Scanner _scanner;
    public readonly Coordinate Coordinate;

    public BeaconReport(Scanner scanner, Coordinate coordinate)
    {
        _scanner = scanner;
        Coordinate = coordinate;
    }

    public override string ToString() => $"S{_scanner} {Coordinate}";

    public void CalculateDistances()
    {
        _manhattanDistances = new Dictionary<BeaconReport, int[]>();
        foreach (var other in _scanner.BeaconReports.Where(_ => _ != this))
        {
            var distances = new int[3];
            distances[0] = Math.Abs(other.Coordinate.X - Coordinate.X);
            distances[1] = Math.Abs(other.Coordinate.Y - Coordinate.Y);
            distances[2] = Math.Abs(other.Coordinate.Z - Coordinate.Z);
            _manhattanDistances.Add(other, distances);
        }
    }

    public bool IsEquivalentTo(BeaconReport other)
    {
        var matches =  (
            from left in _manhattanDistances
            from right in other._manhattanDistances
            where left.Value.OrderBy(_ => _).SequenceEqual(right.Value.OrderBy(_ => _))
            select (left, right)).ToArray();

        if (matches.Length >= 11)
        {
            Console.WriteLine($"found a match between {this}\t\t and {other}\t\t(overlap: {matches.Length})");
        }
        
        return matches.Count() >= 11;
    }
}

public record Coordinate(int X, int Y, int Z)
{
    public static Coordinate operator +(Coordinate a, Coordinate b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Coordinate operator -(Coordinate a, Coordinate b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public Coordinate Transform(int index) => Transformations().ElementAt(index).Transformed;

    public IEnumerable<(Coordinate Transformed, int TransformationIndex)> Transformations()
    {
        yield return (new Coordinate(X, Y, Z), 0);
        yield return (new Coordinate(X, Z, -Y), 1);
        yield return (new Coordinate(X, -Y, -Z), 2);
        yield return (new Coordinate(X, -Z, Y), 3);

        yield return (new Coordinate(-X, -Y, Z), 4);
        yield return (new Coordinate(-X, Z, Y), 5);
        yield return (new Coordinate(-X, Y, -Z), 6);
        yield return (new Coordinate(-X, -Z, -Y), 7);

        yield return (new Coordinate(Y, Z, X), 8);
        yield return (new Coordinate(Y, X, -Z), 9);
        yield return (new Coordinate(Y, -Z, -X), 10);
        yield return (new Coordinate(Y, -X, Z), 11);

        yield return (new Coordinate(-Y, -Z, X), 12);
        yield return (new Coordinate(-Y, X, Z), 13);
        yield return (new Coordinate(-Y, Z, -X), 14);
        yield return (new Coordinate(-Y, -X, -Z), 15);

        yield return (new Coordinate(Z, X, Y), 16);
        yield return (new Coordinate(Z, Y, -X), 17);
        yield return (new Coordinate(Z, -X, -Y), 18);
        yield return (new Coordinate(Z, -Y, X), 19);

        yield return (new Coordinate(-Z, -X, Y), 20);
        yield return (new Coordinate(-Z, Y, X), 21);
        yield return (new Coordinate(-Z, X, -Y), 22);
        yield return (new Coordinate(-Z, -Y, -X), 23);
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
}

public static class Utils
{
    public static IEnumerable<Scanner> ParseInput(this string fileName)
    {
        Scanner? scanner = null;
        var reports = new List<BeaconReport>();
        foreach (var line in File.ReadAllLines(fileName).Append(string.Empty))
        {
            var match = Regex.Match(line, "--- scanner (\\d+) ---");
            if (match.Success)
            {
                reports.Clear();
                scanner = new Scanner(int.Parse(match.Groups[1].Value));
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                scanner!.Add(reports);
                yield return scanner!;
            }
            else
            {
                var split = line.Split(',').Select(int.Parse).ToArray();
                reports.Add(new BeaconReport(scanner!, new Coordinate(split[0], split[1], split[2])));
            }
        }
    }

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().ToList().Part1().ShouldBe(79);
}
