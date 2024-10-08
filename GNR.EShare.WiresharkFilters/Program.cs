﻿// See https://aka.ms/new-console-template for more information

using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Documents;

var boards = new List<string>()
{
    "172.16.2.131",
    "172.16.2.37",
    "172.16.2.110",
    "172.16.2.45",
    "172.16.2.220",
    "172.16.2.182",
    "172.16.2.185",
    "172.16.2.132",
    "172.16.2.97",
    "172.16.4.0",
    "172.16.4.2",
    "172.16.1.1" // Gateway (Http)
};

var self = new List<string>
{
    "172.16.2.77", //PC
    "172.16.2.47", // i
};

var ingoing = from board in boards
    from s in self
    select $"(ip.src == {board} and ip.addr == {s})";

var outgoing = from board in boards
    from s in self
    select $"(ip.src == {s} and ip.addr == {board})";
    //select $"(ip.src == {s})";

bool Xingoing = true;
bool Xutgoing = true;

var ges = new List<string>();

if (Xingoing) ges.AddRange(ingoing);
if (Xutgoing) ges.AddRange(outgoing);

var res = string.Join(" or ", ges);


var additional =
    "!(tcp.analysis.retransmission or mdns or frame contains \"ECloudBox516Pro:\" or dns) && (not tcp or tcp.len > 0)";
//var options = "(!(frame contains \"check failed\") && !(frame contains \"CHECKPASSWORD\") && data.len > 0 && data.len < 300)";
var spam = "!(frame contains \"eartbeat\")";
var windowSpam = "!(frame contains \"CHECKPASSWORD\" || frame contains \"check failed\")";
res = $"({res}) && ({additional}) && {spam} && {windowSpam}"; //&& ({spam}) && ({windowSpam}))";
//  || ip.src == {boards[0]} || ip.addr == {boards[0]})


Console.WriteLine(res);
int failCount = 0;
retry:
var t = new Thread(() =>
{
    System.Windows.Clipboard.SetText(res);
    Console.BackgroundColor = ConsoleColor.DarkGreen;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Copied to clipboard");
    Console.ResetColor();
});
t.SetApartmentState(ApartmentState.STA);
try
{
    t.Start();
}
catch (Exception e)
{
    if (failCount > 2) throw;
    Console.WriteLine(e);
    failCount++;
    goto retry;
}
