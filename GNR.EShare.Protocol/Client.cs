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

    private int _screenCaputureFlag = 0;
    private UdpClient _udpClient = new UdpClient();

    
    public void StartScreenCapture(TimeSpan? frequency = null)
    {
        var old = Interlocked.Exchange(ref _screenCaputureFlag, 1);
        if (old == 0)
        {

            Task.Run(async () =>
            {
                if (_screenCaputureFlag == 0) return;

                var res = await _udpClient.SendAsync(new ScreenCaptureRequestESharePacket().GetBytes());
                _udpClient.re
            });
        }
    }

    public void StopScreenCapture()
    {
        Interlocked.Exchange(ref _screenCaputureFlag, 0);
    }
    
    

    public async Task<ReadOnlyMemory<byte>> GetScreenCapture()
    {
        TaskCompletionSource<ReadOnlyMemory<byte>> b = new TaskCompletionSource<ReadOnlyMemory<byte>>();
        var t = b.Task;
        var c = Interlocked.CompareExchange(ref _currentScreenCaputureTask, t, null);

        if (c == null)
        {
            Task.Run(() =>
            {
                
            });
        }

        return c ?? t;
        
        await _screenCaptureLock.WaitAsync();
        try
        {
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public class ScreenCaptureEventArgs : EventArgs
    {
        public ReadOnlyMemory<byte> Data { get; set; }
    }

    public event EventHandler<ScreenCaptureEventArgs> ScreenCaptureReceived;
}

