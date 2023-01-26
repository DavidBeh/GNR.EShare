// See https://aka.ms/new-console-template for more information
using System;
using System.Runtime.InteropServices;
using EShareDemo;
using System.Net;
using System.IO;
using System.Reflection;

Console.WriteLine("IP Eingeben");

var ipstr = Console.ReadLine();


IPAddress ipaddr = IPAddress.Parse(ipstr);
EShareClient c = new EShareClient(ipaddr);
var cap = await c.GetScreenCapture();
File.WriteAllBytes("screencapture.png", cap.ToArray());
System.Diagnostics.Process.Start("explorer.exe", "screencapture.png");