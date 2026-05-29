using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// ■ SplineCartHitter  ─  CinemachineSplineCart にアタッチ
///
/// 対応バージョン : Cinemachine 3.x  /  Unity 2023 以降
///
/// 機能
///   1. カートの進行方向へプレイヤー（Rigidbody）を吹っ飛ばす
///   2. スマブラ風ヒットストップ（衝突瞬間に双方を一時停止）
///
/// セットアップ
///   ・このスクリプトを SplineCart の GameObject にアタッチ
///   ・Collider の IsTrigger = OFF（通常衝突）を確認
///   ・プレイヤーのタグを Inspector の PlayerTag に合わせる
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CinemachineSplineCart))]
public class SplineCartHitter : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────
    [Header("吹っ飛ばし設定")]
    [Tooltip("吹っ飛ばす力（大きいほど遠くへ飛ぶ）")]
    public float knockbackForce = 15f;

    [Tooltip("上方向の混ぜ具合（0=水平のみ / 1=真上）スマブラなら 0.3 前後")]
    [Range(0f, 1f)]
    public float upwardRatio = 0.3f;

    [Header("ヒットストップ設定")]
    [Tooltip("停止させる秒数。0.08?0.12 が自然")]
    public float hitstopDuration = 0.1f;

    [Tooltip("プレイヤーに設定しているタグ名")]
    public string playerTag = "Player";

    // ── 内部 ───────────────────────────────────────────────
    private CinemachineSplineCart _cart;

    // カートのワールド移動速度（FixedUpdate で算出）
    private Vector3 _cartVelocity;
    private Vector3 _prevPosition;

    // 多重ヒット防止フラグ
    private bool _onCooldown;

    // ──────────────────────────────────────────────────────
    void Awake()
    {
        _cart = GetComponent<CinemachineSplineCart>();
    }

    void Start()
    {
        _prevPosition = transform.position;
    }

    void FixedUpdate()
    {
        // 位置差分からカートの移動速度ベクトルを算出
        _cartVelocity = (transform.position - _prevPosition) / Time.fixedDeltaTime;
        _prevPosition = transform.position;
    }

    // ──────────────────────────────────────────────────────
    void OnCollisionEnter(Collision collision)
    {
        if (_onCooldown) return;
        if (!collision.gameObject.CompareTag(playerTag)) return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRb == null) return;

        // ── 吹っ飛び方向を決定 ────────────────────────────
        // カートの水平移動方向を基準にする（速度ゼロなら transform.forward を使用）
        Vector3 horizontal = new Vector3(_cartVelocity.x, 0f, _cartVelocity.z);
        Vector3 knockDir = horizontal.sqrMagnitude > 0.01f
            ? horizontal.normalized
            : new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        // 上方向を混ぜてスマブラ的な斜め上吹っ飛びに
        knockDir = (knockDir + Vector3.up * upwardRatio).normalized;

        // PlayerHitReceiver があれば取得（なくても動作する）
        Cart_Player receiver = collision.gameObject.GetComponent<Cart_Player>();

        // ── ヒットストップ → 吹っ飛ばしを開始 ────────────
        StartCoroutine(HitstopThenKnockback(playerRb, knockDir, receiver));

        // 多重ヒット防止クールダウン開始
        _onCooldown = true;
        StartCoroutine(CooldownReset());
    }

    // ──────────────────────────────────────────────────────
    IEnumerator HitstopThenKnockback(
        Rigidbody playerRb,
        Vector3 knockDir,
        Cart_Player receiver)
    {
        // ★ ヒットストップ開始 ───────────────────────────

        // プレイヤー：速度ゼロ + 全軸ロック
        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        var originalConstraints = playerRb.constraints;
        playerRb.constraints = RigidbodyConstraints.FreezeAll;

        // カート：コンポーネントを無効化して移動を止める
        // （Speed=0 より確実。AutoDolly や手動 Speed 両方に対応）
        _cart.enabled = false;

        // 移動スクリプトに通知（PlayerHitReceiver がある場合のみ）
        receiver?.StartHitstop(hitstopDuration);

        // ★ 待機 ─────────────────────────────────────────
        yield return new WaitForSeconds(hitstopDuration);

        // ★ ヒットストップ終了 ───────────────────────────

        // カートを再開
        _cart.enabled = true;

        // プレイヤーを解放して吹っ飛ばす
        playerRb.constraints = originalConstraints;   // 元の制約に戻す
        playerRb.linearVelocity = Vector3.zero;          // 解放直後は 0 から開始
        playerRb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
    }

    // ── クールダウン ───────────────────────────────────────
    IEnumerator CooldownReset()
    {
        yield return new WaitForSeconds(hitstopDuration + 0.25f);
        _onCooldown = false;
    }
}