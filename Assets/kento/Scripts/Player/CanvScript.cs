using UnityEngine;

public class CanvScript : MonoBehaviour
{
    void OnEnable() => UICameraFollower.uiList.Add(transform);
    void OnDisable() => UICameraFollower.uiList.Remove(transform);
}
