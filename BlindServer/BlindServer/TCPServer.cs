using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace BlindServer
{
    public class TcpServer : IDisposable
    {
        private static ulong _counter;
        

        private const int BufferSize = 1024;
        public const int MaxConnections = 8;
        
        
        [Serializable]
        public struct Message
        {
            public ulong sender;
            public byte functionName;
            public byte[] content;
        }

        XmlSerializer _serializer = new(typeof(Message[]));
        //private readonly IPHostEntry _hostInfo = Dns.GetHostEntry(Dns.GetHostName());
        private readonly IPEndPoint _serverEndPoint;

        private readonly Socket _server;
        

        //It's possible we accidentally overwrite the same buffer we're reading...
        //We should use a seperate buffer for input than for output... or have a flush interval.
        byte[] _receiveBuffer = new byte[BufferSize]; // Surely there's a better way to clear

        public TcpServer(IPAddress ip, int port)
        {
            _serverEndPoint = new IPEndPoint(ip, port);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Starting up server");
            Console.ResetColor();

            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveBufferSize = BufferSize,
                SendBufferSize = BufferSize
            };
            try
            {
                _server.Bind(_serverEndPoint);
                _server.Listen(MaxConnections);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed during server initializing: " + e);
                Console.ResetColor();
                return;
            }

            _spawnPoints = new();
            for (int i = 14; i >= 0; --i) { // There are 14 spawn points.
                _spawnPoints.Add(i);
                Console.WriteLine($"Adding SpawnPoint {i}");
            }
            
            HandleConnections();
            HeartBeat();
        }

        private async void HandleConnections()
        {
            string name;
            Socket user;
            try
            {
                Console.WriteLine(_serverEndPoint);

                Console.WriteLine("Pending Connection...");
                user = await _server.AcceptAsync();
                Console.WriteLine("Connection Received!");
                int t = await user.ReceiveAsync(_receiveBuffer, SocketFlags.None);
                
                name = Encoding.UTF8.GetString(_receiveBuffer,0, t);

                if (Server.TcpClients.ContainsKey(name))
                {
                    string? str;
                    await using (var xa = new StringWriter())
                    {
                        _serializer.Serialize(xa, new Message[]
                        {
                            new ()
                            {
                                sender = 0,
                                functionName = 6,
                                content = Encoding.UTF8.GetBytes("Name already taken")
                            }
                        });
                        str = xa.ToString();
                    }
                    Console.WriteLine(str);

                    await user.SendAsync(Encoding.UTF8.GetBytes(str ?? throw new InvalidOperationException()), SocketFlags.None);
                    await user.DisconnectAsync(false);
                    Console.WriteLine("Name already taken: " + name);
                    HandleConnections(); // Repeat for all eternity...
                    return;
                }


                Console.WriteLine("Successfully registered: " + name);

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed at HandleConnections: " + e);
                Console.ResetColor();
                return;
            }

            int n = Server.TcpClients.Count;
            //Send server information.

            string id = user.RemoteEndPoint!.ToString()!;

            if (!Server.TcpClients.TryAdd(name, new Tuple<ulong, Socket>(++ _counter, user)))
            {
                await _server.DisconnectAsync(true);
                HandleConnections(); // Repeat for all eternity...
                return;
            }

            await UpdateServerInfo(name);
            await Task.Delay(100); // Stop interleaving, because I'm too lazy to fix
            await using (TextWriter writer = new StringWriter())
            {
                byte[] x = BitConverter.GetBytes(GetSpawnPoint(name));
                byte[] y = Server.GetAllIds(_counter).SelectMany(BitConverter.GetBytes).ToArray();
                byte[] z = new byte[x.Length + y.Length];
                x.CopyTo(z, 0);
                y.CopyTo(z, x.Length);
                _serializer.Serialize(writer, new Message[]{ new(){ 
                    sender = _counter,
                   functionName = 0, 
                   content = Encoding.UTF8.GetBytes($"{name} has connected to the server. There are {n+1} clients connected.")
                },
                new(){ 
                    sender = _counter,
                    functionName = 1, 
                    content =  z//Send over all currently connected users.
                }
                });
                //Update all client player counts.
                await SendMessageToAll(ulong.MaxValue, writer.ToString());
            }
          

            HandleConnections(); // Repeat for all eternity...
            Console.WriteLine("Clients have been told about: " + name +", ID: " + id);

            OnServerListUpdated();

        }

        public async Task UpdateServerInfo(string? name = null)
        {
            string? str;
            await using (TextWriter writer = new StringWriter()){
                XmlSerializer ser = new XmlSerializer(typeof(ServerInfo));
                ser.Serialize(writer, new ServerInfo()
                {
                    LocalUserId = _counter,
                    TcpMilliDelay = Server.TCPMilliDelay,
                    UdpMilliDelay = Server.UDPMilliDelay
                });
                str = writer.ToString();
            }
            await using TextWriter writer2 = new StringWriter();
            _serializer.Serialize(writer2, new Message[]{ new(){ 
                    sender = 0,
                    functionName = 2, 
                    content = Encoding.UTF8.GetBytes(str ?? throw new InvalidOperationException())
                }
            });
           
            //string? bts = writer.ToString();
            //serializer.Serialize(writer, new Message(0, bts)); //XML serialization must go first... That hinders like everything...

            if(string.IsNullOrEmpty(name)) await SendMessageToAll(ulong.MaxValue,writer2.ToString());
            else await SendMessageToClient(name, writer2.ToString());

        }
        
        public async void BeginGame()
        {
            Message []m = 
            {
                new ()
                {
                    sender = 0,
                    functionName = 4,
                    content = BitConverter.GetBytes(StartTime)
                }
            };
            string? str;
            await using (TextWriter tw = new StringWriter())
            {
                _serializer.Serialize(tw, m);
                str = tw.ToString();
            }

            await SendMessageToAll(0,str);
        }

        public const double StartTime = 3;


        private void OnServerListUpdated()
        {
            Console.WriteLine("---------------Client list updated-------------------");
            foreach (var pair in Server.TcpClients)
            {
                Console.WriteLine(pair.Key + ": " + pair.Value.Item2.RemoteEndPoint);
            }
            Console.WriteLine("-----------------------------------------------------");
        }


        //Check if all clients are in the lobby.
        //Check if any new messages have been received.
        private async void HeartBeat()
        {
            while (true)
            {
                await Task.Delay(Server.TCPMilliDelay);
                var list = Server.TcpClients.ToArray();

                foreach (var client in list)
                {
                    //Handle bad disconnections.
                    if (client.Value.Item2.Poll(1000, SelectMode.SelectRead) && client.Value.Item2.Available == 0) //If there are no bytes pending.
                    {
                        Console.WriteLine("Client was disconnected, " + client.Key);
                        Server.TcpClients.Remove(client.Key);
                        AddBackSpawnPoint(Server.ClientSpawnPoints[client.Key]);
                        Server.ClientSpawnPoints.Remove(client.Key);
                        if (Server.TcpClients.Count > 0)
                        {
                            await using (TextWriter writer = new StringWriter())
                            {
                                _serializer.Serialize(writer, new Message[]{ new (){
                                    sender =  client.Value.Item1, // We need to delete the object
                                    functionName = 3, 
                                    content = Encoding.UTF8.GetBytes($"{client.Key} has disconnected from the server. There are {Server.TcpClients.Count} clients connected.")
                                }});
                                //Update all client player counts.
                                await SendMessageToAll(ulong.MaxValue, writer.ToString());
                            }
                        }

                        OnServerListUpdated();
                        continue;
                    }

                    //
                    if (client.Value.Item2.Available == 0) continue;
                    int len = await client.Value.Item2.ReceiveAsync(_receiveBuffer, SocketFlags.None);
                    if (len == 0) continue;
                    
                    //We actually have zero clue what is in here... and we don't care. We're just here to relay.
                    await SendMessageToAll(client.Value.Item1, _receiveBuffer);
                }
            }
        }

        #region Messaging

        private async Task SendMessageToAll(ulong ignore, string? message)
        {
            byte[] b = Encoding.UTF8.GetBytes(message);
            await SendMessageToAll(ignore,  b);
        }

        private async Task SendMessageToClient(string target, string? message)
        {
            byte[] b = Encoding.UTF8.GetBytes(message);
            await SendMessageToClient(target, b);
        }

        private async Task SendMessageToAll(ulong ignore, byte[] message)
        {
            foreach (var client in Server.TcpClients)
            {
                if(client.Value.Item1.Equals(ignore)) continue;
                await SendMessageToClient(client.Key, message);
            }
        }

        private async Task SendMessageToClient(string target, byte[] message)
        {
            Console.WriteLine("Sending Message to: " + target + ", " + Encoding.UTF8.GetString(message));
            if(Server.TcpClients.TryGetValue(target, out var s)) 
                await s.Item2.SendAsync(message, SocketFlags.None);
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("TCP: Client is already disconnected: " + target );
                Console.ResetColor();
            }
            _receiveBuffer = new byte[BufferSize]; // Surely there's a better way to clear
        }
        #endregion

        #region Management

        

     
        //Try to make sure everything is getting cleaned up!
        private void CloseServer()
        {
            _server.Shutdown(SocketShutdown.Both);
            _server.Close();
            Console.WriteLine("Server shutting down");
        }

        private void Dispose(bool disposing)
        {
            CloseServer();
            if (disposing)
            {
                _server?.Dispose();
            }
        }

        public void Dispose()
        {
            CloseServer();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TcpServer()
        {
            Dispose(false);
        }
        #endregion

        #region Spawning
        private static List<int> _spawnPoints;
    
    
        public static void AddBackSpawnPoint(int num)
        {
                        Console.WriteLine("Adding Back Spawn Point "+ num);

            //Sort from highest to lowest as we're always adding and reading from the back like a stack.
            if (_spawnPoints.Count == 0)
            {
                _spawnPoints.Add(num);
                return;
            }
            //Sort, because we know with full certainty that numbers can only ever be 1 number out of place. We can do a single insert.
            //Worst case O(2n) meaning a min heap is faster, but aint nobody got time for that.
            for (int i = _spawnPoints.Count - 1; i >= 0; --i)
            {
                //3,5,6 //ADD 4.
                Console.WriteLine($"Compare: {num}, > {_spawnPoints[i]}");
                if (num < _spawnPoints[i])
                {
                    _spawnPoints.Insert(i+1, num);
                    break;
                }
            }
        }
    
        public static int GetSpawnPoint(string userName)
        {

            int k = _spawnPoints.Count - 1;
            if (k == -1)
            {
                return 0;
            }
            int n = _spawnPoints[k];
            _spawnPoints.RemoveAt(k);
            Server.ClientSpawnPoints.Add(userName, n);
            Console.WriteLine($"Trying to place {userName} at {n}");
            return n;
            
           
        }
        

        #endregion
       
    }
}