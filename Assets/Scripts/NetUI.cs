using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        NetworkManager.OnConnectionSuccess += () =>
        {
            Debug.Log("Connection Success");
            Destroy(gameObject);
        };
        NetworkManager.OnConnectionFail += () =>
        {
            Debug.Log("Connection Failed");
        };
    }
}
