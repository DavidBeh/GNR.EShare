using System.Net;
using System.Net.Sockets;
using GNR.EShare.Protocol.Ops;

namespace GNR.EShare.Protocol;

public class EShareClient
{
    public readonly IPEndPoint EndPoint;

    public EShareClient(IPAddress ipAddress, int port = 48689)
    {
        EndPoint = new IPEndPoint(ipAddress, port);
        _volumeClient = new TcpClient();
        _screenShareClient = new UdpClient();
        _screenShareClient.Connect(EndPoint);
        _screenShareClient.Client.ReceiveTimeout = 2000;
    }

    public SemaphoreSlim _screenCaptureLock = new SemaphoreSlim(1);

    private int _screenCaptureCount = 0;
    private UdpClient _screenShareClient;
    private TcpClient _volumeClient;
    private SemaphoreSlim _volumeLock;
    private int currentVolume;
    private int maxVolume;
    private DateTime lastVolumeUpdate = DateTime.MinValue;
    private Task<ReadOnlyMemory<byte>>? _currentScreenCapture;

    public int GetVolume(int val, TimeSpan? old = null)
    {
        old ??= TimeSpan.FromSeconds(1);
        _volumeLock.Wait();
        try
        {
            if (lastVolumeUpdate < DateTime.Now - old)
            {
                
            }
        }
        finally
        {
            _volumeLock.Release();
        }

        return currentVolume / currentVolume;
    }

    //private DateTimeOffset _lastSencCaptureReq = DateTimeOffset.MinValue;

    private ReadOnlyMemory<byte> GetCaptureScreen2()
    {
        ScreenResponsePacket[]? packets = null;
        int highestNumber = 0; // The highest packNumber that has been accepted (O+)
        int counter = 0; // The number of packets that have been accepted so far
        int size = 0; // The total size of the image
        int? packetsCount = null; // The number of packets that are expected (1+)
        IPEndPoint remoteEp = new IPEndPoint(EndPoint.Address, EndPoint.Port);

        _screenShareClient.Send(new ScreenRequestPacket().GetBytes().Span);
        // When the Server did not get a request for some time, the first request will be ignored =>
        // Let's call this "it is in sleep mode"
        // So we send a second request
        bool allowSecondPacket = true;

        while (packetsCount == null || counter < packetsCount)
        {
            retry:
            Memory<byte> mem;
            try
            {
                mem = _screenShareClient.Receive(ref remoteEp).AsMemory();
            }
            catch (Exception e)
            {
                // The Request timed out, maybe because of sleep mode, so we retry
                // But only once
                if (!allowSecondPacket) throw;
                _screenShareClient.Send(new ScreenRequestPacket().GetBytes().Span);
                goto retry;
            }
            finally
            {
                // Set a flag, that a first packet has already been sent, regardless whether it was ignored or not.
                allowSecondPacket = false;
            }
            
            // Parse the packet
            var packet = new ScreenResponsePacket(mem);
            Console.WriteLine(packet.Number);
            
            // If the packet seems odd, like as it was from a previous request, we ignore it.
            // Each client intance can only have one request at a time.
            // If a client is requested for a screen capture while it is already capturing one,
            // It will return the first one.
            // It is very unlikely, that packets are out of order, so this should be fine
            if (packet.Number > highestNumber + 5)
                continue;
            
            // If the highest number 
            highestNumber = Math.Max(packet.Number, highestNumber);

            // Initializes the packets array, if it is not already initialized
            if (packets == null)
            {
                packetsCount = packet.MaxNumber + 1; 
                packets = new ScreenResponsePacket[packetsCount.Value];

            }
            
            // Updates size- and count-tracker, and stores the packet 
            size += packet.Size;
            packets[packet.Number] = packet;
            counter++;
        }
        
        // Puts the packet toogether
        Memory<byte> memBuffer = new Memory<byte>(new byte[size]);
        size = 0;
        
        foreach (var packet in packets)
        {
            var d = memBuffer.Slice(size, packet.Size);
            packet.Data.CopyTo(d);
            size += packet.Size;
        }
        

        return memBuffer;
    }

    public async Task<ReadOnlyMemory<byte>> GetScreenCapture()
    {
        await _screenCaptureLock.WaitAsync();
        try
        {
            if (_currentScreenCapture == null || _currentScreenCapture.IsCompleted)
                _currentScreenCapture = Task.Run(GetCaptureScreen2);
        }
        finally
        {
            _screenCaptureLock.Release();
        }

        return await _currentScreenCapture;
    }


    /*
    private void UpdateScreenCapture(int count)
    {
        var old = Interlocked.Exchange(ref _screenCaptureCount, count);
        if (old == 0)
        {
            Task.Run(async () =>
            {
                Interlocked
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
    */

    public class ScreenCaptureEventArgs : EventArgs
    {
        public ReadOnlyMemory<byte> Data { get; set; }
    }

    public event EventHandler<ScreenCaptureEventArgs> ScreenCaptureReceived;
}