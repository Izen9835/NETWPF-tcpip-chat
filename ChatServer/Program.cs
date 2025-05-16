using ChatServer.Net.IO;
using System;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                // AcceptTcpClient is a blocking call that waits for a new connection
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

                /* Broadcast connection to everyone */
                BroadcastConnection();

            }

        }

        static void BroadcastConnection()
        {
            // broadcast to each user that is in the list _users
            // contents of the broadcast is information on who is connected to the server
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    // opcode 1 is for telling clients that new user has connected
                    broadcastPacket.WriteOpCode(1);
                    // confused about what exactly usr is here
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }

            }
        }

        public static void BroadcastMessage(string message)
        {
            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }

        public static void BroadcastDisconnect(string uid)
        {


            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                // opcode 10 is for announcing disconnect
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                Console.WriteLine(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());

            }
            BroadcastMessage($"[{DateTime.Now}]: [{disconnectedUser.Username}] has disconnected");
        }
    }
}