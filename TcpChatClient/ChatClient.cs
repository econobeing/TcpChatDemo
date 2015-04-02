using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpChatClient
{
    public class ChatClient
    {
        private readonly TcpClient _clientSocket = new TcpClient();
        private readonly Mutex _streamMutex = new Mutex();
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

                string messageString = string.Empty;
                byte[] readBuffer = new byte[this._clientSocket.ReceiveBufferSize];

                if (!this._streamMutex.WaitOne(250))
                    continue;

                try
                {
                    if (this._serverStream.CanRead && this._serverStream.DataAvailable)
                    {
                        int bytesRead = this._serverStream.Read(readBuffer, 0, this._clientSocket.ReceiveBufferSize);
                        messageString += Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                        this._serverStream.Flush();
                    }
                }
                catch
                {
                    this._stopThreads = true;
                }
                finally
                {
                    this._streamMutex.ReleaseMutex();
                }

                if (!string.IsNullOrWhiteSpace(messageString))
                    Console.WriteLine(messageString);
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

                if (!this._streamMutex.WaitOne(250))
                    continue;

                try
                {
                    this._serverStream.Write(sendBytes, 0, sendBytes.Length);
                    this._serverStream.Flush();
                }
                catch
                {
                    this._stopThreads = true;
                }
                finally
                {
                    this._streamMutex.ReleaseMutex();
                }
            }
        }
    }
}