using UnityEngine;
using Unity.Cinemachine;

public class SplineCartRotationControl : MonoBehaviour
{
    private CinemachineSplineCart cart;
    private Quaternion fixedRotation;

    void Start()
    {
        cart = GetComponent<CinemachineSplineCart>();
        fixedRotation = transform.rotation; // 初期回転を保存
    }

    void LateUpdate()
    {
        // 回転を固定（位置のみ更新）
        transform.rotation = fixedRotation;
    }
}