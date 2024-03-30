using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Netcode;
using TMPro;
using UnityEngine;

public class GameBeginner : MonoBehaviour
{
    private TextMeshProUGUI Text;
    private static GameBeginner gb;


    public static bool RaceIsStarted { get; private set; }
    public static bool RaceIsEnded { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        if (gb && gb != this)
        {
            Destroy(gameObject);
            return;
        }
        gb = this;
        Text = GetComponent<TextMeshProUGUI>();
    }

    public static void StartGame(double length)
    {
        gb.StartCoroutine(gb.BeginGameLoop(length));
    }

    public static void EndGame(string winnerName)
    {
        gb.StartCoroutine(gb.EndGameLoop(winnerName));
    }

    private IEnumerator EndGameLoop(string name)
    {
        float k = 0;
        Text.text = name + " WON!";
        Text.enabled = true;
        RaceIsEnded = true;
        Text.color = Color.green;
    
        while (true)
        {
            k += Time.deltaTime;
            
            Text.fontSize = Mathf.Lerp(32, 256, (Mathf.Sin(k) + 1)/2);
            yield return null;
        }
    }
    
    private IEnumerator BeginGameLoop(double length)
    {
        Color c = Text.color;
        Text.enabled=(true);
        while (length > 0)
        {
            length -= Time.deltaTime;
            float sPerc = (float)(length % 1);
            Text.text = ((int)(length+0.99)).ToString();
            c.a = sPerc;
            Text.color = c;
            Text.fontSize = Mathf.Lerp(32, 256, sPerc);
            yield return null;
        }
        Text.enabled=(false);
        RaceIsStarted = true;
    }
}
