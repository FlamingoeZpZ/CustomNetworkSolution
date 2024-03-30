using Netcode;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NetworkObject x) && x.IsOwner) // Only the locally controlled player will have this.
        {
            
            CheckPointManager.SetNextCheckPoint(x.transform);
            gameObject.SetActive(false);
        }
    }
}
