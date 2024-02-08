using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Lecture
{
    public  static class UDPServer
    {
        private static readonly Socket _server;
        
        static readonly ArraySegment<byte> _receiveBuffer = new ArraySegment<byte>();
        static UDPServer()
        {

            try
            {
                _server = new Socket(StaticUtilities.ServerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _server.Bind(StaticUtilities.ServerEndPoint);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        
        
    
    
        public static async Task<SocketReceiveFromResult> ReceiveFromAsync(ArraySegment<byte> bytes, SocketFlags flags,  IPEndPoint endPoint)
        {
            return await _server.ReceiveFromAsync(bytes, flags, endPoint);
        }
    }

    public class TCPServer
    {

        private readonly Socket _server;
        private readonly List<Socket> _clients = new();
        private bool _isRunning = true;
        

        public TCPServer()
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveBufferSize = StaticUtilities.BufferSize,
                SendBufferSize = StaticUtilities.BufferSize
            };
            try
            {
                _server.Bind(StaticUtilities.ServerEndPoint);
                _server.Listen(StaticUtilities.MaxConnections);
            }
            catch (Exception e)
            {
                Debug.Log("Failed during server initializing: " + e);
                return;
            }
            StartServer();
        }

        private async void StartServer()
        {
            try
            {
                Socket user = await _server.AcceptAsync();
                _clients.Add(user);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
            StartServer(); // Repeat for all eternity...
        }

      


        //Try to make sure everything is getting cleaned up!
        private void CloseServer()
        {
            _isRunning = false;
            _server.Shutdown(SocketShutdown.Both);
            _server.Close();
        }
    }
}
