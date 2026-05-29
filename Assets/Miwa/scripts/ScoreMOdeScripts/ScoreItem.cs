using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    private bool isCollected = false;
    private Rigidbody rb;
    private float collectDelay = 0.5f;
    private float timer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 地面を突き抜けないように連続衝突検知を有効にする
        if (rb != null) rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    public void Launch(Vector3 direction, float force)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // 物理を有効にする
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    // ★重要：地面を突き抜けないように Trigger ではなく Collision で「着地」させる
    // 拾う判定は OnTrigger にする

    private void OnTriggerStay(Collider other)
    {
        if (isCollected || timer < collectDelay) return;

        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                isCollected = true;
                GameManager_M.Instance.AddScore(health.playerIndex, 1);
                // 消える前にエフェクト等あればここで
                Destroy(gameObject);
            }
        }
    }
}