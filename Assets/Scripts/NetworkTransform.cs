using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lecture;
using UnityEngine;

public class NetworkTransform : MonoBehaviour
{

    [field:SerializeField]public bool IsOwner { get; set; }

    [Header("Replication Rules")]
    [SerializeField] private bool position = true;
    [SerializeField] private bool rotation = true;
    [SerializeField] private bool scale = false;
    //If we do not own the object, then we should be replicating it's position and sending it...
    //First of all, how do we determine who owns what object....
    //When a client connects to the game, we need to spawn an object.


    private Func<Task> _response;
    
   // private ArraySegment<byte> _newUploadBuffer;
    private byte[] _uploadBuffer;
    private ArraySegment<byte> _buffer;

    private byte[] _previous;
    
    //Build buffer. Has to be start to work when button is pressed, else not mono
    private void Start()
    {
        Debug.Log("Object loaded: " + IsOwner);
        int num = 0;
        if (position) num += 3; 
        if (rotation) num += 4; 
        if (scale) num += 3; 
        _uploadBuffer = new byte[num * sizeof(float) + sizeof(int)];
       
       
        NetUpdate();

        if (!IsOwner)
        {
            _response = ClientUpdate;
            if (rotation && TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            _buffer = _uploadBuffer;
        }
        else
        {
            _previous = new byte[num * sizeof(float) + sizeof(int)];
             BitConverter.GetBytes(GetHashCode()).CopyTo(_uploadBuffer,0);
            _response = OwnerUpdate;
        }
        
    }

    
    async void NetUpdate()
    {
        while (enabled)
        {
            //BUG:? The delay for the object sending should be TWICE than the object recieving?
            await Task.Delay(StaticUtilities.MilliDelay * (IsOwner?2:1));
            await _response();
        }
    }
    
    private async Task OwnerUpdate()
    {
        //If we own this object, let's just blindly trust the player, and send all the info...
        int num = 0;
        if (position)
        {
            transform.position.ToBytes(ref _uploadBuffer, sizeof(int));
            num += 3;
        }
        if (rotation)
        {
            transform.rotation.ToBytes(ref _uploadBuffer, sizeof(int) + sizeof(float) * num);
            num += 4; 
        } 
        if (scale) transform.localScale.ToBytes(ref _uploadBuffer, sizeof(int) + sizeof(float) * num);

        
        //Check if we've been a silly boy
        if (_previous.SequenceEqual(_uploadBuffer))
        {
            return;
        }

        //We need to create a deep copy, not a shallow copy
        Array.Copy(_uploadBuffer, _previous, _uploadBuffer.Length);
        
        //Upload data.
        _buffer = new ArraySegment<byte>(_uploadBuffer);
         //var x = await Client.client.SendToAsync(_buffer, SocketFlags.None, StaticUtilities.ServerEndPoint);
         var x = await UDPClient.SendToAsync(_buffer, SocketFlags.None, StaticUtilities.ServerEndPoint);
    }
    
    private async Task ClientUpdate()
    {
        //Retrieve data.
        var x =await UDPServer.ReceiveFromAsync(_buffer, SocketFlags.None, StaticUtilities.ClientAnyEndPoint);
        int id = BitConverter.ToInt32(_buffer.Array, 0);
        if (id != GetHashCode()) return;
        int num = 1;
        
        if (position)
        {
            transform.position = _buffer.Array.ToVector3(num);
            num += 3;
        }
        if (rotation)
        {
            transform.rotation =  _buffer.Array.ToQuaternion(num);
            num += 4; 
        } 
        if (scale) transform.localScale =  _buffer.Array.ToVector3(num);
        
       
        
        
    }

}
