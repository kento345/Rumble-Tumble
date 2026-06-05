using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainGameManger : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab = default;
    [SerializeField] private GameObject botPrefab = default;

    [SerializeField] private Transform[] pos = default;
    [SerializeField] private GameObject timeUpPanel;

    private GameObject joinObj;

    IEnumerator Start()
    {
        yield return null; // 1フレーム待つ
        if (timeUpPanel != null) timeUpPanel.SetActive(false);
    }

    private void Awake()
    {
        joinObj = GameObject.Find("JoinedManager");

        if (JoinData.Instance == null) { return; }

        var devices = JoinData.Instance.GetDevices();

        // 生成したプレイヤーをGameManager_Mに登録するためのリスト（または配列）
        // ※GameManager_M側の仕様に合わせて直接登録するか、PlayerHealth経由で自動登録させます
        for (int i = 0; i < pos.Length; i++)
        {
            if (i < devices.Count && devices[i] != null)
            {
                // 1. プレイヤーをコントローラーと紐づけて生成
                var obj = PlayerInput.Instantiate(
                     prefab: playerPrefab,
                     playerIndex: i,
                     pairWithDevice: devices[i]
                );

                obj.transform.position = pos[i].position;
                obj.transform.rotation = pos[i].rotation;
                obj.neverAutoSwitchControlSchemes = true;

                // 【修正ポイント】廃止された Controller の初期化を削除し、
                // プレイヤーの管理は新システム（PlayerHealth や GameManager_M）に一任する
                var health = obj.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.playerIndex = i;
                    // もしStart()のタイミングではなく、ここで即時登録したい場合は以下を有効にします
                    // if (GameManager_M.Instance != null) GameManager_M.Instance.RegisterPlayer(obj, i);
                }

                Debug.Log($"Player{i + 1}: inputIndex={obj.playerIndex}, healthIndex={i}, device={devices[i].displayName}");
            }
            else
            {
                // ボットの生成
                var bot = Instantiate(botPrefab, pos[i].position, pos[i].rotation);
                var health = bot.GetComponent<PlayerHealth>();
                if (health != null) health.playerIndex = i;

                Debug.Log($"Bot{i + 1}: index={i}");
            }
        }
    }

    public void OnReset()
    {
        SceneManager.LoadScene("Start");
        Destroy(joinObj);
    }
}