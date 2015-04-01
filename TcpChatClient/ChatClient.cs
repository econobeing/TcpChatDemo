using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpChatClient
{
    internal class ChatClient
    {
        private readonly TcpClient _clientSocket = new TcpClient();
        private readonly Mutex _streamMutex = new Mutex();
        private NetworkStream _serverStream;
        private bool _stopThreads;

        public void Start(string host, int port)
        {
            this._clientSocket.Connect(host, port);
            this._serverStream = this._clientSocket.GetStream();

            var listenerThread = new Thread(this.ListenerThread);
            listenerThread.Start();

            var sendThread = new Thread(this.SendMessageThread);
            sendThread.Start();
        }

        private void ListenerThread()
        {
            while (!this._stopThreads)
            {
                //sleep for just a little bit to not eat up the CPU
                Thread.Sleep(1);
                if (!this._streamMutex.WaitOne(250))
                    continue;

                var messageString = string.Empty;
                try
                {
                    if (this._serverStream.CanRead && this._serverStream.DataAvailable)
                    {
                        var readBuffer = new byte[this._clientSocket.ReceiveBufferSize];
                        var bytesRead = this._serverStream.Read(readBuffer, 0, this._clientSocket.ReceiveBufferSize);
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

                if (string.IsNullOrWhiteSpace(messageString))
                    continue;

                Console.WriteLine(messageString);
            }
        }

        private void SendMessageThread()
        {
            while (!this._stopThreads)
            {
                var msg = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                var sendBytes = Encoding.UTF8.GetBytes(msg);
                var sent = false;
                while (!sent)
                {
                    if (!this._streamMutex.WaitOne(250))
                        continue;

                    try
                    {
                        this._serverStream.Write(sendBytes, 0, sendBytes.Length);
                        this._serverStream.Flush();
                        sent = true;
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
}