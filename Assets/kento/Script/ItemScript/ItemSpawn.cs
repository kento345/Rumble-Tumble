using System.Collections.Generic;
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabes;
    private Vector2 posX = new Vector2(-7f, 7f);
    private Vector2 posZ = new Vector2(-7f, 7f);
    private float posY = 10f;
    float timer = 0f;
    float SpawnTime = 5f;
    int lastIndex = -1;

    int maxCount = 5;
    [SerializeField] private List<GameObject> items = new List<GameObject>();

    void Start()
    {
        timer = SpawnTime;
    }

    // Update is called once per frame
    void Update()
    {
        //アイテム生成のタイマー
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnItem();
            timer = SpawnTime;
        }
    }

    void SpawnItem()
    {
        if (itemPrefabes == null || itemPrefabes.Length == 0) { return; }
        //Playerの取得したアイテムのList除外
        items.RemoveAll(item => item == null);
        //アイテム生成制限
        if (items.Count >= maxCount) { return; }
        //アイテム種類の重複防止,ランダム生成
        int i;
        if (itemPrefabes.Length <= 1)
        {
            i = 0;
        }
        else
        {
            //do { 処理 } while(条件);条件にあったらもう一度処理
            do
            {
                i = Random.Range(0, itemPrefabes.Length);
            } while (i == lastIndex);
        }

        //アイテム生成位置のランダム生成
        float x = Random.Range(posX.x, posX.y);
        float y = Random.Range(posZ.x, posZ.y);

        Vector3 ranPos = new Vector3(x, posY, y);
        GameObject onj = (Instantiate(itemPrefabes[i], ranPos, Quaternion.identity));
        items.Add(onj);
        lastIndex = i;
    }
}
