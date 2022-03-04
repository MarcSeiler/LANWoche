using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace SimpleReader2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        int ServerPort = 8000;
        TcpListener server = null;
        Thread worker = null;
        Byte[] bytes = new Byte[256];
        String data = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_ListenStart_Click(object sender, RoutedEventArgs e)
        {
            if (server == null)
            {
                server = new TcpListener(localAddr, ServerPort);
                if(worker == null)
                    worker = new Thread(CheckData);

                server.Start();
                worker.Start();
            }
        }

        private void Btn_ListenStop_Click(object sender, RoutedEventArgs e)
        {
            if(server != null)
            {
                server.Stop();
                worker.Abort();
            }
        }

        void CheckData()
        {
            Console.Write("Waiting for a connection... ");

            // Perform a blocking call to accept requests.
            // You could also use server.AcceptSocket() here.
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Connected!");

            data = null;

            NetworkStream stream = client.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", data);

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                string text;
                text = System.Text.Encoding.UTF8.GetString(msg, 0, msg.Length);

                Dispatcher.Invoke(() =>
                {
                    ReceivedTextBox.Text = Convert.ToString(text);
                });
            }
        }
    }
}
