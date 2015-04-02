using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpChatServer
{
    public class ChatServer
    {
        private readonly IList<ClientConnection> _clientConnections = new List<ClientConnection>();
        private readonly TcpListener _serverListener;

        public ChatServer(IPAddress addr, int port)
        {
            Console.WriteLine(">> Starting server on {0}:{1}", addr, port);
            this._serverListener = new TcpListener(addr, port);
            this._serverListener.Start();

            //start threads
            Thread clientConnectThread = new Thread(this.ConnectToClientsThread);
            clientConnectThread.Start();
        }

        public void BroadcastMessage(string msg)
        {
            //don't need a mutex because we're just reading
            Console.WriteLine(msg);
            foreach (ClientConnection clientConnection in this._clientConnections)
                clientConnection.SendMessage(msg);
        }

        private void ConnectToClientsThread()
        {
            int counter = 0;
            while (true)
            {
                //AcceptTcpClient will block until a new connection is made
                TcpClient client = this._serverListener.AcceptTcpClient();
                counter++;
                Console.WriteLine(">> Client #{0} joined", counter);

                ClientConnection clientConnection = new ClientConnection(this);
                clientConnection.StartClient(client, counter.ToString());
                this._clientConnections.Add(clientConnection);
            }
        }
    }
}