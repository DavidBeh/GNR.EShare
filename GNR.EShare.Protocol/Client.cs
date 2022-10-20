using System.Net;
using System.Net.Sockets;
using GNR.EShare.Protocol.Ops;

namespace GNR.EShare.Protocol;

public class Client
{
    
    
    public Client(IPAddress ipAddress, int port = 8121)
    {
        
    }

    public SemaphoreSlim _screenCaptureLock = new SemaphoreSlim(1);

    private int _screenCaptureCount = 0;
    private UdpClient _udpClient = new UdpClient();

    private void UpdateScreenCapture(int count)
    {
        var old = Interlocked.Exchange(ref _screenCaptureCount, count);
        if (old == 0)
        {
            Task.Run(async () =>
            {
                Interlocked.
                await _udpClient.Send()
                    
                    
            });
        }
    }
    
    public async Task<ReadOnlyMemory<byte>> StartScreenCapture(int? count= null)
    {
        
        UdpClient c = new UdpClient();
        await _screenCaptureLock.WaitAsync();
        try
        {
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        c.SendAsync(new ScreenCaptureRequestESharePacket().GetBytes());
    }
    
    public class ScreenCaptureEventArgs : EventArgs
    {
        public ReadOnlyMemory<byte> Data { get; set; }
    }

    public event EventHandler<ScreenCaptureEventArgs> ScreenCaptureReceived;
}

