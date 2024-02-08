using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lecture;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class NetworkManager : MonoBehaviour
{
    
    [SerializeField] private NetworkTransform playerPrefab;
    [SerializeField] private Client clientPrefab;

    //TODO TCP and UDP servers that handle messages.
    public TCPServer server;
    //public TCPClient client;
    
    public static Action OnConnectionSuccess;
    public static Action OnConnectionFail;
    public static Action OnPlayerJoined;
        
    public static NetworkManager Instance { get; private set; }

    
    //NEEDS to happy first ALWAYS
    
    void OnEnable()
    {
        Debug.Log("NetworkManager Starting");
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("NetworkManager Ready");
    }

    [ServerRPC] // this doesn't do anything rn... just looks cool
    public async void SpawnObject()
    {
        //I need to do more research...
    }
    
    
    private bool _isConnecting;
    
    public void StartAsServer()
    {
        if (_isConnecting) return;
        _isConnecting = true;
        Debug.Log("Starting as Server");
        // Implement your server logic here
    }
    
    public void StartAsHost()
    {
        if (_isConnecting) return;
        _isConnecting = true;
        Debug.Log("Starting as Host");
        // Implement your host logic here
        server = new TCPServer();

    }

    public void StartAsClient()
    {
        if (_isConnecting) return;
        _isConnecting = true;
        
        // Implement your client logic here
        NetworkTransform tr = Instantiate(playerPrefab);
        tr.IsOwner = true;
        Debug.Log("Starting as Client");
    }
    
}
