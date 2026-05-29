using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ■ ConveyorCartManager
/// 
/// 複数のカート＋ルートのセットをリストで管理し、
/// ランダムで一つだけをアクティブにして指定秒数後に非アクティブに戻す。
/// 
/// セットアップ
///   ・カートとルートをまとめた親オブジェクトを CartSets リストに登録
///   ・または Cart と Spline を個別に CartEntries に登録
/// </summary>
public class ConveyorCartManager : MonoBehaviour
{
    [System.Serializable]
    public class CartEntry
    {
        public string label;           // 管理用ラベル（任意）
        public GameObject cart;        // CinemachineSplineCart の GameObject
        public GameObject spline;      // SplineContainer の GameObject
    }

    [Header("カート＋ルートのセット一覧")]
    public List<CartEntry> cartEntries = new List<CartEntry>();

    [Header("タイミング設定")]
    [Tooltip("アクティブになっている秒数")]
    public float activeDuration = 3.0f;

    [Tooltip("非アクティブ状態が続く秒数（次の抽選までのインターバル）")]
    public float intervalDuration = 2.0f;

    [Tooltip("ゲーム開始から最初の抽選までの待機秒数")]
    public float initialDelay = 1.0f;

    // 現在アクティブなエントリのインデックス（-1 = なし）
    private int _activeIndex = -1;

    // ─────────────────────────────────────────
    void Start()
    {
        // 全セットを非アクティブにしてからサイクル開始
        DeactivateAll();
        StartCoroutine(CyclRoutine());
    }

    // ─────────────────────────────────────────
    IEnumerator CyclRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // ── ランダムで一つ選んでアクティブに ──
            ActivateRandom();

            yield return new WaitForSeconds(activeDuration);

            // ── 非アクティブに戻す ──
            DeactivateAll();

            yield return new WaitForSeconds(intervalDuration);
        }
    }

    // ─────────────────────────────────────────
    void ActivateRandom()
    {
        if (cartEntries.Count == 0) return;

        // 直前と同じインデックスを避けて抽選（1種類のみなら気にしない）
        int next = _activeIndex;
        if (cartEntries.Count > 1)
        {
            while (next == _activeIndex)
                next = Random.Range(0, cartEntries.Count);
        }
        else
        {
            next = 0;
        }

        _activeIndex = next;
        SetEntryActive(_activeIndex, true);

        Debug.Log($"[CartManager] アクティブ: {cartEntries[_activeIndex].label} (index={_activeIndex})");
    }

    void DeactivateAll()
    {
        for (int i = 0; i < cartEntries.Count; i++)
            SetEntryActive(i, false);

        _activeIndex = -1;
    }

    void SetEntryActive(int index, bool active)
    {
        var entry = cartEntries[index];
        if (entry.cart != null) entry.cart.SetActive(active);
        if (entry.spline != null) entry.spline.SetActive(active);
    }

    // ─────────────────────────────────────────
    // 外部から手動でアクティブ切り替えたい場合のAPI
    public void ActivateEntry(int index)
    {
        DeactivateAll();
        _activeIndex = index;
        SetEntryActive(index, true);
    }

    public void ForceNextCycle()
    {
        StopAllCoroutines();
        DeactivateAll();
        StartCoroutine(CyclRoutine());
    }
}