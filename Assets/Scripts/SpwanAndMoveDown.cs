using UnityEngine;

public class SpawnAndMoveDown : MonoBehaviour
{
    [Header("スポーン設定")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float spawnInterval = 2f;

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        if (prefab == null)
        {
            Debug.LogWarning("プレハブが設定されていません！");
            return;
        }

        // このオブジェクトの位置にプレハブを生成
        GameObject spawnedObject = Instantiate(prefab, transform.position, Quaternion.identity);

        // 移動コンポーネントを追加
        MoveDown moveComponent = spawnedObject.AddComponent<MoveDown>();
        moveComponent.speed = moveSpeed;
    }
}

// 真下に移動するコンポーネント
public class MoveDown : MonoBehaviour
{
    [HideInInspector]
    public float speed = 5f;

    void Update()
    {
        // 真下（-Y方向）に移動
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 画面外に出たら削除（オプション）
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}