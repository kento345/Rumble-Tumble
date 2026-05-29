using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class UICameraFollower : MonoBehaviour
{
    public static readonly List<Transform> uiList = new();

    void LateUpdate()
    {
        if(Camera.main == null) { return; }
        
        Quaternion camRot = Camera.main.transform.rotation;

        for (int i = 0; i < uiList.Count; i++)
        {
            uiList[i].rotation = camRot;
        }
    }
}
