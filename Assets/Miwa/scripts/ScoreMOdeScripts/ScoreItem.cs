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
            rb.isKinematic = false;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isCollected || timer < collectDelay) return;

        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                isCollected = true;
                // ★古いScoreManagerではなく、GameManager_Mに統合されたスコアシステムへ加点！
                GameManager_M.Instance.AddScore(health.playerIndex, 1);
                Destroy(gameObject);
            }
        }
    }
}