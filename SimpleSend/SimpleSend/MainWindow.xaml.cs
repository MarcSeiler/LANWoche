using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleSend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ServerIP = "127.0.0.1";
        int ServerPort = 8001;
        TcpClient client = null;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            if(client == null)
                client = new TcpClient();
            client.Connect(ServerIP, ServerPort);
        }

        private void Btn_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
                client.Close();
            client = null;

        }

        private void Btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                string toSend = TextSendBox.Text;
                byte[] bytes = ASCIIEncoding.UTF8.GetBytes(toSend);

                NetworkStream nwStream = client.GetStream();
                nwStream.Write(bytes, 0, bytes.Length);
                nwStream.Flush();
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
