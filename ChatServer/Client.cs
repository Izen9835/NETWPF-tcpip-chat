using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        // for stopping Task.Run()
        private CancellationTokenSource _cts = new CancellationTokenSource();



        // Constructor
        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();

            // Use PacketReader to parse payload
            _packetReader = new PacketReader(ClientSocket.GetStream());
            Console.WriteLine($"[{DateTime.Now}] New client UID: {UID}");


            var opcode = _packetReader.ReadByte();

            // consider adding opcode validation
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{ DateTime.Now }]: Client has connected with the username: {Username}");

            Task.Run(() => Process(_cts.Token));
        }

        void Process(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5: // receive message
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message received {msg}");
                            Program.BroadcastMessage($"[{DateTime.Now}]: [{Username}]: {msg}");
                            break;
                        case 10: // receive disconnect request
                            // Broadcast a disconnect packet
                            // Although, it never actually manages to reach this line
                            // always crashes on _packetReader.ReadByte()
                            // "Cannot access a disposed object"
                            Console.WriteLine($"[{DateTime.Now}]: [{UID.ToString()}] ({Username}) has disconnected.");
                            Program.BroadcastDisconnect(UID.ToString());

                            // send disconnect-ack to user
                            var ackPack = new PacketBuilder();
                            ackPack.WriteOpCode(11);
                            ClientSocket.Client.Send(ackPack.GetPacketBytes());
                            _cts.Cancel();

                            ClientSocket.Close();
                            break;
                        default:
                            break;
                    }

                } 
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex}");
                    _cts.Cancel();
                    ClientSocket.Close();
                }
            }
        }
    }
}
