// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Settings;
using UnityEngine;

namespace QualisysRealTime.Unity
{

    public static class Rotation
    {
        public enum ECoordinateAxes
        {
            AxisXpositive = 1,
            AxisXnegative = -1,
            AxisYpositive = 2,
            AxisYnegative = -2,
            AxisZpositive = 3,
            AxisZnegative = -3
        };

        public static void GetCalibrationAxesOrder(Axis axisUp, out ECoordinateAxes reNewX, out ECoordinateAxes reNewY, out ECoordinateAxes reNewZ)
        {
            switch (axisUp)
            {
                case Axis.XAxisUpwards:
                    reNewX = ECoordinateAxes.AxisZpositive;
                    reNewY = ECoordinateAxes.AxisXpositive;
                    reNewZ = ECoordinateAxes.AxisYpositive;
                    break;
                case Axis.YAxisUpwards:
                    reNewX = ECoordinateAxes.AxisXpositive;
                    reNewY = ECoordinateAxes.AxisYpositive;
                    reNewZ = ECoordinateAxes.AxisZpositive;
                    break;
                case Axis.ZAxisUpwards:
                    reNewX = ECoordinateAxes.AxisYpositive;
                    reNewY = ECoordinateAxes.AxisZpositive;
                    reNewZ = ECoordinateAxes.AxisXpositive;
                    break;
                case Axis.XAxisDownwards:
                    reNewX = ECoordinateAxes.AxisZnegative;
                    reNewY = ECoordinateAxes.AxisXnegative;
                    reNewZ = ECoordinateAxes.AxisYnegative;
                    break;
                case Axis.YAxisDownwards:
                    reNewX = ECoordinateAxes.AxisXnegative;
                    reNewY = ECoordinateAxes.AxisYnegative;
                    reNewZ = ECoordinateAxes.AxisZnegative;
                    break;
                case Axis.ZAxisDownwards:
                    reNewX = ECoordinateAxes.AxisYnegative;
                    reNewY = ECoordinateAxes.AxisZnegative;
                    reNewZ = ECoordinateAxes.AxisXnegative;
                    break;
                default:
                    reNewX = ECoordinateAxes.AxisXpositive;
                    reNewY = ECoordinateAxes.AxisYpositive;
                    reNewZ = ECoordinateAxes.AxisZpositive;
                    break;
            }
        }


        public static Quaternion GetAxesOrderRotation(ECoordinateAxes eNewX, ECoordinateAxes eNewY, ECoordinateAxes eNewZ)
        {
            Quaternion oRotation = Quaternion.identity;

            switch (eNewY)
            {
                case ECoordinateAxes.AxisXpositive:
                    {
                        oRotation *= QuaternionHelper.RotationZ(90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisYpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationX(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationX(90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationX(-90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisXnegative:
                    {
                        oRotation *= QuaternionHelper.RotationZ(-90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisYnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationX(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationX(-90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationX(90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisYpositive:
                    {
                        // Retain Y axis.
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationY(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationY(90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationY(-90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisYnegative:
                    {
                        oRotation *= QuaternionHelper.RotationX(180 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationY(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationY(90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisZnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationY(-90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisZpositive:
                    {
                        oRotation *= QuaternionHelper.RotationX(-90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationZ(-90 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationZ(90 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
                case ECoordinateAxes.AxisZnegative:
                    {
                        oRotation *= QuaternionHelper.RotationX(90 * Mathf.PI / 180);
                        switch (eNewX)
                        {
                            case ECoordinateAxes.AxisXnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYpositive:
                                {
                                    oRotation *= QuaternionHelper.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                            case ECoordinateAxes.AxisYnegative:
                                {
                                    oRotation *= QuaternionHelper.RotationZ(180 * Mathf.PI / 180);
                                    break;
                                }
                        }
                        break;
                    }
            }

            //oRotation.normalize();
            return oRotation;
        }
    }
}
