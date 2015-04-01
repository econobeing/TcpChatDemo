using System;
using System.Net;

namespace TcpChatServer
{
    public class Program
    {
        private static int ChoosePortNumber()
        {
            var port = 0;
            while (port <= 0 || port > 65535)
            {
                Console.Write("\nChoose Port Number: ");
                var input = Console.ReadLine();
                try
                {
                    port = int.Parse(input);
                }
                catch { }

                if (port <= 0 || port > 65535)
                    Console.WriteLine("Invalid choice, please try again");
            }

            return port;
        }

        private static IPAddress ChooseServerIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            var i = 0;
            foreach (var ip in host.AddressList)
            {
                Console.WriteLine("({0}) - {1}", i, ip);
                i++;
            }

            var choice = -2;
            while (choice < -1 || choice >= host.AddressList.Length)
            {
                Console.Write("\nChoose an IP address to start the server on, -1 to start on localhost: ");
                var input = Console.ReadLine();
                try
                {
                    choice = int.Parse(input);
                }
                catch { }

                if (choice < -1 || choice >= host.AddressList.Length)
                {
                    Console.WriteLine("Invalid choice, please try again");
                }
            }
            Console.WriteLine("");

            if (choice == -1)
                return new IPAddress(new byte[] { 127, 0, 0, 1 });

            return host.AddressList[choice];
        }

        private static void Main(string[] args)
        {
            Console.Title = "TCP Chat Server";
            var ip = ChooseServerIp();
            var port = ChoosePortNumber();

            var server = new ChatServer(ip, port);
        }
    }
}