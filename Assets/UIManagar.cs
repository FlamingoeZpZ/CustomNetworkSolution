using System;
using System.Collections;
using System.Text;
using Netcode;
using TMPro;
using UnityEngine;

public class UIManagar : MonoBehaviour
{
    [SerializeField] private TMP_InputField name;
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private TextMeshProUGUI failedObject;
    [SerializeField] private GameObject panelA;
    [SerializeField] private GameObject panelB;

    private static UIManagar derefl;

    private void Awake()
    {
        if (derefl && derefl != this)
        {
            Destroy(gameObject);
            return;
        }

        derefl = this;
    }

    public async void TryConnect()
    {
        print("Trying to Connect");
        if (string.IsNullOrEmpty(name.text))
        {
            StartCoroutine(ScrewedUp("Invalid Name"));
            return;
        }
        bool success = await NetworkManager.Instance.Initialize(name.text, ip.text);

        if (!success)
        {
            StartCoroutine(ScrewedUp("Server not responding"));
            return;
        }

        panelA.SetActive(false);
        panelB.SetActive(true);
    }

    public void Quit()
    {
        panelA.SetActive(true);
        panelB.SetActive(false); 
        NetworkManager.Instance.Quit();
    }

    public static void Disconnect(ref byte[] msgContent)
    {
        derefl.StartCoroutine(derefl.ScrewedUp(Encoding.UTF8.GetString(msgContent)));
    }

    private IEnumerator ScrewedUp(string str)
    {
        float d = 10;
        Color c = failedObject.color;
        failedObject.text = "Connection failed: " + str;
        failedObject.gameObject.SetActive(true);
        panelA.SetActive(true);
        panelB.SetActive(false);
        while (d > 0)
        {
            d -= Time.deltaTime;
            c.a = Mathf.Lerp(0,1, d/10);
            failedObject.color = c;
            yield return null;
        }
        failedObject.gameObject.SetActive(false);

    }
}

