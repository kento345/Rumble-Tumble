using UnityEngine;
using System.Reflection;
using static GameMode;

public class SuddenDeathMode : IGameMode
{
    public void OnEnter()
    {
        // UIの表示
        if (GameManager_M.Instance != null && GameManager_M.Instance.suddenDeathUI != null)
        {
            GameManager_M.Instance.suddenDeathUI.SetActive(true);
        }

        // ★重要：ここで「現在の倍率」をセットする
        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.currentKnockbackMultiplier = GameManager_M.Instance.suddenDeathKnockbackMultiplier;
        }
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        // UIの非表示
        if (GameManager_M.Instance != null && GameManager_M.Instance.suddenDeathUI != null)
        {
            GameManager_M.Instance.suddenDeathUI.SetActive(false);
        }

        // 倍率を元に戻す
        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.currentKnockbackMultiplier = 1.0f;
        }
    }

    public void PowerUpSinglePlayer(GameObject player) { }
}