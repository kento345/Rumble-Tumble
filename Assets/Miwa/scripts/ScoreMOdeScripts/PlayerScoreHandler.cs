using System.Collections;
using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    private PlayerHealth _health;
    [SerializeField] private GameObject _itemPrefab; // インスペクターからアイテムを設定できるように念のため復元
    private int lastAttackerIndex = -1;
    private float lastHitTime;

    private void Awake()
    {
        // 【修正ポイント】確実に自分自身の PlayerHealth コンポーネントを取得する
        _health = GetComponent<PlayerHealth>();

        if (_health == null)
        {
        }
    }

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
        // 【安全対策】万が一 _health が正しく取得できていなかった場合のガード
        if (_health == null) return;

        // 自分で自分を落としたことにならないようチェック
        if (index == _health.playerIndex) return;

        lastAttackerIndex = index;
        lastHitTime = Time.time;
    }

    public void HandleDeath()
    {
        if (_health == null) return;

        int myIndex = _health.playerIndex;
        int currentScore = GameManager_M.currentScores[myIndex];

        // 1. 相手への加点（落とした人に+3点）
        if (lastAttackerIndex != -1)
        {
            GameManager_M.Instance.AddScore(lastAttackerIndex, 3);
        }

        // 2. 自分の減点ペナルティ
        if (currentScore > 0)
        {
            int penalty = Mathf.Max(1, currentScore / 2);
            GameManager_M.Instance.AddScore(myIndex, -penalty);
        }

        lastAttackerIndex = -1;
    }
}