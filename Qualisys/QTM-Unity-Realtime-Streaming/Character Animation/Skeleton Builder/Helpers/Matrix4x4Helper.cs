// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//

using UnityEngine;

namespace QualisysRealTime.Unity
{
    public static class Matrix4x4Helper
    {
        /// <summary>
        /// Create matrix from position and three vectors
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="forward">forward vector</param>
        /// <param name="up">up vector</param>
        /// <param name="right">right vector</param>
        /// <returns>matrix with cooridnate system based on vectors</returns>
        public static Matrix4x4 GetMatrix(Vector3 position, Vector3 front, Vector3 up, Vector3 right)
        {
            var mat = Matrix4x4.identity;

            mat.SetTRS(position, Quaternion.identity, Vector3.one);

            var right4v = new Vector4(right.x, right.y, right.z, 0);
            var up4v = new Vector4(up.x, up.y, up.z, 0);
            var front4v = new Vector4(front.x, front.y, front.z, 0);

            mat.SetColumn(0, right4v);
            mat.SetColumn(1, up4v);
            mat.SetColumn(2, front4v);

            return mat;
        }
    }
}
