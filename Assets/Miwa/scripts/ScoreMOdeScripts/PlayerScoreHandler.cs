using System.Collections;
using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    private PlayerHealth _health;
    [SerializeField] private GameObject _itemPrefab;
    private int lastAttackerIndex = -1;
    private float lastHitTime;

    private void Awake() => _health = GetComponent<PlayerHealth>();

    // ぶつかっている間、常に「最後に触った人」を更新し続ける
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var attackerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (attackerHealth != null)
            {
                SetLastAttacker(attackerHealth.playerIndex);
            }
        }
    }

    public void SetLastAttacker(int index)
    {
        // 自分で自分を落としたことにならないようチェック
        if (index == _health.playerIndex) return;

        lastAttackerIndex = index;
        lastHitTime = Time.time;
    }

    public void HandleDeath()
    {
        int myIndex = _health.playerIndex;
        int currentScore = GameManager_M.currentScores[myIndex];

        // 1. 相手への加点
        if (lastAttackerIndex != -1)
        {
            GameManager_M.Instance.AddScore(lastAttackerIndex, 3);
        }

        // 2. 自分の減点とアイテム放出
        if (currentScore > 0)
        {
            int penalty = Mathf.Max(1, currentScore / 2);
            GameManager_M.Instance.AddScore(myIndex, -penalty);
        }

        lastAttackerIndex = -1;
    }

    public void DropItemsAtRespawn(int count)
    {
        StartCoroutine(DropItemsWithDelay(count));
    }

    private IEnumerator DropItemsWithDelay(int count)
    {
        yield return new WaitForSeconds(0.2f); // 少し待ってから放出
        for (int i = 0; i < count; i++)
        {
            if (_itemPrefab == null) break;

            // 少しバラけるようにランダムな位置から生成
            Vector3 spawnPos = transform.position + Vector3.up * 1.0f + Random.insideUnitSphere * 0.5f;
            GameObject itemObj = Instantiate(_itemPrefab, spawnPos, Quaternion.identity);

            ScoreItem itemScript = itemObj.GetComponent<ScoreItem>();
            if (itemScript != null)
            {
                Vector3 launchDir = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)).normalized;
                itemScript.Launch(launchDir, 4f);
            }
        }
    }
}