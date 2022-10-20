// See https://aka.ms/new-console-template for more information

using System.Text;
using PacketDotNet;
using SharpPcap;

Console.WriteLine("Hello, World!");

foreach (var liveDevice in SharpPcap.CaptureDeviceList.Instance)
{
    if (liveDevice.MacAddress is null) continue;
    try
    {
        liveDevice.Open(DeviceModes.Promiscuous);
        liveDevice.Filter = "tcp and ip";
        
        liveDevice.OnPacketArrival += (sender, capture) =>
        {
            var rawPacket = capture.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            var tcpPacket = packet.Extract<TcpPacket>();
            if (tcpPacket != null)
            {
                
                var str = Encoding.UTF8.GetString(tcpPacket.PayloadPacket.PayloadData);
                Console.WriteLine(str);
            }
        };
        Task.Run(() => liveDevice.Capture());
        Console.WriteLine($"Capturing {liveDevice.Name} | {liveDevice.Description} | {liveDevice.MacAddress}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"FAILED Capturing {liveDevice.Name} | {liveDevice.Description} | {liveDevice.MacAddress}");
    }


}

Console.ReadLine();