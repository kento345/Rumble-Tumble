//using Unity.Services.Authentication;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainGameManger : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab = default;
    [SerializeField] private GameObject botPrefab = default;

    [SerializeField] private Transform[] pos = default;
    [SerializeField] private GameObject timeUpPanel;

    private GameObject joinObj;

    int i = 0;

    IEnumerator Start()
    {
        yield return null; // 1フレーム待つ
        timeUpPanel.gameObject.SetActive(false);
    }

    private void Awake()
    {
        joinObj = GameObject.Find("JoinedManager"); // オブジェクト名
        
        //インスタンスがない場合はreturn
        if (JoinData.Instance == null) { return; }

        //インスタンスで保持しているデバイス情報を取得
        var devices = JoinData.Instance.GetDevices();

        //人数分Playerの生成
        for (i = 0; i < pos.Length; i++)
        {
            if (i < devices.Count && devices[i] != null)
            {
                var obj = PlayerInput.Instantiate(
                     prefab: playerPrefab,
                     playerIndex: i,
                     pairWithDevice: devices[i]
                );
                obj.transform.position = pos[i].position;
                obj.transform.rotation = pos[i].rotation;
                obj.neverAutoSwitchControlSchemes = true;

                var controller = obj.GetComponent<PlayerInputController>();
                if (controller != null) controller.Init(devices[i], i);
                var health = obj.GetComponent<PlayerHealth>();
                if (health != null) health.playerIndex = i;

                Debug.Log($"Player{i + 1}: inputIndex={obj.playerIndex}, healthIndex={i}, device={devices[i].displayName}");
            }
            else
            {
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