using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using UnityEngine;

//Thank you chat GPT
[AttributeUsage(AttributeTargets.Method)]
public class ServerRPCAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class ClientRPCAttribute : Attribute { }

public static class StaticUtilities
{
    #region Netcode
    
   

    //public const int MilliDelay = 20;
    public static int MilliDelay = 10;
    public const int BufferSize = 1024;
    public const int MaxConnections = 8;

    private const int Port = 8888;
    private static readonly IPHostEntry HostInfo = Dns.GetHostEntry(Dns.GetHostName());
    public static readonly IPEndPoint ServerEndPoint;
    public static readonly IPEndPoint ClientAnyEndPoint  = new IPEndPoint(IPAddress.Any, 0) ;
    
    

    #endregion

    static StaticUtilities()
    {
        try
        {
            ServerEndPoint = new IPEndPoint(Dns.GetHostAddresses(HostInfo.HostName)[1], Port);
            Debug.Log($"Initializing IPEndpoint: {Dns.GetHostAddresses(HostInfo.HostName)[1]}:{Port}");
        }
        catch (Exception)
        {
            
            ServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
        }
    }


    #region Conversions

    #region Vector3

    public static byte[] ToBytes(this Vector3 vector3)
    {
        float[] floats = { vector3.x, vector3.y, vector3.z };
        byte[] bytes = new byte[floats.Length * sizeof(float)];
        Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static void ToBytes(this Vector3 vector3, [NotNull] ref byte[] bytes, int initialOffset)
    {
        if (initialOffset < 0 || initialOffset + sizeof(float) * 3 > bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(initialOffset),
                "Initial offset is out of range or insufficient space in byte array.");

        float[] floats = { vector3.x, vector3.y, vector3.z };
        Buffer.BlockCopy(floats, 0, bytes, initialOffset, floats.Length * sizeof(float));
    }
    
    public static Vector3 ToVector3(this byte[] bytes, int startIndex)
    {
        if (startIndex < 0 || startIndex + sizeof(float) * 3 > bytes.Length)
            throw new ArgumentException("Invalid start index or insufficient data in byte array.", nameof(startIndex));

        float[] floats = new float[3];
        Buffer.BlockCopy(bytes, startIndex * sizeof(float), floats, 0, floats.Length * sizeof(float));

       
        
        return new Vector3(floats[0], floats[1], floats[2]);
    }

    #endregion

    #region Quaternion

    public static byte[] ToBytes(this Quaternion quaternion)
    {
        float[] floats = { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        byte[] bytes = new byte[floats.Length * sizeof(float)];
        Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static void ToBytes(this Quaternion quaternion, [NotNull] ref byte[] bytes, int initialOffset)
    {
        if (initialOffset < 0 || initialOffset + sizeof(float) * 4 > bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(initialOffset),
                "Initial offset is out of range or insufficient space in byte array.");
        
        float[] floats = { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        Buffer.BlockCopy(floats, 0, bytes, initialOffset , floats.Length * sizeof(float));
    }

    public static Quaternion ToQuaternion(this byte[] bytes, int startIndex)
    {
        if (startIndex < 0 || startIndex + sizeof(float) * 4 > bytes.Length)
            throw new ArgumentException("Invalid start index or insufficient data in byte array.", nameof(startIndex));

        float[] floats = new float[4];
        Buffer.BlockCopy(bytes, startIndex * sizeof(float), floats, 0, floats.Length * sizeof(float));
        
        
        return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
    }

    #endregion

    #endregion
}