using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoineManager : MonoBehaviour
{
    [SerializeField] private InputAction joinAction = default;  //参加するときの入力
    [SerializeField] private InputAction leaveAction = default;  //参加するときの入力
    [SerializeField] private InputAction startAction = default;

    private int maxPlayers = 4;        //参加上限

    //----------
    [SerializeField] private Text device1text;         //1デバイス名Text
    [SerializeField] private Text device2text;         //2デバイス名Text
    [SerializeField] private Text device3text;         //3デバイス名Text
    [SerializeField] private Text device4text;         //4デバイス名Text

    //----------
    [SerializeField] private GameObject playerPrefab = default; //Player
    private Dictionary<int,GameObject> playerObjects = new (); //生成したPlayerオブジェクトのリスト

    private Dictionary<int, int> playerMap = new();
    private List<InputDevice> joinDevices = new List<InputDevice>();             //参加中のデバイス

    private void Awake()
    {
        //最大参加可能数で配列を初期化
        for (int i = 0; i < maxPlayers; i++)
        {
            joinDevices.Add(null);
        }
        playerMap = new Dictionary<int, int>(maxPlayers);
        // InputActionを有効化し、コールバックを設定
        joinAction.Enable();
        joinAction.performed += OnJoin;

        leaveAction.Enable();
        leaveAction.performed += OnLeave;

        startAction.Enable();
        startAction.performed += OnGameStarte;

        //-----Text非表示-----
        device1text.enabled = false;
        device2text.enabled = false;
        device3text.enabled = false;
        device4text.enabled = false;
    }


    private void OnDestroy()
    {
        joinAction.performed -= OnJoin;
        leaveAction.performed -= OnLeave;
        startAction.RemoveAllBindingOverrides();
    }


    //-----参加-----
    private void OnJoin(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;
        for (int i = 0; i < joinDevices.Count; i++)
        {
            if (joinDevices[i] == device)
            {
                return;
            }
        }
        int playerIndex = -1;
        for (int i = 0; i < joinDevices.Count; i++)
        {
            if (joinDevices[i] == null)
            {
                joinDevices[i] = device;

                playerIndex = i;
                Debug.Log($"{i + 1}P参加");

                break;
            }
        }

        // deviceId → playerIndex
        playerMap[device.deviceId] = playerIndex;
        Debug.Log($"Join : DeviceID {device.deviceId} → Player{playerIndex}");
        UpdateDeviceTexts();
        CreatePlayer(device);
    }

    //-----退出-----
    void OnLeave(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;
        if (!joinDevices.Contains(device)) { return; }

        for (int i = 0; i < joinDevices.Count; i++)
        {
            if (joinDevices[i] == device)
            {
                joinDevices[i] = null;

                Debug.Log($"{i + 1}P退出");

                break;
            }
        }

        if (playerMap.ContainsKey(device.deviceId))
        {
            playerMap.Remove(device.deviceId);
        }

        UpdateDeviceTexts();

        if (playerObjects.TryGetValue(device.deviceId,out GameObject obj))
        {
            PlayerDataHolder.Instance.RemoveData(obj);
            var input = obj.GetComponent<PlayerInput>();

            if(input != null)
            {
                input.user.UnpairDevicesAndRemoveUser();
            }
            Destroy(obj);

            playerObjects.Remove(device.deviceId);
        }
    }

    //-----UIの更新-----
    void UpdateDeviceTexts()
    {
        Text[] texts = { device1text, device2text, device3text, device4text };

        for (int i = 0; i < texts.Length; i++)
        {
            if (joinDevices[i] != null)
            {
                texts[i].enabled = true;

                texts[i].text =
                    $"{joinDevices[i].displayName}\n参加中";
            }
            else
            {
                texts[i].enabled = false;
                texts[i].text = "";
            }
        }
    }


    //StartButtonが押されたときのScene移行
    public void OnGameStarte(InputAction.CallbackContext context)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Title" || currentSceneName == "Start")
        {
            /*            startAction.Disable();
                        joinAction.Disable();
                        leaveAction.Disable();*/

            SceneManager.LoadScene("prot");
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

    void CreatePlayer(InputDevice device){
        if(device == null) return;
        //保持しているデバイス情報と人数を取得
        int playerIndex = playerMap[device.deviceId];

        var obj = PlayerInput.Instantiate(
               prefab: playerPrefab,
               playerIndex: playerIndex,
               pairWithDevice: device
           );
        //obj.transform.position = transform.position;
        PlayerDataHolder.Instance.SetData(obj.gameObject);
        DontDestroyOnLoad(obj);
        playerObjects[device.deviceId] = obj.gameObject;
    }
}
