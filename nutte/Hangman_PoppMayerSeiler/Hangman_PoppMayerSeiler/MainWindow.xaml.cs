using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Hangman_PoppMayerSeiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int port = 8001;
        static TcpListener listener = new TcpListener(IPAddress.Any, port);

        private bool gameStart = false;
        private int HangmanLvl = -1;
        private int LettersFound = 0;
        private string TheWordToGuess;

        Rectangle[] HangmanLvlArray = new Rectangle[10];

        TextBox[] LetterArray = new TextBox[10];
        Rectangle[] LetterUnderlineArray = new Rectangle[10];
        List<string> AlreadyUseLetters = new List<string>();




        public MainWindow()
        {
            InitializeComponent();
            InitGraphic();
        }

        private void InitGraphic()
        {
            HangmanLvlArray[0] = HangmanLvl0;
            HangmanLvlArray[1] = HangmanLvl1;
            HangmanLvlArray[2] = HangmanLvl2;
            HangmanLvlArray[3] = HangmanLvl3;
            //Achtung Lvl4 ist ellipse
            HangmanLvlArray[5] = HangmanLvl5;
            HangmanLvlArray[6] = HangmanLvl6;
            HangmanLvlArray[7] = HangmanLvl7;
            HangmanLvlArray[8] = HangmanLvl8;
            HangmanLvlArray[9] = HangmanLvl9;


            LetterArray[0] = TBLetter0;
            LetterArray[1] = TBLetter1;
            LetterArray[2] = TBLetter2;
            LetterArray[3] = TBLetter3;
            LetterArray[4] = TBLetter4;
            LetterArray[5] = TBLetter5;
            LetterArray[6] = TBLetter6;
            LetterArray[7] = TBLetter7;
            LetterArray[8] = TBLetter8;
            LetterArray[9] = TBLetter9;

            LetterUnderlineArray[0] = LetterUnderline0;
            LetterUnderlineArray[1] = LetterUnderline1;
            LetterUnderlineArray[2] = LetterUnderline2;
            LetterUnderlineArray[3] = LetterUnderline3;
            LetterUnderlineArray[4] = LetterUnderline4;
            LetterUnderlineArray[5] = LetterUnderline5;
            LetterUnderlineArray[6] = LetterUnderline6;
            LetterUnderlineArray[7] = LetterUnderline7;
            LetterUnderlineArray[8] = LetterUnderline8;
            LetterUnderlineArray[9] = LetterUnderline9;


            deleteHangman();
            deleteLetters();
        }

        private void deleteHangman()
        {
            for(int i = 0; i < 10; i++)
            {
                if(i != 4)
                    HangmanLvlArray[i].Visibility = Visibility.Hidden;
            }

            HangmanLvl4.Visibility = Visibility.Hidden;
            HangmanLvl = -1;
        }

        private void deleteLetters()
        {
            for (int i = 0; i < 10; i++)
            {
                LetterArray[i].Text = "";
                LetterArray[i].Visibility = Visibility.Hidden;
                LetterUnderlineArray[i].Visibility = Visibility.Hidden;
            }
            AlreadyUseLetters.Clear();
            LettersFound = 0;
        }

        private void InitGame()
        {
            for (int i = 0; i < TheWordToGuess.Length; i++)
            {
                LetterArray[i].Visibility = Visibility.Visible;
                LetterUnderlineArray[i].Visibility = Visibility.Visible;
            }
        }


        


        private void Btn_GameStart_Click(object sender, RoutedEventArgs e)
        {
            if (!gameStart && TBMyWord.Text != "" && TBMyWord.Text.Length < 10)
            {
                TheWordToGuess = TBMyWord.Text;
                TheWordToGuess = TheWordToGuess.ToUpper();
                TBMyWord.Text = "";
                gameStart = true;

                InitGame();



                listener.Start();

                Thread ListenThread = new Thread(ListenWorker);
                ListenThread.Start();
                
            }
        }



        private void Btn_GameStop_Click(object sender, RoutedEventArgs e)
        {
            if (gameStart)
            {
                listener.Stop();
                gameStart = false;
                deleteHangman();
                deleteLetters();
            }
        }

        private void analyseWord(string LetterReceived)
        {
            if(AlreadyUseLetters.Contains(LetterReceived))
            {
                return;
            }
            else
            {
                AlreadyUseLetters.Add(LetterReceived);

                Dispatcher.Invoke(() => { TBUsedLetters.Text = ""; });
                foreach (String Letter in AlreadyUseLetters)
                {
                    Dispatcher.Invoke(() => { TBUsedLetters.Text += Letter; });
                }

                checkMyWordForLetter(LetterReceived);
            }
        }

        private void checkMyWordForLetter(string LetterReceived)
        {
            if(TheWordToGuess.Contains(LetterReceived))
            {
                int lenght;
                int index = 0;
                lenght = TheWordToGuess.Length;

                while(true)
                {
                    index = TheWordToGuess.IndexOf(LetterReceived, index);
                    if (index != -1)
                    {                      
                        Dispatcher.Invoke(() => { LetterArray[index].Text = LetterReceived; });
                        index++;
                        LettersFound++;
                    }
                    else
                        break;
                }
          
            }
            else
            {
                //HangmanLvl erhöhen
                HangmanLvl++;
                if (HangmanLvl == 4)
                {
                    Dispatcher.Invoke(() => { HangmanLvl4.Visibility = Visibility.Visible; });
                }
                else
                    Dispatcher.Invoke(() => { HangmanLvlArray[HangmanLvl].Visibility = Visibility.Visible; });

            }
        }



        void clientWorker(Object o)
        {
            TcpClient client = (TcpClient)o;
            string dataReceived;
            string LetterReceived;

            do
            {
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                Debug.WriteLine("--> Wait for data");
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                Debug.WriteLine("--> Data received");

                dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if(dataReceived.Length > 1)
                {
                    LetterReceived = dataReceived.Substring(0, 1);
                }
                else
                    LetterReceived = dataReceived;

                LetterReceived = LetterReceived.ToUpper();

                analyseWord(LetterReceived);

            } while (HangmanLvl != 9 && LettersFound != TheWordToGuess.Length);

            Debug.WriteLine("--> Disconnect");
            if (HangmanLvl == 9)
            {
                Dispatcher.Invoke(() => { MessageBox.Show("You Lost"); });
            }
            else
            {
                Dispatcher.Invoke(() => { MessageBox.Show("You Won"); });
            }
            client.Close();
        }

        void ListenWorker()
        {
            while (true)
            {
                Debug.WriteLine("--> Wait for connection");
                TcpClient client = listener.AcceptTcpClient();
                Debug.WriteLine("--> Connected");


                // ParameterizedThreadStart ...
                Thread clientThread = new Thread(clientWorker);
                clientThread.Start(client);
            }
        }
    }
}
