// スクリプトB
using UnityEngine;

public class Delay : MonoBehaviour
{
    public ObjectSpawner ObjectSpawner; // Inspectorで設定
    public float delayTime = 3f; // 3秒後

    void Start()
    {
        // 最初はスクリプトAを無効化
        ObjectSpawner.enabled = false;

        // 指定秒数後にアクティブ化
        Invoke("ActivateScriptA", delayTime);
    }

    void ActivateScriptA()
    {
        ObjectSpawner.enabled = true;
    }
}