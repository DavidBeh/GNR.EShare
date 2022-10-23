// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using GNR.EShare.Protocol;

Console.WriteLine("Hello, World!");


var client = new EShareClient(IPAddress.Parse("172.16.2.185"));

while (true)
{
    var scr = client.GetScreenCapture().Result;
    Console.WriteLine("SCR: " + scr.ToString());
    File.WriteAllBytes("scr.jfif", scr.ToArray());
    Process.Start(new ProcessStartInfo("scr.jfif") { UseShellExecute = true });
    Console.ReadLine();
}