using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinData : MonoBehaviour
{
    public static JoinData Instance { get; private set; } //Playerの接続データインスタンス

    private List<InputDevice> devices = new();  //参加中のPlayerデバイス

    //private InputDevice[] devices;                                


    private void Awake()
    {
        //既に存在する場合は、新しく生成された方を破棄する。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //インスタンスに自身を取得,シーンをまたいでも破壊されない
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// デバイスの保存
    /// </summary>
    /// <param name="joinDevices"></param>
    public void SetDevice(List<InputDevice> joinDevices)
    {
        devices = new List<InputDevice>(joinDevices);
    }
    /// <summary>
    /// デバイス取得
    /// </summary>
    /// <returns></returns>
    public List<InputDevice> GetDevices()
    {
        return devices;
    }
    /// <summary>
    /// Player人数取得
    /// </summary>
    /// <returns></returns>
    public int GetPlayerCount()
    {
        return devices.Count;
    }
    /// <summary>
    /// 指定Playerのデバイス確認
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool IsDeviceForPlayer(int playerIndex, InputDevice device)
    {
        if (device == null) return false;
        if (playerIndex < 0 || playerIndex >= devices.Count) return false;

        return devices[playerIndex] == device;
    }
}
