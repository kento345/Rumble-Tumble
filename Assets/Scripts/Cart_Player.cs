using System.Collections;
using UnityEngine;

/// <summary>
/// ■ PlayerHitReceiver  ─  プレイヤーにアタッチ（任意・推奨）
///
/// SplineCartHitter からヒットストップを受け取り、
/// 移動スクリプトが CanMove を参照することで
/// ヒットストップ中の入力処理をスキップできる。
///
/// ── 既存の移動スクリプトへの組み込み方 ──────────────
///
///   // フィールドに追加
///   private PlayerHitReceiver _hitReceiver;
///
///   void Awake()
///   {
///       _hitReceiver = GetComponent<PlayerHitReceiver>();
///   }
///
///   void FixedUpdate()      // または Update()
///   {
///       // ヒットストップ中は入力を無視する
///       if (_hitReceiver != null && !_hitReceiver.CanMove) return;
///
///       // ↓ 以下は既存の移動処理
///   }
/// ────────────────────────────────────────────────────
/// </summary>
public class Cart_Player : MonoBehaviour
{
    /// <summary>
    /// true  ＝ 通常（移動可能）
    /// false ＝ ヒットストップ中（移動スクリプト側でスキップ）
    /// </summary>
    public bool CanMove { get; private set; } = true;

    /// <summary>
    /// SplineCartHitter から呼ばれる。duration 秒間 CanMove を false にする。
    /// </summary>
    public void StartHitstop(float duration)
    {
        StopAllCoroutines();                    // 連続ヒット時に前のコルーチンをリセット
        StartCoroutine(HitstopRoutine(duration));
    }

    private IEnumerator HitstopRoutine(float duration)
    {
        CanMove = false;
        yield return new WaitForSeconds(duration);
        CanMove = true;
    }
}