using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDataHolder : MonoBehaviour
{
    public static PlayerDataHolder Instance { get; private set; } //Playerの接続データインスタンス
    public List<GameObject> players = new();
    public List<int> ID = new();

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

    public void SetData(GameObject player, int id)
    {
        players.Add(player);
    }
    public void RemoveData(GameObject player,int id)
    {
        players.Remove(player);
        ID.Remove(id);
    }
}