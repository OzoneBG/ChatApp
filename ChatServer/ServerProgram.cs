namespace ChatServer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public delegate void BroadcastDelegate(string message);

    public delegate void DisconnectUserDelegate(int id);

    class ServerProgram
    {
        static TcpListener server;
        static TcpClient client;
        static Logger log;
        static Dictionary<int, ClientHandler> connectedClients;
        static int totalConnections;

        static void Main()
        {
            Init();

            ResetTitle();

            Thread connsThread = new Thread(AcceptConnections);
            connsThread.Start();
        }

        public static void DisconnectUser(int id)
        {
            ClientHandler client;
            connectedClients.TryGetValue(id, out client);
            client.Client.Close();
            connectedClients.Remove(id);
            log.Write(string.Format("Client id {0} disconnected!", client.Id));
            Console.WriteLine("Client id {0} disconnected!", client.Id);
            ResetTitle();
        
        }

        public static void BroadcastMessage(string message)
        {
            foreach (var kvPair in connectedClients)
            {
                ClientHandler client = kvPair.Value;

                NetworkStream netStream = client.Client.GetStream();

                byte[] buffer = Encoding.ASCII.GetBytes(message + '$');

                netStream.Write(buffer, 0, buffer.Length);
            }
        }

        private static void ResetTitle()
        {
            Console.Title = "Server " + connectedClients.Count + " connections";
        }

        static void AcceptConnections()
        {
            while(true)
            {
                client = server.AcceptTcpClient();

                if (client.Connected)
                {
                    //1. Connect client
                    log.Write("New client connection request!");
                    Console.WriteLine("New client connection request!");
                    totalConnections++;

                    //2. Recieve connecting information
                    /* TO DO: RECIEVE USERNAME */
                    NetworkStream stream = client.GetStream();

                    byte[] buffer = new byte[256];

                    stream.Read(buffer, 0, buffer.Length);

                    string recievedUsername = Encoding.ASCII.GetString(buffer);

                    recievedUsername = recievedUsername.Substring(0, recievedUsername.IndexOf("$"));

                    //3. Send current Id               
                    string data = totalConnections.ToString();

                    buffer = Encoding.ASCII.GetBytes(data);

                    stream.Write(buffer, 0, buffer.Length);

                    //4. Create a client with that information and add it to list
                    ClientHandler handler = new ClientHandler(client, log, totalConnections, recievedUsername);

                    connectedClients.Add(totalConnections, handler);

                    log.Write(string.Format("Client {0} connected!", recievedUsername));
                    Console.WriteLine("Client {0} connected!", recievedUsername);

                    BroadcastMessage(string.Format("{0} joined the chatroom!", recievedUsername));

                    //5. Flush the stream
                    stream.Flush();

                    //6. Reset Title  
                    ResetTitle();
                }
            }
        }

        static void Init()
        {
            log = new Logger();
            var ip = IPAddress.Parse("192.168.0.101");
            server = new TcpListener(ip, 8888);
            server.Start();
            log.Write(string.Format("Server started on {0}", server.LocalEndpoint.ToString()));
            Console.WriteLine("Server started on {0}", server.LocalEndpoint.ToString());

            connectedClients = new Dictionary<int, ClientHandler>();

            totalConnections = 0;
        }
    }
}
