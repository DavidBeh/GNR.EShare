using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

[assembly: InternalsVisibleTo("GNR.EShare.Forms")]

namespace GNR.EShare.Protocol.Ops
{
    public static class ProtocolMessgesHelper
    {
        private static readonly byte[] _setVolBegin;
        private static readonly byte[] _getVol;


        private static readonly byte[] _mirrorTouchBegin;

        private static readonly byte[] _messageEnd;
        private static readonly byte[] _mediaControlPlay;
        private static readonly byte[] _mediaControlPause;

        private static readonly byte[] _pushFileBegin;
        private static readonly byte[] _pushFileEnd;

        private static readonly byte[] _setHttpPortBegin;
        private static readonly byte[] _setHttpPortEnd;


        static ProtocolMessgesHelper()
        {
            _setVolBegin = new byte[]
            {
                0x4D, 0x65, 0x64, 0x69, 0x61, 0x43, 0x6F, 0x6E, 0x74, 0x72, 0x6F, 0x6C, 0x0D, 0x0A, 0x73, 0x65,
                0x74, 0x56, 0x6F, 0x6C, 0x75, 0x6D, 0x65, 0x20
            };

            _getVol = new byte[]
            {
                0x4D, 0x65, 0x64, 0x69, 0x61, 0x43, 0x6F, 0x6E, 0x74, 0x72, 0x6F, 0x6C, 0x0D, 0x0A, 0x67, 0x65,
                0x74, 0x56, 0x6F, 0x6C, 0x75, 0x6D, 0x65, 0x20, 0x0D, 0x0A, 0x0D, 0x0A
            };


            _messageEnd = new byte[]
            {
                0x0D, 0x0A, 0x0D, 0x0A
            };

            _mirrorTouchBegin = new byte[]
            {
                0x4D, 0x49, 0x52, 0x52, 0x4F, 0x52, 0x54, 0x4F, 0x55, 0x43, 0x48, 0x45, 0x56, 0x45, 0x4E, 0x54,
                0x0D, 0x0A, 0x32, 0x0D, 0x0A, 0x7B
            };

            _pushFileBegin = new byte[]
            {
                0x4F, 0x70, 0x65, 0x6E, 0x66, 0x69, 0x6C, 0x65, 0x0D, 0x0A, 0x4F, 0x70, 0x65, 0x6E, 0x20
            };

            _pushFileEnd = new byte[]
            {
                0x20, 0x0D, 0x0A
            };

            _setHttpPortBegin = new byte[]
            {
                0x61, 0x75, 0x74, 0x68, 0x0D, 0x0A, 0x74, 0x65, 0x73, 0x74, 0x20
            };

            _setHttpPortEnd = new byte[]
            {
                0x20, 0x45, 0x53, 0x68, 0x61, 0x72, 0x65, 0x20, 0x0D, 0x0A, 0x0D, 0x0A
            };
        }


        public static ReadOnlySpan<byte> ChangeVolumeRequest(int value)
        {
            var str = value.ToString().AsSpan();

            Span<byte> b = new byte[_setVolBegin.Length + _messageEnd.Length + str.Length];

            _setVolBegin.CopyTo(b);
            var cursor = _setVolBegin.Length;

            Encoding.ASCII.GetBytes(str, b.Slice(cursor));
            cursor += str.Length;

            _messageEnd.CopyTo(b.Slice(cursor));
            return b;
        }

        public static ReadOnlySpan<byte> GetVolumeRequest()
        {
            return _getVol.AsSpan();
        }

        public static (int Value, int Max) GetVolumeResponse(ReadOnlySpan<byte> response)
        {
            var sep = response.IndexOf((byte)0x2F);
            var valStr = Encoding.ASCII.GetString(response.Slice(0, sep));
            var maxStr = Encoding.ASCII.GetString(response.Slice(sep + 1));
            return (int.Parse(valStr), int.Parse(maxStr));
        }

        internal static ReadOnlySpan<byte> TouchControlRequest(double x, double y, RequestType request)
        {
            var xx = (x * 1280).ToString("F6", CultureInfo.InvariantCulture).AsSpan();
            var yy = (y * 720).ToString("F6", CultureInfo.InvariantCulture).AsSpan();

            Span<byte> b = new byte[xx.Length + yy.Length + 3 + _mirrorTouchBegin.Length];

            var r = Encoding.ASCII.GetBytes(((int)request).ToString()).AsSpan();
            _mirrorTouchBegin.CopyTo(b);
            r.CopyTo(b.Slice(18));

            int cursor = _mirrorTouchBegin.Length;
            cursor += Encoding.ASCII.GetBytes(xx, r.Slice(cursor));

            b[cursor++] = 0x2C; // ,
            cursor += Encoding.ASCII.GetBytes(yy, r.Slice(cursor));
            b[cursor] = 0x7D; // }

            return b;
        }

        public static ReadOnlySpan<byte> PushFileRequest(string name, string mimeType)
        {
            var encodedPath = "/" + HttpUtility.UrlEncode(name);
            var mimeBytes = Encoding.ASCII.GetBytes(mimeType);
            Span<byte> b = new byte[_pushFileBegin.Length + encodedPath.Length + _pushFileEnd.Length +
                                    mimeBytes.Length + 1];
            _pushFileBegin.CopyTo(b);
            int cursor = _pushFileBegin.Length;
            cursor += Encoding.ASCII.GetBytes(encodedPath, b.Slice(cursor));
            b[cursor++] = 0x20;
            mimeBytes.CopyTo(b.Slice(cursor));
            cursor += mimeBytes.Length;
            _pushFileEnd.CopyTo(b.Slice(cursor));
            return b;
        }

        public static ReadOnlySpan<byte> SetHttpPortRequest(int port)
        {
            var ps = port.ToString();
            Span<byte> b = new byte[_setHttpPortBegin.Length + _setHttpPortEnd.Length + ps.Length];
            _setHttpPortBegin.CopyTo(b);
            int cursor = _setHttpPortBegin.Length;
            cursor += Encoding.ASCII.GetBytes(ps, b.Slice(cursor));
            _setHttpPortEnd.CopyTo(b.Slice(cursor));
            return b;
        }


        internal enum RequestType
        {
            Press = 0,
            Release = 1,
            Hold = 2,
        }
    }
}