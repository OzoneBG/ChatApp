namespace ChatServer
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class ClientHandler
    {
        private TcpClient client;

        private Logger log;

        private Thread messagesThread;

        public BroadcastDelegate broadcastDelegate;

        public DisconnectUserDelegate disconnectDelegate;

        public int Id { get; set; }

        public string Username { get; set; }

        public TcpClient Client
        {
            get { return this.client; }
            set { this.client = value; }
        }

        public ClientHandler(TcpClient client, Logger log, int id, string username)
        {
            this.client = client;
            this.log = log;
            this.Id = id;
            this.Username = username;
            broadcastDelegate = new BroadcastDelegate(ServerProgram.BroadcastMessage);
            disconnectDelegate = new DisconnectUserDelegate(ServerProgram.DisconnectUser);
            Initialize();
        }

        private void Initialize()
        {
            messagesThread = new Thread(RecieveMessages);
            messagesThread.Start();
        }

        public void AbortThread()
        {
            this.messagesThread.Abort();
        }

        private void RecieveMessages()
        {
            while (true)
            {
                if (client != null)
                {
                    string recievedData = string.Empty;

                    try
                    {
                        NetworkStream netStream = client.GetStream();
                        byte[] buffer = new byte[256];
                        netStream.Read(buffer, 0, buffer.Length);
                        recievedData = Encoding.ASCII.GetString(buffer);


                        recievedData = recievedData.Substring(0, recievedData.IndexOf('$'));

                        if (recievedData.Contains("True") && recievedData.Contains("dc"))
                        {
                            disconnectDelegate(this.Id);
                        }
                        else
                        {
                            log.Write(string.Format("{0} sent {1}", this.Username, recievedData));
                            Console.WriteLine("{0} sent a message.", this.Username);
                            string formattedMessage = string.Format("{0}: {1}", this.Username, recievedData);
                            broadcastDelegate(formattedMessage);
                        }

                        buffer = null;

                        

                        netStream.Flush();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //Console.WriteLine("Recieved data was too large!");
                    }
                    catch (IOException)
                    {
                        log.Write("Lost connection to client!");
                        Console.WriteLine("Lost connection to client!");
                        return;
                    }
                    catch(ObjectDisposedException)
                    {
                        return;
                    }
                }
            }
        }
    }
}
