using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpChatClient
{
    public class ChatClient
    {
        private readonly TcpClient _clientSocket = new TcpClient();
        private readonly Object _streamLock = new Object();
        private NetworkStream _serverStream;
        private bool _stopThreads;

        public void Start(string host, int port)
        {
            this._clientSocket.Connect(host, port);
            this._serverStream = this._clientSocket.GetStream();

            Thread listenerThread = new Thread(this.ListenerThread);
            listenerThread.Start();

            Thread sendThread = new Thread(this.SendMessageThread);
            sendThread.Start();
        }

        private void ListenerThread()
        {
            while (!this._stopThreads)
            {
                //sleep for just a little bit to not eat up the CPU
                Thread.Sleep(1);

                byte[] readBuffer = new byte[this._clientSocket.ReceiveBufferSize];
                int bytesRead = 0;

                lock (this._streamLock)
                {
                    try
                    {
                        if (this._serverStream.CanRead && this._serverStream.DataAvailable)
                        {
                            bytesRead = this._serverStream.Read(readBuffer, 0, this._clientSocket.ReceiveBufferSize);
                            this._serverStream.Flush();
                        }
                    }
                    catch
                    {
                        this._stopThreads = true;
                    }
                }

                if (bytesRead <= 0)
                    continue;

                try
                {
                    string messageString = Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                    if (!string.IsNullOrWhiteSpace(messageString))
                        Console.WriteLine(messageString);
                }
                catch
                {
                    Console.WriteLine("Error encoding message");
                }
            }
        }

        private void SendMessageThread()
        {
            while (!this._stopThreads)
            {
                string msg = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                byte[] sendBytes = Encoding.UTF8.GetBytes(msg);

                lock (this._streamLock)
                {
                    try
                    {
                        this._serverStream.Write(sendBytes, 0, sendBytes.Length);
                        this._serverStream.Flush();
                    }
                    catch
                    {
                        this._stopThreads = true;
                    }
                }
            }
        }
    }
}