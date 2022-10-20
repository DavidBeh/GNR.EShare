using System.Net;
using System.Net.Sockets;

namespace GNR.EShare.Middleman;

public class UdpProxy
{
    private readonly IPEndPoint _remoteEndPoint;
    public UdpClient InnerListener { get; private set; }
    private SemaphoreSlim _lock = new SemaphoreSlim(1);
    
    
    
    public UdpProxy(IPEndPoint remoteEndPoint, int port)
    {
        _remoteEndPoint = remoteEndPoint;
        InnerListener = new UdpClient(port);
        _cancelSource = new CancellationTokenSource();
        _cancelToken = _cancelSource.Token;
    }

    private CancellationToken _cancelToken;
    private CancellationTokenSource _cancelSource;

    public void Start()
    {
        var notStarted = _lock.Wait(TimeSpan.FromSeconds(1));
        if (!notStarted) return;

        Task.Run(() =>
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                
            }
        });
    }
}