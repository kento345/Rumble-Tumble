using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputController : MonoBehaviour
{
    private int playerIndex;
    private InputDevice myDevice;

    public void Init(InputDevice device, int index)
    {
        myDevice = device;
        playerIndex = index;
    }

    /*変更点あり*/
    //Player操作反転
    private bool isInverted = false;

    Vector2 inputVer;
    public Vector2 InputVeer => inputVer; // 追加


    private PlayerStateManager stateManager;
    private MoveController move;
    private AtackController atack;

    
    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
        move = GetComponent<MoveController>();
        atack = GetComponent<AtackController>();
    }

    private void OnEnable()
    {
       OnMoveStop(true);
        var initScripts = GetComponents<Initalize>();
        foreach (var script in initScripts)
        {
            script.Inited();
        }
    }

    public void OnMoveStop(bool x)
    {
        // このScriptを無効化,有効化
        move.enabled = x;
        atack.enabled = x;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!move.enabled) return;
        if (stateManager.State == State.Knockback)
        {
            move.SetMoveInput(Vector2.zero);
            stateManager.UpdateMoveState(Vector2.zero);
            return;
        }
        inputVer = context.ReadValue<Vector2>();

        if (isInverted)
        {
            //操作反転
            inputVer *= -1;
        }

        //ステート変更のための入力受け取り
        stateManager.UpdateMoveState(inputVer);
        //移動処理のための入力受け取り
        move.SetMoveInput(inputVer);
    }

    public void OnAtatck(InputAction.CallbackContext context)
    {
        if (!atack.enabled) return;
        if (context.performed)
        {
            atack.Shot(0);
        }
        if (context.canceled)
        {
            atack.Shot(1);
        }
    }

    public void SetReverse(bool value)
    {
        isInverted = value;
    }
}
