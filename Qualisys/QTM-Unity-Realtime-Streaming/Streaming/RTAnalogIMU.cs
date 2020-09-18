// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    class RTAnalogIMU : MonoBehaviour
    {
        public string ChannelX = "Put IMU channel X name here";
        public string ChannelY = "Put IMU channel Y name here";
        public string ChannelZ = "Put IMU channel Z name here";
        public string ChannelW = "Put IMU channel W name here";

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

            var channels = rtClient.GetAnalogChannels(new List<string>() { ChannelX, ChannelY, ChannelZ, ChannelW });
            if (channels != null)
            {
                for (int i = 0; i < channels[0].Values.Length; i++)
                {
                    transform.rotation = new Quaternion(channels[0].Values[i], channels[1].Values[i], channels[2].Values[i], channels[3].Values[i]);
                }
            }
        }
    }
}