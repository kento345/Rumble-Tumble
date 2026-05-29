using UnityEngine;

public class TestBall : MonoBehaviour
{
    [Header("Ball Physics")]
    [Tooltip("ボールの質量")]
    [SerializeField] private float mass = 1f;

    [Tooltip("空気抵抗")]
    [SerializeField] private float drag = 0.5f;

    [Tooltip("角度抵抗")]
    [SerializeField] private float angularDrag = 0.05f;

    [Header("Punch Settings")]
    [Tooltip("弱パンチの力")]
    [SerializeField] private float weakPunchForce = 5f;

    [Tooltip("強パンチの力")]
    [SerializeField] private float strongPunchForce = 15f;

    [Header("Debug Controls")]
    [Tooltip("デバッグモードを有効化")]
    [SerializeField] private bool enableDebugControls = true;

    [Tooltip("デバッグ時のパンチ方向")]
    [SerializeField] private Vector3 debugPunchDirection = Vector3.forward;

    private Rigidbody rb;

    private void Start()
    {
        // Rigidbodyの取得または追加
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // 物理パラメータの適用
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
    }

    private void Update()
    {
        // デバッグ用キー入力
        if (enableDebugControls)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ApplyWeakPunch(debugPunchDirection.normalized);
                Debug.Log("弱パンチ実行！");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                ApplyStrongPunch(debugPunchDirection.normalized);
                Debug.Log("強パンチ実行！");
            }
        }
    }

    // 弱パンチを受ける
    public void ApplyWeakPunch(Vector3 direction)
    {
        if (rb != null)
        {
            rb.AddForce(direction.normalized * weakPunchForce, ForceMode.Impulse);
        }
    }

    // 強パンチを受ける
    public void ApplyStrongPunch(Vector3 direction)
    {
        if (rb != null)
        {
            rb.AddForce(direction.normalized * strongPunchForce, ForceMode.Impulse);
        }
    }

    // 外部から力を加える汎用メソッド
    public void ApplyForce(Vector3 direction, float force)
    {
        if (rb != null)
        {
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }
    }

    private void OnValidate()
    {
        // インスペクターで値を変更したときにRigidbodyに即座に反映
        if (rb != null)
        {
            rb.mass = mass;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
        }
    }

    // Gizmoで方向を表示
    private void OnDrawGizmos()
    {
        if (enableDebugControls)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, debugPunchDirection.normalized * 2f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + debugPunchDirection.normalized * 2f, 0.2f);
        }
    }
}