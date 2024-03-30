using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SpawnManager : MonoBehaviour
{
    private static Transform[] _availablePoints;

    //How do we know which player number they are?
    //What happens if a player leaves?
    //The server needs to tell us what number player we are, but we can know from the network manager our index.
    //If the network manager already knows our index, then we can't use a stack.
    

    // Start is called before the first frame update
    void Awake()
    {
        _availablePoints = GetComponentsInChildren<Transform>();
        print($"There are {_availablePoints.Length}  spawn points: ");
    }

    public static Transform GetSpawnPoint(int id) => _availablePoints[id+1];

    
}
