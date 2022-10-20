using System.ComponentModel;

namespace GNR.EShare.Protocol.Ops;

public abstract class ESharePacket
{
    public abstract ReadOnlyMemory<byte> GetBytes();
}