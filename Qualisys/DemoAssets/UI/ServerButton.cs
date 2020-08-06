using UnityEngine;
using UnityEngine.UI;
using QualisysRealTime.Unity;
using QTMRealTimeSDK;
using System.Collections;

[System.Serializable]
public class ServerButton : MonoBehaviour
{
    public Button button;
    public Text HostText;
    public Text IpAddressText;
    public Text InfoText;
    public DiscoveryResponse response;
    void Start()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(OnClick));
    }
    void OnClick()
    {
        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        RTClient.GetInstance().StartConnecting(response.IpAddress, -1, true, true, false, true, false, true);
        InfoText.color = Color.yellow;
        InfoText.text = "Connecting ...";
        
        while (RTClient.GetInstance().ConnectionState == RTConnectionState.Connecting) 
        {
            yield return null;
        }
        
        if (RTClient.GetInstance().ConnectionState == RTConnectionState.Connected)
        {
            SendMessageUpwards("Disable");
        }
        else
        {
            InfoText.color = Color.red;
            InfoText.text = "Could not connect to this server";
        }
    }
}
