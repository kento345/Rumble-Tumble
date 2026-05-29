using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(DecalProjector))]
public class DecalFitToParent : MonoBehaviour
{
    void OnEnable()
    {
        FitToParent();
    }

    void FitToParent()
    {
        var decal = GetComponent<DecalProjector>();
        var parent = transform.parent;

        if (parent == null) return;

        var renderer = parent.GetComponent<Renderer>();
        if (renderer == null) return;

        // 親の実サイズ（スケール込み）
        Vector3 size = renderer.bounds.size;

        decal.size = size;
    }
}
