// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using QTMRealTimeSDK;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    class RTAnalog : MonoBehaviour
    {
        public string ChannelName = "Put QTM Analog channel name here";

        protected RTClient rtClient;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();

            var channel = rtClient.GetAnalogChannel(ChannelName);
            if (channel != null)
            {
                foreach (var value in channel.Values)
                {
                    // TODO::: What do we do with the analog values for this channel...?
                }
            }
        }
    }
}