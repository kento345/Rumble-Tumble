using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 areaSize = new Vector3(10f, 0f, 10f);
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.5f, 2f);

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector3 moveDirection = Vector3.down;

    [Header("デバッグ表示")]
    [SerializeField] private bool showDebugArea = true;
    [SerializeField] private float debugAreaDistance = 10f;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            Vector3 randomPos = GetRandomPosition();
            GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);

            // ランダムスケール
            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // SphereCollider のサイズもスケールに合わせる
            SphereCollider sphere = obj.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                float originalRadius = 0.860137f; // prefab の半径
                sphere.radius = originalRadius * randomScale;
            }

            // 移動コンポーネントを追加
            MoveObject moveComponent = obj.AddComponent<MoveObject>();
            moveComponent.speed = moveSpeed;
            moveComponent.direction = moveDirection.normalized;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-areaSize.x / 2, areaSize.x / 2);
        float y = Random.Range(-areaSize.y / 2, areaSize.y / 2);
        float z = Random.Range(-areaSize.z / 2, areaSize.z / 2);
        return transform.position + new Vector3(x, y, z);
    }

    private void OnDrawGizmosSelected()
    {
        // 生成エリア（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, areaSize);

        // デバッグエリア（赤色）- 移動方向の先に表示
        if (showDebugArea)
        {
            Vector3 normalizedDirection = moveDirection.normalized;
            Vector3 debugPosition = transform.position + normalizedDirection * debugAreaDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(debugPosition, areaSize);

            // 移動方向を示す線（緑色）
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, debugPosition);
        }
    }
}

// 指定方向に移動するコンポーネント
public class MoveObject : MonoBehaviour
{
    [HideInInspector]
    public float speed = 5f;
    [HideInInspector]
    public Vector3 direction = Vector3.down;

    void Update()
    {
        // 指定方向に移動
        transform.position += direction * speed * Time.deltaTime;

        // 遠くに行ったら削除（メモリ節約）
        if (Vector3.Distance(Vector3.zero, transform.position) > 50f)
        {
            Destroy(gameObject);
        }
    }
}