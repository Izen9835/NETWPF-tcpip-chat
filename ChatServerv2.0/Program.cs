using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7891);
            TcpListener listener = new(ipEndPoint);

            try
            {
                listener.Start();
                Console.WriteLine("Server started. Waiting for connection...");

                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await using NetworkStream stream = handler.GetStream();

                var message = $"📅 {DateTime.Now} 🕛";
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes);

                Console.WriteLine($"Sent message: \"{message}\"");
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("Server stopped.");
            }
        }
    }
}
