using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int playerIndex;

    void Start()
    {
/*        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.RegisterPlayer(gameObject, playerIndex);
        }*/
    }

    public void OnStart(int id)
    {
        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.RegisterPlayer(gameObject, id);
        }
    }

    public void OnFallOut()
    {
        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.OnPlayerEliminated(gameObject);
        }
        Destroy(gameObject);
    }
}