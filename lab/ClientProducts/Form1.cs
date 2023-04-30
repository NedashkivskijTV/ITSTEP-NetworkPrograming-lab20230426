using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ProductLibrary;

namespace ClientProducts
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Text = "Client";

            // ������ ������� ���� ��������� � �������� �볺��� - ��� ��������
            Process.Start("ServerProducts.exe");
        }

        private void btnGetProducts_Click(object sender, EventArgs e)
        {
            // ��������� �� ������ ������
            Task.Run(async () =>
            {
                // C�������� ������ ����� "� ���� �����"
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 1024);
                IPEndPoint endPoint = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[2], 1024);

                // �������� ����� �� ���� �볺��� - ����������� �� ������� �� �������/������ ���
                Socket socket_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP); // ��������� ������

                // ���� ���������� �� ������� �� ��������� ����� 
                // � ������������� Task ������
                try
                {
                    // ϳ��������� �� �������
                    socket_client.Connect(endPoint);

                    // �������� �'������� (�� �����������)
                    if (socket_client.Connected)
                    {
                        // ---------------------------------- ²������� �����
                        // ��������� ������
                        string query = "GET\r\n";

                        // ��������� ��������� ������
                        byte[] buff = Encoding.Default.GetBytes(query);

                        // ³������� ������ � ������������� ������������
                        await socket_client.SendAsync(new ArraySegment<byte>(buff), SocketFlags.None);


                        // ---------------------------------- ��������� �����
                        // ��������� ������ ��������� ������ ��� ��������� ����� - ��������� ��������� ������
                        byte[] buff_receive = new byte[1024];

                        // ����� ��� ���������� ��������� �����
                        string data;
                        // �����. �� �������� ������� �������� ��� � ������
                        int len;

                        // ���� ������������ �����
                        do
                        {
                            len = await socket_client.ReceiveAsync(buff_receive, SocketFlags.None);

                            // ���������� ���������� ����� (� ������������� ���������)
                            data = Encoding.Default.GetString(buff_receive, 0, len);

                        } while (socket_client.Available > 0);

                        // �������� ����� � �������� ������ (������������ �����)
                        List<Product> products = JsonSerializer.Deserialize<List<Product>>(data.ToString());

                        // ��������� �������/������������� �������� � ��������� ��������� ���������
                        // (����������� ����������/�������������� �����������)
                        dgvProductsCollection.BeginInvoke(new Action<List<Product>>(ListUpdate), products);

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    // �������� �'������
                    socket_client.Shutdown(SocketShutdown.Both); // ��������� ��� ���������
                    socket_client.Close(); // �������� ������
                }

                // ����� ���������� (������ ���������� ��������� ������)
                Console.ReadLine();
            });

        }

        private void ListUpdate(List<Product> products)
        {
            dgvProductsCollection.DataSource = null;
            dgvProductsCollection.DataSource = products;
        }
    }
}