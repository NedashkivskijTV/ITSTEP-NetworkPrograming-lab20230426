using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ProductLibrary;

namespace ServerProducts
{
    public partial class Form1 : Form
    {
        // �������� �������� Product
        List<Product> products;

        public Form1()
        {
            InitializeComponent();
            Text = "Server";

            // �����������/���������� �������� Product ���������� (������� ������ � ��)
            products = new List<Product>();
            products.Add(new Product("Lemon", "yellow", 11));
            products.Add(new Product("Pineapple", "sweet", 15));
            products.Add(new Product("Orange", "juicy", 17));
            
            // ������ ������ (����������) ��� ��������� ���������� ���������� (dataGridView1) ���� ���� ��������
            //ListUpdate(products);
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            // ������������ Task - ���� ������, �� ����������� � ��������
            Text = "Server was started!";
            tbServerInfo.BeginInvoke(new Action<string>(UpdateTextBox), "Server was started and ready to send products!\r\n");

            // ��������� �� ������ ������
            Task.Run(async () =>
            {
                // C�������� ������ ����� "� ���� �����"
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 1024);
                IPEndPoint endPoint = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[2], 1024);

                // �������� ����� �� ���� ������� - ���������� ����������
                Socket socket_TAP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP); // ��������� ������

                // �������� ������ �� ������ �����
                socket_TAP.Bind(endPoint); // (�������� ����� ���������������� ��� � ������ ����� - socket ���� �������� �� ������ �����)

                // �����������/������ ������ � ����� ���������������
                socket_TAP.Listen(10);


                // ���� ��������� ���������������, ���������/�������� ����� 
                // � ������������� Task ������
                try
                {
                    while (true)
                    {
                        // ��������� ������ ��� ��������� �볺���
                        Socket ns = await socket_TAP.AcceptAsync();
                        tbServerInfo.BeginInvoke(new Action<string>(UpdateTextBox), $"Client {ns.RemoteEndPoint} was connected !");

                        // ���������� ����� ��� �������� �볺��� - ��в�˲��ֲ� �����
                        byte[] buff = Encoding.Default.GetBytes(JsonSerializer.Serialize<List<Product>>(products));
                        
                        // �������� ����� � ���������� ������� �������� ��� � ������
                        int len = await ns.SendAsync(new ArraySegment<byte>(buff), SocketFlags.None);

                        // ��������� ��� ��� ������� ����������� �볺��� �����
                        tbServerInfo.BeginInvoke(new Action<string>(UpdateTextBox), $"{len} bytes was send to {ns.RemoteEndPoint}");

                        // �������� �'������
                        ns.Shutdown(SocketShutdown.Both); // ��������� ��� ���������
                        ns.Close(); // �������� ������
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                //// ����� ���������� (������ ���������� ��������� ������)
                //Console.ReadLine();
            });

        }

        private void UpdateTextBox(string str)
        {
            StringBuilder builder = new StringBuilder(tbServerInfo.Text);
            builder.Append("\r\n" + str);
            tbServerInfo.Text = builder.ToString();
        }
    }
}