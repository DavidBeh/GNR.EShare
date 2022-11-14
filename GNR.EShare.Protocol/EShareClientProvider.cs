using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNR.EShare.Protocol
{


    internal class EShareClientProvider
    {
        private static readonly Dictionary<IPEndPoint, EShareClient> _clients = new ();

        private readonly SemaphoreSlim _lock = new(2);

        public async Task GetClientAsync(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
            _lock.WaitAsync();
            try
            {
                EShareClient? cl = null;
                var cachedEp = _clients.Keys.FirstOrDefault(point => point == endPoint || point.Equals(endPoint));
                if (cachedEp == null)
                {
                    cl = new EShareClient(endPoint.Address);
                    _clients.Add(endPoint, cl);
                }
                else
                {

                }
                
                
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
