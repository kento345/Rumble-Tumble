using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoinController : MonoBehaviour
{
    [SerializeField] private InputAction joinAction = default;
    [SerializeField] private InputAction leaveAction = default;
    [SerializeField] private InputAction startAction = default;

    private int maxPlayers = 4;

    //----------
    [SerializeField] private Text device1text;         //1デバイス名Text
    [SerializeField] private Text device2text;         //2デバイス名Text
    [SerializeField] private Text device3text;         //3デバイス名Text
    [SerializeField] private Text device4text;         //4デバイス名Text

    private Dictionary<InputDevice, int> playerMap = new();
    [SerializeField] private List<InputDevice> joinDevices = new();
    [SerializeField] private List<string> debugDevices = new();
    public IReadOnlyList<InputDevice> joinDeviceList => joinDevices;

    /// <summary>
    /// 参加,退出のAction有効化
    /// </summary>
    private void Awake()
    {
        //初期化
        joinDevices = new List<InputDevice>(maxPlayers);
        //参加InputAction有効化
        joinAction.Enable();
        joinAction.performed += OnJoin;
        //退出InputAction有効化
        leaveAction.Enable();
        leaveAction.performed += OnLeave;
        //開始InputAction有効化
        startAction.Enable();
        startAction.performed += OnGameStarte;
        //-----Text非表示-----
        device1text.enabled = false;
        device2text.enabled = false;
        device3text.enabled = false;
        device4text.enabled = false;
    }
    //Scene変更後にAction無効化
    private void OnDestroy()
    {
        joinAction.performed -= OnJoin;
        joinAction.Disable();

        leaveAction.performed -= OnLeave;
        leaveAction.Disable();

        startAction.performed -= OnGameStarte;
        startAction.Disable();
    }

    /// <summary>
    /// 参加処理
    /// </summary>
    /// <param name="context"></param>
    void OnJoin(InputAction.CallbackContext context)
    {
        //入力デバイス取得
        var device = context.control.device;

        if (playerMap.ContainsKey(device)) return;
        //参加人数がMaxならreturn
        if (joinDevices.Count >= maxPlayers) { return; }
        //重複防止
        if (joinDevices.Contains(device)) { return; }

        int playerIndex = joinDevices.Count;

        //List追加
        joinDevices.Add(device);
        playerMap[device] = playerIndex;

        //UIの更新
        UpdateDeviceTexts();
    }
    /// <summary>
    /// 退出処理
    /// </summary>
    /// <param name="context"></param>
    void OnLeave(InputAction.CallbackContext context)
    {
        var device = context.control.device;
        if (!joinDevices.Contains(device)) { return; }
        if (!playerMap.ContainsKey(device)) return;

        int index = playerMap[device];

        joinDevices.Remove(device);
        playerMap.Remove(device);

        // ★インデックス再計算（ここが重要）
        RebuildMap();
        //UIの更新
        UpdateDeviceTexts();
    }

    private void RebuildMap()
    {
        playerMap.Clear();

        for (int i = 0; i < joinDevices.Count; i++)
        {
            playerMap[joinDevices[i]] = i;
        }
    }

    /// <summary>
    /// UIの更新
    /// </summary>
    void UpdateDeviceTexts()
    {
        Text[] texts = { device1text, device2text, device3text, device4text };

        debugDevices.Clear();

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].enabled = false;
            texts[i].text = "";
        }

        for (int i = 0; i < joinDevices.Count; i++)
        {
            texts[i].enabled = true;
            texts[i].text = $"{joinDevices[i].displayName}\n参加中";

            debugDevices.Add(joinDevices[i].displayName);
        }

    }

    //StartButtonが押されたときのScene移行
    public void OnGameStarte(InputAction.CallbackContext context)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Title" || currentSceneName == "Start")
        {
            startAction.Disable();
            joinAction.Disable();
            leaveAction.Disable();

            JoinData.Instance.SetDevice(joinDevices);
            SceneManager.LoadScene("ModeSelect_ui");
        }
    }
    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Title" || currentSceneName == "JoinScene")
        {
            startAction.Enable();
            joinAction.Enable();
            leaveAction.Enable();
        }
    }

}
