using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace GNR.EShare.Protocol.Ops;

public class RemoteControlRequests
{
    private static readonly ReadOnlyMemory<byte> _getVol;
    private static readonly ReadOnlyMemory<byte> _setVol;
    private static readonly ReadOnlyMemory<byte> requestEnd;

    static RemoteControlRequests()
    {
        
        _getVol = Convert.FromHexString("4d65646961436f4D 65 64 69 61 43 6F 6E 74 72 6F 6C 0D 0A 73 65 74 56 6F 6C 75 6D 656e74726f6c0d0a676574566f6c756d65200d0a0d0a");

        _setVol = Convert.FromHexString("4d65646961436f6e74726f6c0d0a736574566f6c756d65203239");
        requestEnd = Convert.FromHexString("0d0a0d0a");
        

    }


    public static ReadOnlyMemory<byte> GetCurrentVolume() => _getVol;

    public static ReadOnlyMemory<byte> SetVolume(int volume)
    {
        var volStr = volume.ToString();
        var volBytes = Encoding.ASCII.GetBytes(volStr);
        var buffer = new Memory<byte>(new byte[5]);
        _getVol.CopyTo(buffer);

        volBytes.AsMemory().CopyTo(buffer.Slice(_getVol.Length));
        requestEnd.CopyTo(buffer.Slice(_getVol.Length + volBytes.Length));


        return buffer;
    }
    
    
}

public class RemoteControlResponses
{
    public static double? GetCurrentVolume(ReadOnlySpan<byte> b, out int value, out int max)
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