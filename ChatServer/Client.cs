using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        
        // Used to specify which client you want to perform actions on
        public Guid UID { get; set; }

        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;


        // Constructor
        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();

            // Use PacketReader to parse payload
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();

            // consider adding opcode validation
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{ DateTime.Now }]: Client has connected with the username: {Username}");

            Task.Run(() => Process());
        }

        void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message received {msg}");
                            Program.BroadcastMessage($"[{DateTime.Now}]: [{Username}]: {msg}");
                            break;
                        default:
                            break;
                    }

                } 
                catch (Exception)
                {
                    // Broadcast a disconnect packet
                    Console.WriteLine($"[{UID.ToString()}] has disconnected.");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                }
            }
        }
    }
}
