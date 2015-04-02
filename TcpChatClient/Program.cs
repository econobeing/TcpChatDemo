using System;

namespace TcpChatClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "TCP Chat Client";

            Console.Write("\nEnter IP address of host: ");
            string ipString = Console.ReadLine();

            Console.Write("\nEnter host port: ");
            int portNum = int.Parse(Console.ReadLine());

            ChatClient chatClient = new ChatClient();
            chatClient.Start(ipString, portNum);
        }
    }
}