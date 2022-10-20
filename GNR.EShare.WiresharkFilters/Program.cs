// See https://aka.ms/new-console-template for more information

using System.Windows.Documents;

var s = "";

List<string> boards = new List<string>()
{
    "172.16.2.131",
    "172.16.2.37",
    "172.16.2.110",
    "172.16.2.45",
    "172.16.2.220",
    "172.16.2.97",
    "172.16.4.0",
};

string self = "172.16.2.171";

var ingoing = from board in boards
    select $"(ip.src == {board} and ip.addr == {self})";

var outgoing = from board in boards
    select $"(ip.src == {self} and ip.addr == {board})";

bool Xingoing = true;
bool Xutgoing = true;

var ges = new List<string>();

if (Xingoing) ges.AddRange(ingoing);
if (Xutgoing) ges.AddRange(outgoing);

var res = string.Join(" or ", ges);

var additional =
    "!(tcp.analysis.retransmission or mdns or frame contains \"ECloudBox516Pro:\" or dns) && (not tcp or tcp.len > 0)";
var options = "(frame contains \"rtsp\" or frame contains \"OPTIONS\")";
res = $"({res}) && ({additional})";



Console.WriteLine(res);
var t = new Thread(() => System.Windows.Clipboard.SetText(res));
t.SetApartmentState(ApartmentState.STA);
t.Start();