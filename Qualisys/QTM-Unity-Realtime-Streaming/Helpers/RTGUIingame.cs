// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using QTMRealTimeSDK;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{

    public class RTGUIingame : MonoBehaviour
    {
        short portUDP = -1;
        int selectedServer = 0;
        DiscoveryResponse? selectedDiscoveryResponse = null;
        List<DiscoveryResponse> discoveryResponses;

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            GUI.Box(new Rect(10, 10, 220, 155), "Qualisys Realtime Streamer");

            GUI.Label(new Rect(20, 40, 200, 40), "QTM Server:\n(switch with arrow keys)");

            if (discoveryResponses == null) discoveryResponses = RTClient.GetInstance().GetServers();
            var connectionState = RTClient.GetInstance().ConnectionState;
            List<GUIContent> serverSelection = new List<GUIContent>();
            foreach (var discoveryResponse in discoveryResponses)
            {
                serverSelection.Add(new GUIContent(discoveryResponse.HostName + " (" + discoveryResponse.IpAddress + ":" + discoveryResponse.Port + ") " + discoveryResponse.InfoText));
            }

            GUI.Label(new Rect(20, 75, 200, 40), serverSelection[selectedServer], style);

            if (Input.GetKeyDown(KeyCode.LeftArrow) && connectionState == RTConnectionState.Disconnected)
            {
                selectedServer--;
                if (selectedServer < 0)
                {
                    selectedServer += serverSelection.Count;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && connectionState == RTConnectionState.Disconnected)
            {
                selectedServer++;
                if (selectedServer > serverSelection.Count - 1)
                {
                    selectedServer = 0;
                }
            }
            selectedDiscoveryResponse = discoveryResponses[selectedServer];
            if (connectionState == RTConnectionState.Connected)
            {
                if (GUI.Button(new Rect(20, 115, 200, 40), "Disconnect"))
                {
                    Disconnect();
                }
            }
            else if (connectionState == RTConnectionState.Disconnected)
            {
                if (GUI.Button(new Rect(20, 115, 200, 40), "Connect"))
                {
                    Connect();
                }
            }
            GUI.Label(new Rect(20, 90, 200, 40), "Status: " + RTClient.GetInstance().ConnectionState);
        }

        void Disconnect()
        {
            RTClient.GetInstance().Disconnect();
        }

        void Connect()
        {
            if (selectedDiscoveryResponse.HasValue)
                RTClient.GetInstance().Connect(selectedDiscoveryResponse.Value, portUDP, true, true, false, true, false, true, true);
        }
    }
}