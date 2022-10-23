
using System.Buffers.Binary;

namespace GNR.EShare.Protocol.Ops;

public class ScreenResponsePacket
{
    public readonly byte Number;
    public readonly byte MaxNumber;
    public readonly Int16 Size;
    public readonly ReadOnlyMemory<byte> Data;

    public bool CompareHeaders(ScreenResponsePacket other)
    {
        return other.Data.Slice(0, 23).Equals(Data.Slice(0, 22))
               && other.Data.Slice(24, 20).Equals(Data.Slice(24, 14));
    }

    public ScreenResponsePacket(ReadOnlyMemory<byte> data)
    {
        MaxNumber = data.Slice(27, 1).Span[0];
        Number = data.Slice(23).Span[0];
        Size = BinaryPrimitives.ReadInt16BigEndian(data.Slice(0x00000026).Span);
        Data = data.Slice(44, Size);
    }
    
    public static List<Memory<byte>> CreatePackets(Memory<byte> file, int chunkSize = 1400)
    {
        throw new NotImplementedException();
    }
}

public class ScreenRequestPacket : ESharePacket
{
    private static readonly ReadOnlyMemory<byte> mem;

    static ScreenRequestPacket()
    {
        mem = Convert.FromHexString("0000001a00000000000000000000000c0000000067657453637265656e436170000000000000000000000000000000000000");
    }

    public override ReadOnlyMemory<byte> GetBytes()
    {
        return mem;
    }
}