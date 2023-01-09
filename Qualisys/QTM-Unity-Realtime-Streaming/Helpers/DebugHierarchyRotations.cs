using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace QualisysRealTime.Unity
{
    public class DebugHierarchyRotations : MonoBehaviour
    {
        public Color color = Color.red;

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            var transforms = new Stack<Transform>();
            transforms.Push(transform);
            while (transforms.Count > 0)
            {
                var x = transforms.Pop();
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.ArrowHandleCap(-1, x.transform.position, x.rotation, 0.04f, EventType.Repaint);

                foreach (Transform child in x)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        UnityEditor.Handles.DrawLine(x.position, child.position);
                        transforms.Push(child);
                    }
                }

            }
        }
    }
}
