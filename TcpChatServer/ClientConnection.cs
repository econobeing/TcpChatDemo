using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpChatServer
{
    public class ClientConnection
    {
        private readonly ChatServer _server;
        private readonly Object _streamLock = new Object();
        private TcpClient _client;
        private NetworkStream _clientStream;
        private bool _isConnected = true;

        public ClientConnection(ChatServer server)
        {
            this._server = server;
        }

        private string ClientNum { get; set; }

        public void SendMessage(string msg)
        {
            //encode the message
            byte[] sendBytes = Encoding.UTF8.GetBytes(msg);

            if (!this._isConnected)
                return;

            lock (this._streamLock)
            {
                try
                {
                    this._clientStream.Write(sendBytes, 0, sendBytes.Length);
                    this._clientStream.Flush();
                }
                catch
                {
                    //drop for any exception
                    this._isConnected = false;
                    Console.WriteLine(">> [{0}] - Exception caught while sending to client, stopping connection",
                        this.ClientNum);
                }
            }
        }

        public void StartClient(TcpClient client, string clientNum)
        {
            this.ClientNum = clientNum;
            this._client = client;
            this._clientStream = client.GetStream();

            Thread receiveThread = new Thread(this.ReceiveMessagesThread);
            receiveThread.Start();
        }

        private void ReceiveMessagesThread()
        {
            while (this._isConnected)
            {
                //sleep for just a little bit to not eat up the CPU
                Thread.Sleep(1);

                byte[] readBuffer = new byte[this._client.ReceiveBufferSize];
                int bytesRead = 0;

                lock (this._streamLock)
                {
                    try
                    {
                        //if there's data in the stream, get it
                        if (this._clientStream.CanRead && this._clientStream.DataAvailable)
                        {
                            bytesRead = this._clientStream.Read(readBuffer, 0, this._client.ReceiveBufferSize);
                            this._clientStream.Flush();
                        }
                    }
                    catch
                    {
                        this._isConnected = false;
                        Console.WriteLine(">> [{0}] - Exception caught while reading from client, stopping connection",
                            this.ClientNum);
                    }
                }

                if (bytesRead <= 0)
                    continue;

                try
                {
                    string dataFromClient = Encoding.UTF8.GetString(readBuffer, 0, bytesRead);

                    //if there was actually data received, tell the server to broadcast it
                    if (!string.IsNullOrWhiteSpace(dataFromClient))
                    {
                        dataFromClient = string.Format("[{0}]: {1}", this.ClientNum, dataFromClient);
                        this._server.BroadcastMessage(dataFromClient);
                    }
                }
                catch
                {
                    Console.WriteLine("Error encoding message");
                }
            }
        }
    }
}