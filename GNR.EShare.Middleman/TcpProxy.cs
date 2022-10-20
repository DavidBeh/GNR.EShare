using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GNR.EShare.Middleman;

public class TcpProxy : IDisposable
{
    private readonly IPEndPoint _remoteEndPoint;
    public TcpListener InnerListener { get; private set; }
    private SemaphoreSlim _lock = new SemaphoreSlim(1);
    
    
    
    public TcpProxy(IPEndPoint remoteEndPoint, IPAddress? innerAddress = null)
    {
        _remoteEndPoint = remoteEndPoint;
        InnerListener = new TcpListener(innerAddress ?? IPAddress.Loopback, 8121);
        _cancelSource = new CancellationTokenSource();
        _cancelToken = _cancelSource.Token;
    }

    private CancellationToken _cancelToken;
    private CancellationTokenSource _cancelSource;

    public void Start()
    {
        InnerListener.Start();
        var notStarted = _lock.Wait(TimeSpan.FromSeconds(1));
        if (!notStarted) return;

        Task.Run(async () =>
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                try
                {
                    var client = await InnerListener.AcceptTcpClientAsync(_cancelToken);
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            var outClient = new TcpClient();
                            outClient.Connect(_remoteEndPoint);
                            var inStream = client.GetStream();
                            inStream.ReadTimeout = 1000;
                            var outStream = outClient.GetStream();
                            var inTask = Task.Run(() => inStream.CopyToAsync(outStream));
                            var outTask = Task.Run(() =>
                            {
                                byte[] buffer = new byte[1024];
                                int read = 0;
                                do
                                {
                                    read = outStream.Read(buffer, 0, buffer.Length);
                                    inStream.Write(buffer, 0, read);
                                } while (read > 0);
                            });
                            Task.WaitAll(inTask, outTask);
                        }
                        finally
                        {
                            client.Close();
                        }
                    }, _cancelToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }, _cancelToken);
        
    }

    public void Dispose()
    {
        _cancelSource.Cancel();
    }
}