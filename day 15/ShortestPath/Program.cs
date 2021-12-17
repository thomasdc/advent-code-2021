using Shouldly;
using Xunit;

"input.txt".ParseInput().Part1().Print();

public static class Solver
{
    private static readonly (int x, int y)[] NeighbourVectors = { (0, -1), (0, 1), (-1, 0), (1, 0) };

    public static long Part1(this int[][] risks)
    {
        var shortestPath = Dijkstra(risks).ToArray();
        return shortestPath.Sum(_ => risks[_.y][_.x]);
    }

    private static IEnumerable<(int x, int y)> Dijkstra(int[][] risks)
    {
        var height = risks.Length;
        var width = risks[0].Length;
        var nodes = risks.SelectMany((row, y) => row.Select((_, x) => (x, y)).ToArray()).ToArray();
        var initial = (0, 0);
        var destination = (height - 1, width - 1);
        var unvisited = new List<(int x, int y)>(nodes);
        var paths = new ((int x, int y) node, int cost, (int x, int y)? via)[nodes.Length];
        paths[0] = (initial, 0, initial);
        var i = 1;
        foreach (var node in nodes.Except(new[] {initial}))
        {
            paths[i++] = (node, int.MaxValue, null);
        }

        do
        {
            var current = paths.First(_ => unvisited.Contains(_.node)).node;
            foreach (var neighbour in current.Neighbours(width, height).Intersect(unvisited))
            {
                var currentCost = paths.Single(_ => _.node == current).cost;
                var cost = currentCost + risks[neighbour.y][neighbour.x];
                if (cost < paths.Single(_ => _.node == neighbour).cost)
                {
                    var entry = paths.Single(_ => _.node == neighbour);
                    paths[Array.IndexOf(paths, entry)] = (neighbour, cost, current);
                }
            }

            unvisited.Remove(current);
            Sort();
        } while (unvisited.Contains(destination));

        void Sort() => Array.Sort(paths, (left, right) => left.cost - right.cost);

        var currentNode = destination;
        do
        {
            yield return currentNode;
            var path = paths.Single(_ => _.node == currentNode);
            currentNode = path.via!.Value;
        } while (currentNode != initial);
    }

    private static (int x, int y)[] Neighbours(this (int x, int y) point, int width, int height) => NeighbourVectors
            .Select(_ => (x: point.x + _.x, y: point.y + _.y))
            .Where(_ => !(_.x < 0 || _.x >= width || _.y < 0 || _.y >= height))
            .ToArray();
}

public static class Utils
{
    public static int[][] ParseInput(this string fileName) => 
        File.ReadAllLines(fileName).Select(_ => _.Select(x => int.Parse(x.ToString())).ToArray()).ToArray();

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Fact]
    public void ValidatePart1Example() => "example.txt".ParseInput().Part1().ShouldBe(40);
}
