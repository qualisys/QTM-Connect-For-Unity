// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Settings;
using UnityEngine;

namespace QualisysRealTime.Unity
{

    public static class Rotation
    {
        public static Quaternion GetCoordinateSystemRotation(Axis axisUp)
        {
            Quaternion retval = Quaternion.identity;
            switch (axisUp)
            {
                case Axis.XAxisUpwards:
                    retval = Quaternion.Euler(0, 0, -90);
                    break;
                case Axis.YAxisUpwards:
                    retval = Quaternion.Euler(0, 0, 0);
                    break;
                case Axis.ZAxisUpwards:
                    retval = Quaternion.Euler(-90, 0, 0);
                    break;
                case Axis.XAxisDownwards:
                    retval = Quaternion.Euler(0, 0, 90);
                    break;
                case Axis.YAxisDownwards:
                    retval = Quaternion.Euler(0, 0, 180);
                    break;
                case Axis.ZAxisDownwards:
                    retval = Quaternion.Euler(90, 0, 0);
                    break;
                default:
                    retval = Quaternion.Euler(0, 0, 0);
                    break;
            }
            return retval;
        }
    }
}
