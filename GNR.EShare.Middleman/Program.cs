// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using GNR.EShare.Middleman;

Console.WriteLine("Hello, World!");
Socket s = null!;
string pw = "172.16.2.37";
string no = "172.16.4.0";
TcpProxy p = new TcpProxy(new IPEndPoint(IPAddress.Parse(no), 8121));
p.Start();

Console.ReadLine();

return;

Task.Run(() =>
{
    int udpStart = 40000;
    int udpEnd = 49999;

    int tcpStart = 50000;
    int tcpEnd = 59999;


    TcpListener tcpIn = new TcpListener(IPAddress.Loopback, 8121);

    tcpIn.Start();
    Console.WriteLine("Listening on port 8121");

    while (true)
    {
        TcpClient client = tcpIn.AcceptTcpClient();
        Task.Run(async () =>
        {
            Console.WriteLine("Client Accepted");

            TcpClient tcpOut = new TcpClient();

            tcpOut.Connect("172.16.2.157", 8121);
            var stream = client.GetStream();
            Console.WriteLine("Stream read");

            var inputTask = Task.Run(async () =>
            {
                Console.WriteLine("Input Task Started");
                var ms = new MemoryStream();
                Memory<byte> b = new Memory<byte>();
                stream.CopyTo(ms);
                
                Console.WriteLine("Received: " + Encoding.UTF8.GetString(ms.ToArray()));
                Console.WriteLine(b.Length);
            });

            var outputTask = Task.Run(async () =>
            {
                return;
                Memory<byte> b = null!;
                var outStream = tcpOut.GetStream();
                while (await outStream.ReadAsync(b) != 0)
                {
                    await stream.WriteAsync(b);
                    Console.WriteLine("Received: " + Encoding.UTF8.GetString(b.ToArray()));
                }
            });
            await Task.WhenAll(inputTask, outputTask);
            tcpOut.Dispose();
            client.Dispose();
        });
    }
});

Console.ReadLine();