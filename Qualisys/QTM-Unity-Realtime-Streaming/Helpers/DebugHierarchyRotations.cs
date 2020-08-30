using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHierarchyRotations : MonoBehaviour
{
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        var transforms = new Stack<Transform>();
        transforms.Push(transform);
        while (transforms.Count > 0) 
        {
            var x = transforms.Pop();

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.ArrowHandleCap(-1, x.transform.position, x.rotation, 0.2f, EventType.Repaint);
            
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
