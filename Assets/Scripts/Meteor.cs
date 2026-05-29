using UnityEngine;

public class Meteor : MonoBehaviour
{
    public GameObject Smoke;

    [Header("消滅設定")]
    [SerializeField] private float waitTime = 2f; // 停止する時間
    [SerializeField] private float shrinkDuration = 1f; // 縮小にかかる時間

    [Header("エフェクト削除設定")]
    [SerializeField] private ParticleSystem[] particlesToRemove; // 削除するパーティクルシステム

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stage"))
        {
            // 接触したオブジェクトを消去
            Destroy(collision.gameObject);

            // 煙エフェクトを生成
            Vector3 contactPoint = collision.contacts[0].point;
            if (Smoke != null)
            {
                Instantiate(Smoke, contactPoint, Quaternion.identity);
            }

            // 移動コンポーネントを無効化
            MoveObject moveComponent = GetComponent<MoveObject>();
            if (moveComponent != null)
            {
                moveComponent.enabled = false;
            }

            // 物理演算を停止
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;

                // 指定されたパーティクルシステムを削除
                if (particlesToRemove != null && particlesToRemove.Length > 0)
                {
                    foreach (ParticleSystem ps in particlesToRemove)
                    {
                        if (ps != null)
                        {
                            Destroy(ps.gameObject);
                        }
                    }
                }
            }

            // 縮小して消滅するコルーチンを開始
            StartCoroutine(ShrinkAndDestroy());
        }
    }

    private System.Collections.IEnumerator ShrinkAndDestroy()
    {
        // 指定時間待機
        yield return new WaitForSeconds(waitTime);

        // 元のスケールを保存
        Vector3 originalScale = transform.localScale;
        float elapsedTime = 0f;

        // 徐々に縮小
        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        // 完全に消去
        Destroy(gameObject);
    }
}