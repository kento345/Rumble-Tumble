using UnityEngine;



public class MoveController : MonoBehaviour,Initalize
{
    [Header("移動設定")]
    public float Speed = 5.0f;
    private float Speed2 = 0f;
    [SerializeField] private float chargingmoveSpeedRate = 0.3f;
    private float curentSpeed = 0f;

    [SerializeField] private float rotSpeed = 10.0f;
    private float rotSpeed2 = 0f;
    [SerializeField] private float ChargeingRotSpeedRate = 0.7f;
    private float curentRotSpeed = 0f;

    Vector2 inputVer;

    Rigidbody rb;

    //-----Script参照-----
    private PlayerStateManager stateManager;
    private AtackController ac;

    //初期化
    public void Inited()
    {
        inputVer = Vector2.zero;

        curentSpeed = Speed;
        curentRotSpeed = rotSpeed;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    private void Awake()
    {
        Speed2 = Speed * chargingmoveSpeedRate;
        rotSpeed2 = rotSpeed * ChargeingRotSpeedRate;
        curentSpeed = Speed;

        rb = GetComponent<Rigidbody>();
        stateManager = GetComponent<PlayerStateManager>();
        ac = GetComponent<AtackController>();
    }

    public void SetMoveInput(Vector2 input)
    {
        inputVer = input;
    }

    // Update is called once per frame
    void Update()
    {
        //stateManager.UpdateMoveState(inputVer);
        Move();
    }

    public void Move()
    {
        //サドンデスにて速度10倍に
        float suddenDeathMultiplier = 1.0f;
        if(GameManager_M.Instance !=null&&GameManager_M.Instance.CurrentModeState==GameManager_M.Mode.SuddenDeath)
        {
            suddenDeathMultiplier = GameManager_M.Instance.suddenDeathSpeedMultiplier;
        }

        if (ac.isRigid) { return; }
        if(stateManager.State == State.Knockback) { return; }

        curentSpeed = Speed*suddenDeathMultiplier;
        curentRotSpeed = rotSpeed;

        if (stateManager.ActionState == ActionState.Charge)
        {
            curentSpeed = (Speed*chargingmoveSpeedRate)*suddenDeathMultiplier;
            curentRotSpeed = rotSpeed2;
        }

        if (stateManager.MoveState == MoveState.Idle) { return; }

        if (stateManager.ActionState != ActionState.Attack)
        {
            Vector3 move = new Vector3(inputVer.x, 0, inputVer.y) * curentSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);

            if (move != Vector3.zero)
            {
                Quaternion Rot = Quaternion.LookRotation(move, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Rot, curentRotSpeed * Time.deltaTime));
            }
        }
    }
}
