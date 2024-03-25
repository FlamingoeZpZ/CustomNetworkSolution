using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SpawnManager : MonoBehaviour
{
    private static Transform[] _availablePoints;

    private static List<int> _spawnPoints;
    //How do we know which player number they are?
    //What happens if a player leaves?
    //The server needs to tell us what number player we are, but we can know from the network manager our index.
    //If the network manager already knows our index, then we can't use a stack.
    

    // Start is called before the first frame update
    void Awake()
    {
        _availablePoints = GetComponentsInChildren<Transform>();
        _spawnPoints = new();
        for (int i = _availablePoints.Length-1; i >= 0; --i) {
            _spawnPoints.Add(i);
        }
    }
    
    public static Transform GetNextSpawnPoint()
    {
        int k = _spawnPoints.Count - 1;
        if (k == -1)
        {
            Debug.LogError("There are no spawn points remaining");
            return _availablePoints[0];
        }
        int n = _spawnPoints[k];
        _spawnPoints.RemoveAt(0);
        return _availablePoints[n];
    }

    public static void AddBackSpawnPoint(int num)
    {
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
            if (num > _spawnPoints[i])
            {
                _spawnPoints.Insert(i+1, num);
            }
        }
    }
}
