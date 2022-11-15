using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using GNR.EShare.Protocol;
using GNR.EShare.Protocol.Ops;

namespace GNR.EShare.Forms
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private HttpListener _listener;
        private TcpClient? _tcpClient;
        private int _port;
        private HashAlgorithm _hasher;
        private IServiceProvider _services;
        public Form1(IServiceProvider services)
        {
            _services = services;
            _hasher = SHA512.Create();

            InitializeComponent();
            //this.Refresh();
        }

        static int GetFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //AllocConsole();
            _listener = new HttpListener();
            _port = GetFreeTcpPort();
            _port = 8989;
            _listener.Prefixes.Add($"http://*:{_port}/");
            return;
            //_listener.Start();
            Task.Run(() =>
            {
                var ctx = _listener.GetContext();

                Task.Run(() => { Console.WriteLine("Request: " + ctx.Request); });
            });

            return;
            Image img;

            Task.Run(() =>
            {
                while (true)
                {
                    this.Invoke(() =>
                    {
                        Task.Delay(200).Wait();
                        foreach (ListViewItem listView1Item in listView1.Items)
                        {
                            Task.Delay(50).Wait();

                            var newIndex = (listView1Item.ImageIndex + 1) % 3;
                            Console.WriteLine(newIndex);
                            listView1Item.ImageIndex = newIndex;
                        }
                    });
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var eshare = new EShareDevice(IPAddress.Parse(textBox1.Text));
                eshare.SetVolume(1d);
                var vol = eshare.GetVolume();
                MessageBox.Show(this, $"{vol}", "Volume", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"{exception}", "Volume", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(exception);
            }
            

            return;
            var dialog = new OpenFileDialog();
            var res = dialog.ShowDialog(this);
            if (res != DialogResult.OK) return;
            _tcpClient?.Dispose();
            _tcpClient = new TcpClient();
            _tcpClient.Connect(textBox1.Text, 8121);


            var str = _tcpClient.GetStream();

            var message = ProtocolMessgesHelper.PushFileRequest("hello.bmp", "image/bmp");
            var portMessage = ProtocolMessgesHelper.SetHttpPortRequest(_port);
            str.Write(portMessage);
            str.Write(message);
            Console.WriteLine(Convert.ToHexString(message));
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}