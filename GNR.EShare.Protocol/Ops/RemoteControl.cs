using System.Text;

namespace GNR.EShare.Protocol.Ops;

public class RemoteControlRequests
{
    private static readonly ReadOnlyMemory<byte> _getVol;
    private static readonly ReadOnlyMemory<byte> _setVol;
    private static readonly ReadOnlyMemory<byte> requestEnd;

    static RemoteControlRequests()
    {
        _getVol = Convert.FromHexString("4d65646961436f6e74726f6c0d0a676574566f6c756d65200d0a0d0a");
        _setVol = Convert.FromHexString("4d65646961436f6e74726f6c0d0a736574566f6c756d65203239");
        requestEnd = Convert.FromHexString("0d0a0d0a");

    }

    public ReadOnlyMemory<byte> GetCurrentVolume() => _getVol;

    public ReadOnlyMemory<byte> SetVolume(int volume)
    {
        var volStr = volume.ToString();
        var volBytes = Encoding.ASCII.GetBytes(volStr);
        var buffer = new Memory<byte>(new byte[_setVol.Length + volBytes.Length + requestEnd.Length]);
        _getVol.CopyTo(buffer);
        
        volBytes.AsMemory().CopyTo(buffer.Slice(_getVol.Length));
        requestEnd.CopyTo(buffer.Slice(_getVol.Length + volBytes.Length));
        
        return buffer;
    }
    
    
}

public class RemoteControlResponses
{
    public double? GetCurrentVolume(ReadOnlySpan<byte> b, out int value, out int max)
    {
        value = 0;
        max = 0;
        if (b.Length != 5 || b[2] != 0x2f)
        {

            return null;
        }

        try
        {
            var strV = Encoding.ASCII.GetString(b.Slice(0, 2));
            var strM = Encoding.ASCII.GetString(b.Slice(3, 2));
            
            return (double) int.Parse(strV) / int.Parse(strM);
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    
}

public class GetMedi