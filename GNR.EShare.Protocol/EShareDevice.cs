using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GNR.EShare.Protocol.Ops;

namespace GNR.EShare.Protocol;

public class EShareDevice : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    private int _maxVol = 30;
    private readonly TcpChannel _mediaSharingChannel;
    private TcpChannel _pushMessageChannel;

    private readonly IObservable<long> _scheduler;

    private readonly TcpChannel _volumeChannel;

    private readonly Subject<double> _volumeSubject;
    public readonly IObservable<double> VolumeObservable;

    private readonly Subject<PlaybackStatus> _playbackSubject;
    public readonly IObservable<PlaybackStatus> PlaybackObservable;

    public EShareDevice(IPEndPoint endPoint)
    {
        EndPoint = endPoint;

        _volumeChannel = new(EndPoint);
        _pushMessageChannel = new(EndPoint);
        _mediaSharingChannel = new(EndPoint);

        _volumeSubject = new();
        VolumeObservable = _volumeSubject.AsObservable();
        
        _playbackSubject = new();
        PlaybackObservable = _playbackSubject.AsObservable();

        _scheduler = Observable.Interval(TimeSpan.FromSeconds(1.5)).Where(l => Watch);
        _disposables.Add(
            _scheduler.Subscribe(l => { _volumeSubject.Publish(GetVolume()); })
        );
    }

    public EShareDevice(IPAddress address, int port = 8121) : this(new(address, port))
    {

        
    }

    public IPEndPoint EndPoint { get; }
    private Subject<double> VolumeSubject { get; }
    private bool Watch { get; set; }

    public void Dispose()
    {
        _volumeSubject.Dispose();
        VolumeSubject.Dispose();
    }

    public double GetVolume()
    {
        var response = _volumeChannel.SendAndRecieve(ProtocolMessgesHelper.GetVolumeRequest(), 7);
        var vol = ProtocolMessgesHelper.GetVolumeResponse(response);
        _maxVol = vol.Max;
        return (double)vol.Value / _maxVol;
    }

    public void PushFile(string name, string mimeType, int port)
    {
        _mediaSharingChannel.SendMessage(ProtocolMessgesHelper.SetHttpPortRequest(port));
        Task.Delay(200).Wait();
        _mediaSharingChannel.SendMessage(ProtocolMessgesHelper.PushFileRequest(name, mimeType));
    }


    public void SetVolume(double value)
    {
        GetVolume();
        _volumeChannel.SendMessage(
            ProtocolMessgesHelper.ChangeVolumeRequest((int)Math.Round(value * _maxVol,
                MidpointRounding.ToPositiveInfinity)));
    }

    // TCP_CHANNEL


    private class TcpChannel
    {
        private readonly IPEndPoint _endPoint;
        private TcpClient? _client;
        private readonly SemaphoreSlim _lock = new(1);

        public TcpChannel(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        private void Connect(bool forceReconnect = false)
        {
            _lock.Wait();
            try
            {
                if (forceReconnect || _client == null)
                {
                    _client = new();
                    _client.Connect(_endPoint);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public void SendMessage(ReadOnlySpan<byte> message)
        {
            _lock.Wait();
            try
            {
                if (_client == null)
                    Connect();
                try
                {
                    _client!.GetStream().Write(message);
                }
                catch (Exception e)

                {
                    _client!.Dispose();
                    _client.Close();
                    Connect(true);
                    throw;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public ReadOnlySpan<byte> SendAndRecieve(ReadOnlySpan<byte> message, int length)
        {
            _lock.Wait();
            try
            {
                SendMessage(message);
                Span<byte> buf = new byte[length];
                var len = _client.GetStream().Read(buf);
                return buf.Slice(0, len);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}

public record PlaybackStatus(TimeSpan? Current, TimeSpan? Duration);