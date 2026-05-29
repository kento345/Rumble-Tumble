using UnityEngine;

public class AcceleratingBall : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float startAngle = 45f;
    [SerializeField] private float accelerationRate = 1.1f;
    [SerializeField] private float maxSpeed = 150.0f;

    private Vector3 velocity;

    void Start()
    {
        // 開始角度をラジアンに変換してxz平面上の速度ベクトルを設定
        float angleRad = startAngle * Mathf.Deg2Rad;
        velocity = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * speed;
    }

    void Update()
    {
        // xz平面上でボールを移動
        transform.position += velocity * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // "Bound"タグのオブジェクトと衝突した場合
        if (collision.gameObject.CompareTag("Bound"))
        {
            // 衝突点の法線を取得（最初の接触点を使用）
            Vector3 normal = collision.contacts[0].normal;

            // 法線のy成分を0にしてxz平面上に投影
            normal.y = 0f;
            normal.Normalize();

            // 速度ベクトルを反射
            velocity = Vector3.Reflect(velocity, normal);

            // 速度を加速
            velocity *= accelerationRate;

            // 最高速度を超えないように制限
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }

            Debug.Log($"反射！現在の速度: {velocity.magnitude}");
        }
    }
}