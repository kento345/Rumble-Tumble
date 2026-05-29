using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    [SerializeField] public float lifetime = 5f; // 何秒後に消えるか

    private float timer = 0f; // 経過時間をカウント

    void Update()
    {
        // 前のフレームからの時間を加算
        timer += Time.deltaTime;

        // 一定時間を超えたら削除
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
