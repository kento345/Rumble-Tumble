using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;

public class SplineCartFollowPath : MonoBehaviour
{
    private CinemachineSplineCart cart;
    public SplineContainer splineContainer;

    void Start()
    {
        cart = GetComponent<CinemachineSplineCart>();
    }

    void LateUpdate()
    {
        if (splineContainer != null && cart != null)
        {
            // Splineの位置を取得
            float t = cart.SplinePosition;

            // Splineから接線（進行方向）を取得
            Vector3 forward = splineContainer.EvaluateTangent(t);

            // 進行方向を向く
            if (forward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }
    }
}