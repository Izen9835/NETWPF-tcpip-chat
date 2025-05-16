using ChatClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectedEvent;

        private CancellationTokenSource _cts;

        // Constructor
        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);

                // instantiate packet reader only if we manage to establish connection
                PacketReader = new PacketReader(_client.GetStream());

                if (!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }

                _cts = new CancellationTokenSource();


                ReadPackets();

            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var opcode = PacketReader.ReadByte();
                        switch (opcode)
                        {
                            case 1:
                                connectedEvent?.Invoke();
                                break;
                            case 5:
                                msgReceivedEvent?.Invoke();
                                break;
                            case 10:
                                userDisconnectedEvent?.Invoke();
                                break;
                            case 11:
                                // the ACK signal from server that we can safely disconnect
                                DisconnectFromServer();
                                break;
                            default:
                                Console.WriteLine("Unknown opcode received");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ReadPackets Exception: {ex.Message}");
                        break;
                    }
                }

                Console.WriteLine("Packet reading task stopped.");
            }, _cts.Token);
        }

        public void DisconnectFromServer()
        {
            if (_client.Connected)
            {
                _cts.Cancel(); // to stop the while loop in ReadPackets
                _client.Close(); //disconnect from TCP Server
                _client = new TcpClient(); // reset to allow new connection later
            }
        }


        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            // opcode 5 is for sending a text message
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendDisconnectMessageToServer()
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(10);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
