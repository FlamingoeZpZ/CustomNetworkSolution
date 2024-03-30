using Netcode;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    private static CheckPoint[] _cps;

    private static int currentIdx;

    private static Vector3 sp;
    private static Quaternion sr;

    public static void GetCurrentCheckPoint(out Vector3 pos, out Quaternion rot)
    {
        pos = sp;
        rot = sr;
    }
    
    public static void SetNextCheckPoint(Transform tr){
        sp = tr.position + Vector3.up * 2;
        sr = tr.rotation;
        if (++currentIdx >= _cps.Length)
        {
            //Send end game message.
            NetworkManager.Instance.EndGameClient();
            return;
        }
        
        _cps[currentIdx].gameObject.SetActive(true);
    }


    // Start is called before the first frame update
    void Start()
    {
        _cps = GetComponentsInChildren<CheckPoint>();
        foreach (var l in _cps)
        {
            l.gameObject.SetActive(false);
        }
        _cps[0].gameObject.SetActive(true);
        currentIdx = 0;
        sp = transform.position - Vector3.up;
        sr = transform.rotation;
    }
    
    
    
}
