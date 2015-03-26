using System;

namespace TcpChatClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("\nEnter IP address of host: ");
            var ipString = Console.ReadLine();

            Console.Write("\nEnter host port: ");
            var portNum = int.Parse(Console.ReadLine());

            var chatClient = new ChatClient();
            chatClient.Start(ipString, portNum);
        }
    }
}