using UnityEngine;

public class ShowHideMobileUI : MonoBehaviour
{
    [SerializeField] GameObject MobileUI;
    [SerializeField] GameObject PcConcoleUI;
    
    void Start()
    {
        #if UNITY_STANDALONE
        bool state = false;
        #else
        bool state = true;
        #endif
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        MobileUI.SetActive (state);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        PcConcoleUI.SetActive (!state);
    }
}
