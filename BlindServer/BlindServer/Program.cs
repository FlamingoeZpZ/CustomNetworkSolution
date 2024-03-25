// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;

namespace BlindServer;

[Serializable]
public struct ServerInfo
{
    public int TcpMilliDelay;
    public int UdpMilliDelay;
    public ulong LocalUserId;
}

public static class Server
{
    
    
    public static readonly Dictionary<string, Tuple<ulong,Socket>> TcpClients = new();
    public static readonly HashSet<EndPoint> UdpClients = new();
    private static readonly string directory = "Settings.dat";
    public static int TCPMilliDelay = 20;
    public static int UDPMilliDelay = 20;
    
    public static List<ulong> GetAllIds(ulong ignore)
    {
        // Extracting the ulong part of the Tuple from each KeyValuePair in the dictionary
        var ulongValues = TcpClients.Where(kvp => kvp.Value.Item1 != ignore).Select(kvp => kvp.Value.Item1).ToList();
        return ulongValues;
    }
    
    public static void Main()
    {
        TcpClients.Clear();
        IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

        
        
        
        Console.WriteLine("Choose an IP from the list below");
        for (var index = 0; index < hostInfo.AddressList.Length; index++)
        {
            var VARIABLE = hostInfo.AddressList[index];
            Console.WriteLine(index+") "+ VARIABLE);
        }

        if (!ReadIntFromFile(directory, out int y) || y < 0 || y >= hostInfo.AddressList.Length)
        {
            do
            {
                while (!int.TryParse(Console.ReadLine(), out y))
                {
                    Console.WriteLine("Invalid input. Please enter an integer.");
                }
            }
            while (y < 0 || y >= hostInfo.AddressList.Length);
        }

        WriteIntToFile(directory, y);
        
        IPAddress ip = hostInfo.AddressList[y];
        TcpServer tcp = new TcpServer(ip, 8888);
        UdpServer udp = new UdpServer(ip, 8889);
        while (true)
        {
            string? str  = Console.ReadLine()?.ToLower();
            if (string.IsNullOrEmpty(str)) continue;
            string[] args = str.Split(' ');
            switch (args[0])
            {
                case "quit": 
                    Console.WriteLine("Shutting down server");
                    tcp.Dispose();
                    udp.Dispose();
                    return;
                case "clear":
                    if (args.Length != 2) break;
                    if (args[1] == "settings")
                    {
                        File.Delete(directory);
                    }

                    break;
                case "tickrate":
                    if (args.Length < 2) break;
                    if (int.TryParse(args[2], out int x))
                    {
                        if(args[1] == "tcp")
                            TCPMilliDelay = x;
                        else if (args[1] == "udp")
                            UDPMilliDelay = x;
                        else
                            break;
                        tcp.UpdateServerInfo();
                    } 
                    break;
                
                default:
                    Console.WriteLine("Unrecognized command: " + str);
                    break;
            }
        }
        
    }
    //Chat gpt generated
    public static void WriteIntToFile(string filePath, int value)
    {
        try
        {
            using StreamWriter writer = new StreamWriter(filePath);
            writer.Write(value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }

    public static bool ReadIntFromFile(string filePath, out int y)
    {
        y = 0; // Initialize y to a default value in case of failure
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Save file does not exist.");
                return false; // Return false if the file does not exist
            }

            using StreamReader reader = new StreamReader(filePath);
            y = int.Parse(reader.ReadLine());
            return true; // Return true to indicate success
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from file: {ex.Message}");
            return false; // Return false to indicate failure
        }
    }

   

}