using Shouldly;
using System.Globalization;
using Xunit;

"input.txt".ParseInput().Part2().Print();

public static class Solver
{
    public static long Part1(this string transmission)
    {
        var bits = new BitEnumerator(transmission.Bits());
        var packet = Packet.Create(bits);
        return packet.VersionSum;
    }

    public static long Part2(this string transmission)
    {
        var bits = new BitEnumerator(transmission.Bits());
        var packet = Packet.Create(bits);
        return packet.CalculateValue();
    }

    private static IEnumerable<bool> Bits(this string transmission)
    {
        foreach (var packet in transmission.Select(_ => int.Parse(_.ToString(), NumberStyles.HexNumber)))
        {
            yield return (packet & 1 << 3) > 0;
            yield return (packet & 1 << 2) > 0;
            yield return (packet & 1 << 1) > 0;
            yield return (packet & 1 << 0) > 0;
        }
    }

    public static long ToNumber(this bool[] bits) =>
        Convert.ToInt64(string.Join("", bits.Select(_ => _ ? "1" : "0")), fromBase: 2);
}

public abstract class Packet
{
    public long Version { get; private set; }
    public long TypeId { get; private set; }

    public virtual long VersionSum => Version;
    
    public static Packet Create(BitEnumerator bits)
    {
        var version = bits.TakeNumber(3);
        var typeId = bits.TakeNumber(3);
        Packet packet = typeId == 4 ? new LiteralValue() : new Operator();
        packet.Version = version;
        packet.TypeId = typeId;
        packet.Process(bits);
        return packet;
    }

    public abstract long CalculateValue();
    protected abstract void Process(BitEnumerator bits);
}

public class LiteralValue : Packet
{
    public long Number { get; private set; }

    public override long CalculateValue() => Number;

    protected override void Process(BitEnumerator bits)
    {
        var accumulated = new List<bool>();
        var final = false;
        while (!final)
        {
            final = !bits.Take(1)[0];
            accumulated.AddRange(bits.Take(4));
        }

        Number = accumulated.ToArray().ToNumber();
    }
}

public class Operator : Packet
{
    private readonly List<Packet> _subPackets = new();

    public override long VersionSum => Version + _subPackets.Sum(_ => _.VersionSum);

    public override long CalculateValue()
    {
        return TypeId switch
        {
            0 => _subPackets.Sum(_ => _.CalculateValue()),
            1 => _subPackets.Aggregate(1L, (acc, _) => acc * _.CalculateValue()),
            2 => _subPackets.Min(_ => _.CalculateValue()),
            3 => _subPackets.Max(_ => _.CalculateValue()),
            5 => _subPackets.ElementAt(0).CalculateValue() > _subPackets.ElementAt(1).CalculateValue() ? 1 : 0,
            6 => _subPackets.ElementAt(0).CalculateValue() < _subPackets.ElementAt(1).CalculateValue() ? 1 : 0,
            7 => _subPackets.ElementAt(0).CalculateValue() == _subPackets.ElementAt(1).CalculateValue() ? 1 : 0,
            _ => throw new NotImplementedException(),
        };
    }

    protected override void Process(BitEnumerator bits)
    {
        if (bits.Take(1)[0])
        {
            var numberOfSubPackets = bits.TakeNumber(11);
            for (var i = 0; i < numberOfSubPackets; i++)
            {
                var child = Create(bits);
                _subPackets.Add(child);
            }
        }
        else
        {
            var totalLength = bits.TakeNumber(15);
            var initial = bits.EnumeratedCount;
            while (totalLength > bits.EnumeratedCount - initial)
            {
                var child = Create(bits);
                _subPackets.Add(child);
            }
        }
    }
}

public class BitEnumerator
{
    private readonly IEnumerator<bool> bits;
    public int EnumeratedCount { get; private set; }

    public BitEnumerator(IEnumerable<bool> bits)
    {
        this.bits = bits.GetEnumerator();
    }

    public bool[] Take(int count)
    {
        var result = new bool[count];
        for (var i = 0; i < count; i++)
        {
            bits.MoveNext();
            EnumeratedCount++;
            result[i] = bits.Current;
        }

        return result;
    }

    public long TakeNumber(int bitCount) => Take(bitCount).ToNumber();
}

public static class Utils
{
    public static string ParseInput(this string fileName) => File.ReadAllText(fileName);

    public static void Print(this object o) => Console.WriteLine(o);
}

public class Tests
{
    [Theory]
    [InlineData("8A004A801A8002F478", 16)]
    [InlineData("620080001611562C8802118E34", 12)]
    [InlineData("C0015000016115A2E0802F182340", 23)]
    [InlineData("A0016C880162017C3686B18A3D4780", 31)]
    public void ValidatePart1Examples(string transmission, int versionSum) => transmission.Part1().ShouldBe(versionSum);
    
    [Theory]
    [InlineData("C200B40A82", 3)]
    [InlineData("04005AC33890", 54)]
    [InlineData("880086C3E88112", 7)]
    [InlineData("CE00C43D881120", 9)]
    [InlineData("D8005AC2A8F0", 1)]
    [InlineData("F600BC2D8F", 0)]
    [InlineData("9C005AC2F8F0", 0)]
    [InlineData("9C0141080250320F1802104A08", 1)]
    public void ValidatePart2Examples(string transmission, int value) => transmission.Part2().ShouldBe(value);
}
