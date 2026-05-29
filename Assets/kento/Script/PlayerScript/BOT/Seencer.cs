#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;

public class Seencer : MonoBehaviour
{
    private int layer;
    bool isGround = false;
    int groundCount = 0;
    private void Start()
    {
        layer = LayerMask.NameToLayer("Ground");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == layer)
        {
            groundCount++;
            isGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == layer)
        {
            groundCount--;
            if(groundCount <= 0)
            {
                groundCount = 0;
                isGround = false;
            }
        }
    }

    public bool CheckLayer()
    {
        return isGround;
    }
}
