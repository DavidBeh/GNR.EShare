using System.ComponentModel;
using System.Net.Sockets;

namespace GNR.EShare.Protocol.Ops;

public class ScreenCaptureResponsePacket
{
    public readonly byte Number;
    public readonly byte Total;
    public readonly ReadOnlyMemory<byte> Data;

    public ScreenCaptureResponsePacket(ReadOnlyMemory<byte> data)
    {
        Number = data.Slice(0x00000017, 1).ToArray()[0];
        Total = data.Slice(0x0000001B, 1).ToArray()[0];
        Data = data.Slice(0x0000001C);
    }
}

public class ScreenCaptureRequestESharePacket : ESharePacket
{
    private static readonly ReadOnlyMemory<byte> mem;

    static ScreenCaptureRequestESharePacket()
    {
        mem = Convert.FromHexString("0000001a00000000000000000000000c0000000067657453637265656e436170000000000000000000000000000000000000");
    }

    public override ReadOnlyMemory<byte> GetBytes()
    {
        return mem;
    }
}

