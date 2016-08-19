namespace ChatClient
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    public partial class MainWindow : Window
    {
        private TcpClient client;

        private bool bHasConnected;

        private int Id;

        public MainWindow()
        {
            InitializeComponent();

            client = new TcpClient();
            bHasConnected = false;

        }


        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!bHasConnected)
            {
                client.Connect("188.254.209.106", 8888);
                bHasConnected = true;

                //send username data
                string defaultUsername = "Anonymous";

                string username = tbx_Username.Text;

                NetworkStream stream = client.GetStream();

                byte[] buffer;

                string finalUsername;

                if (username != string.Empty)
                {
                    finalUsername = username;
                    
                }
                else
                {
                    finalUsername = defaultUsername;
                }

                buffer = Encoding.ASCII.GetBytes(finalUsername + "$");

                stream.Write(buffer, 0, buffer.Length);

                //recieve id on a new thread
                Thread recieveIdThread = new Thread(RecieveId);
                recieveIdThread.Start();

                //Create a thread to listen for messages
                Thread messageListenerThread = new Thread(GetMessages);
                messageListenerThread.Start();

                tbx_Chat.Text = "Connected to chat server...";
            }
        }

        private void RecieveId()
        {
            //recieve id on a new thread
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, buffer.Length);

            string id = Encoding.ASCII.GetString(buffer);

            this.Id = int.Parse(id);
        }

        private void GetMessages()
        {
            while(true)
            {
                try
                {
                    NetworkStream stream = this.client.GetStream();
                    byte[] buffer = new byte[256];
                    stream.Read(buffer, 0, buffer.Length);

                    string data = Encoding.ASCII.GetString(buffer);

                    data = data.Substring(0, data.IndexOf('$'));

                    Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            //string oldText = tbx_Chat.Text;
                            //string newText = oldText + Environment.NewLine + ">> " + data;
                            //tbx_Chat.Text = newText;
                            tbx_Chat.AppendText(Environment.NewLine + ">> " + data);
                            tbx_Chat.CaretIndex = tbx_Chat.Text.Length;
                            tbx_Chat.ScrollToEnd();
                        }
                    ));
                }
                catch(IOException)
                {

                }
                catch(ObjectDisposedException)
                {

                }
                catch(InvalidOperationException)
                {
                    MessageBox.Show("Server went offline");
                    return;
                }

        }
        }

        private void PopulateChatBox(string message)
        {
            tbx_Chat.Text = message;
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string message = tbx_Message.Text;
            if (message != string.Empty)
            {
                NetworkStream netStream = client.GetStream();

                message = message + '$';
                byte[] buffer = Encoding.ASCII.GetBytes(message);

                netStream.Write(buffer, 0, buffer.Length);
                netStream.Flush();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            if (bHasConnected)
            {
                //Disconnect id:
                string message = "dc" + this.Id + true + "$";

                //MessageBox.Show(message);

                var stream = this.client.GetStream();

                byte[] buffer = Encoding.ASCII.GetBytes(message);

                stream.Write(buffer, 0, buffer.Length);

                stream.Flush();

                client.Close();
            }

        }

        private void tbx_Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
                tbx_Message.Text = "";
            }
        }
    }
}
