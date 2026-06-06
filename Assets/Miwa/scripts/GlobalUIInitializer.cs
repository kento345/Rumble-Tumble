using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalUIInitializer : MonoBehaviour
{
    private static GlobalUIInitializer instance;

    private void Awake()
    {
        // 二重生成を防ぎ、シーンをまたいでも消えないようにする
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // シーンが読み込まれたときに実行されるイベントを登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // タイトルシーンに戻ってきたときだけ実行（シーン名は自分のものに合わせてください）
        if (scene.name == "title_UI")
        {
            StartCoroutine(ForceFocusRoutine());
        }
    }

    private IEnumerator ForceFocusRoutine()
    {
        // 1. まず時間を確実に動かす
        Time.timeScale = 1f;

        // 2. シーン内の全オブジェクトが立ち上がるまで少し待つ
        yield return new WaitForSecondsRealtime(0.2f);

        if (EventSystem.current != null)
        {
            // 3. 名前でボタンを探す（動画で使っていたボタン名にする）
            GameObject target = GameObject.Find("StartButton");

            if (target == null) target = GameObject.Find("Option_BT");

            if (target != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(target);
            }
        }
    }

    private void OnDestroy()
    {
        // オブジェクトが破棄されるときはイベント登録を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
