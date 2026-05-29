using UnityEngine;
using System.Collections.Generic;

public class BouncyWall : MonoBehaviour
{
    [Header("Ball Settings")]
    [Tooltip("ボールの加速倍率")]
    [SerializeField] private float ballForceMultiplier = 1.5f;

    [Header("Player Settings")]
    [Tooltip("プレイヤーを跳ね返す力")]
    [SerializeField] private float playerBounceForce = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // ボールの速度を記録する辞書
    private Dictionary<Rigidbody, Vector3> ballVelocities = new Dictionary<Rigidbody, Vector3>();

    private void FixedUpdate()
    {
        // 全てのBallタグオブジェクトの速度を記録
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject ball in balls)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                ballVelocities[rb] = rb.linearVelocity;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ballタグのオブジェクトが当たった場合
        if (collision.gameObject.CompareTag("Ball"))
        {
            HandleBallCollision(collision);
        }
        // Playerタグのオブジェクトが当たった場合
        else if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
    }

    private void HandleBallCollision(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        // 衝突点の法線を取得
        Vector3 normal = collision.contacts[0].normal;

        // 衝突直前の速度を取得
        Vector3 incomingVelocity = Vector3.zero;
        if (ballVelocities.ContainsKey(rb))
        {
            incomingVelocity = ballVelocities[rb];
        }
        else
        {
            // フォールバック: 相対速度を使用
            incomingVelocity = -collision.relativeVelocity;
        }

        // 入射速度の大きさ
        float speed = incomingVelocity.magnitude;

        if (showDebugLogs)
        {
            Debug.Log($"衝突前速度: {speed}, 法線: {normal}");
        }

        // 速度がほぼ0の場合は処理しない
        if (speed < 0.01f)
        {
            if (showDebugLogs)
            {
                Debug.Log("速度が0に近いため反射をスキップ");
            }
            return;
        }

        // 反射方向を計算
        Vector3 reflectDirection = Vector3.Reflect(incomingVelocity.normalized, normal);

        // 反射速度を計算（倍率を適用）
        Vector3 reflectVelocity = reflectDirection * speed * ballForceMultiplier;

        // 速度を直接設定
        rb.linearVelocity = reflectVelocity;

        if (showDebugLogs)
        {
            Debug.Log($"反射後速度: {reflectVelocity.magnitude}, 方向: {reflectDirection}");
        }
    }

    private void HandlePlayerCollision(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        // 衝突点の法線を取得
        Vector3 normal = collision.contacts[0].normal;

        // 法線方向に力を加える
        rb.AddForce(normal * playerBounceForce, ForceMode.Impulse);

        if (showDebugLogs)
        {
            Debug.Log($"プレイヤー跳ね返し - 力: {playerBounceForce}, 方向: {normal}");
        }
    }
}