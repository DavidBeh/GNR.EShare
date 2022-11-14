using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GNR.EShare.Protocol.Ops;

namespace GNR.EShare.Protocol
{
    public class EShareDevice
    {
        public IPEndPoint EndPoint { get; }

        private TcpChannel _mediaControlChannel;

        public EShareDevice(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            _mediaControlChannel = new TcpChannel(EndPoint);
        }

        public EShareDevice(IPAddress address, int port = 8121) : this(new IPEndPoint(address, port))
        {
        }

        // METHODS

        private int _maxVol = 30;

        public double GetVolume()
        {
            var response = _mediaControlChannel.SendAndRecieve(ProtocolMessgesHelper.GetVolumeRequest(), 7);
            var vol = ProtocolMessgesHelper.GetVolumeResponse(response);
            _maxVol = vol.Max;
            return (double) vol.Value / _maxVol;
        }
        
        public void SetVolume(double value)
        {
            GetVolume();
            _mediaControlChannel.SendMessage(ProtocolMessgesHelper.ChangeVolumeRequest((int) Math.Round(value * _maxVol, MidpointRounding.ToPositiveInfinity)));
        }

        // TCP_CHANNEL

        private class TcpChannel
        {
            private readonly IPEndPoint _endPoint;
            private TcpClient? _client;

            public TcpChannel(IPEndPoint endPoint)
            {
                _endPoint = endPoint;
            }

            private void Connect(bool forceReconnect= false)
            {
                if (forceReconnect || _client == null)
                {
                    _client = new TcpClient();
                    _client.Connect(_endPoint);
                }
            }

            public void SendMessage(ReadOnlySpan<byte> message)
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

            public ReadOnlySpan<byte> SendAndRecieve(ReadOnlySpan<byte> message, int length)
            {
                SendMessage(message);
                Span<byte> buf = new byte[length];
                var len = _client.GetStream().Read(buf);
                return buf.Slice(0, len);
            }

        }
    }
}