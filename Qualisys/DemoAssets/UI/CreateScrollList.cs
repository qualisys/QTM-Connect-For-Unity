using UnityEngine;
using System.Collections.Generic;
using QualisysRealTime.Unity;
using QTMRealTimeSDK;

public class CreateScrollList : MonoBehaviour {

    public GameObject serverButtonPrefab; 
    public Transform contentPanel;
    private List<DiscoveryResponse> discoveryResponses;

    void Update()
    {
        if (RTClient.GetInstance().GetStreamingStatus())
        {
            Disable();
        }

        if (contentPanel.childCount < 1)
        {
            UpdateList();
        }
    }
    public void UpdateList()
    {
        for (int i = 0; i < contentPanel.childCount; i++)
        {
            Destroy(contentPanel.GetChild(i).gameObject);
        }
        discoveryResponses = RTClient.GetInstance().GetServers();
        foreach (DiscoveryResponse server in discoveryResponses)
        {
            GameObject newServer = Instantiate(serverButtonPrefab) as GameObject;
            ServerButton serverButton = newServer.GetComponent<ServerButton>();
            serverButton.HostText.text = server.HostName;
            serverButton.IpAddressText.text = server.IpAddress + ":" + server.Port;
            serverButton.InfoText.text = server.InfoText;
            serverButton.response = server;
            newServer.transform.SetParent(contentPanel);
        }
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
