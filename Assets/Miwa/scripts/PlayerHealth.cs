using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int playerIndex;

    void Start()
    {
        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.RegisterPlayer(gameObject, playerIndex);
        }
    }

    public void OnFallOut()
    {
        if (GameManager_M.Instance != null)
        {
            // モジュール構造に対応したGameManagerの死亡通知を叩く
            GameManager_M.Instance.OnPlayerEliminated(gameObject);
        }
        Destroy(gameObject);
    }
}