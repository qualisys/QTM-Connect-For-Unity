using UnityEngine;
using UnityEngine.UI;
using QTMRealTimeSDK.Data;
using QualisysRealTime.Unity;
using QTMRealTimeSDK;

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
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(Connect));
    }
    void Connect()
    {
        if (!RTClient.GetInstance().Connect(response, response.Port, true, true, false, true, false, true))
        {
            InfoText.color = Color.red;
            InfoText.text = "Could not connect to this server";
        }
        else
        {
            SendMessageUpwards("Disable");
        }
    }
}
