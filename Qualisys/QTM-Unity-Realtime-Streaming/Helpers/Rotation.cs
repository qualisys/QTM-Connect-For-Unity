// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Settings;
using System;
using UnityEngine;

namespace QualisysRealTime.Unity
{

    public static class Rotation
    {
        public static Quaternion GetCoordinateSystemRotation(Axis axisUp)
        {
            switch (axisUp)
            {
                case Axis.XAxisUpwards:
                    return Quaternion.Euler(0, 0, -90);
                case Axis.YAxisUpwards:
                    return Quaternion.Euler(0, 0, 0);
                case Axis.ZAxisUpwards:
                    return Quaternion.Euler(-90, 0, 0);
                case Axis.XAxisDownwards:
                    return Quaternion.Euler(0, 0, 90);
                case Axis.YAxisDownwards:
                    return Quaternion.Euler(0, 0, 180);
                case Axis.ZAxisDownwards:
                    return Quaternion.Euler(90, 0, 0);
                default:
                    throw new NotImplementedException(axisUp.ToString());
            }
        }
    }
}
