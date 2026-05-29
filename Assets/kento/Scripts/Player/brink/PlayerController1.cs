using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class PlayerController1 : MonoBehaviour
{
    [Header("移動設定")]

    [SerializeField] private float speed = 5.0f; //移動スピード
    [SerializeField] private float ChargeMoveSpeedRate = 0.3f; //チャージ・硬直中の速度倍率
    private float speed2 = 0; //チャージ中のスピード
    private float curentSpeed = 0;  //現在のスピード
    [SerializeField] private float rotSpeed = 10.0f; //旋回スピード
    [SerializeField] private float ChargeRotateSpeedRate = 0.7f; //チャージ・硬直中の旋回倍率
    private float rotSpeed2 = 0;　//チャージ中旋回スピード
    private float curentRotSpeed = 0;//現在の旋回スピード

    private Vector2 inputVer;


    [Header("ブリンク設定")]

    [SerializeField] private float tackleForce;    //ブリンク力
    [SerializeField] private float tackleDuration = 0.5f;//持続時間
    [SerializeField] private float tackleCooldown = 1.0f;//クールダウン時間

    //-----硬直-----
    [SerializeField] private float StrongRecoveryTime = 1.0f; //硬直時間
    private float curentRecoveryTime;
    private bool isfinish = false;

    private bool isPrese = false; //攻撃キー入力フラグ
     public bool isStrt = false;//チャージ開始フラグ
    private float t = 0f; //チャージ量
    [HideInInspector] public float chargeMax = 5.0f; //チャージ上限
    private bool isMax = false;//チャージがMaxかのフラグ

    bool isAttack1 = false;
    bool isAttack2 = false;

    [Header("ノックバック,無敵設定")]
    [SerializeField] private float WeakKnockbackForce = 2.5f; //弱ブリンクノックバック
    [SerializeField] private float StrongKnockbackForce = 5.0f;//強ブリンクノックバック
    private float curentknockbackForce = 0f;//現在のノックバック力


    private Rigidbody rb;
    private bool isTackling = false;
    private float lastTackleTime = 0f; // 最後のタックル時間


    [Header("当たり判定設定")]
    [SerializeField] private SphereCollider searchArea;
    [SerializeField] private float angle = 45f;

    [Header("エフェクト設定")]
    [SerializeField] private ParticleSystem run;//走り
    [SerializeField] private ParticleSystem chage;//チャージ
    [SerializeField] private ParticleSystem strong;//強
    [SerializeField] private ParticleSystem weak;//弱


    //-----PlayerID-----
    private PlayerInput playerInput;
    [SerializeField] private Text IDtext;

    Reception1 reception;
    Animator animator;

    private void Awake()
    {
        speed2 = speed * ChargeMoveSpeedRate;
        rotSpeed2 = rotSpeed * ChargeRotateSpeedRate;
        curentRecoveryTime = StrongRecoveryTime;

        run.Stop();
        chage.Stop();
        strong.Stop(); 
        weak.Stop();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        if (!JoinData.Instance.GetDevices().Contains(context.control.device))
            return;


        if (GameManager_M.Instance != null && !GameManager_M.Instance.IsGameStartedProperty){inputVer = Vector2.zero;return;}
        inputVer = context.ReadValue<Vector2>();
        if (context.performed)
        {
            if (run && !run.isPlaying)
                run.Play();
        }

        if (context.canceled)
        {
            if (run && run.isPlaying)
                run.Stop();
        }
    }

    public void OnTackle(InputAction.CallbackContext context)
    {
        if (!JoinData.Instance.GetDevices().Contains(context.control.device))
            return;

        if (GameManager_M.Instance != null && !GameManager_M.Instance.IsGameStartedProperty) return;
        if (context.performed)
        {
            if (isStrt || isTackling) return;
            isfinish = false;

            if (!isTackling && Time.time > lastTackleTime + tackleCooldown)
            {
                isStrt = true;
                isPrese = true;
                chage.Play();
            }
        }
        if (context.canceled)
        {
            isPrese = false;
            if (isStrt && !isTackling && Time.time > lastTackleTime + tackleCooldown)
            {
                chage.Stop();
                Tackle();
            }
            isStrt = false;
        }
    }

    public void SetCharge(float value)
    {
        t = value;
    }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
      /*  if (playerInput != null)
        {
            playerID = playerInput.playerIndex;
        }

        IDtext.text += $"Player {playerID + 1}\n";*/

        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        reception = GetComponent<Reception1>();
    }

    void FixedUpdate()
    {
        if (reception != null && !reception.isKnockback)
        {
            if (isStrt)
            {
                if (t < chargeMax)
                {
                    t += Time.deltaTime;
                }
                if (t >= chargeMax)
                {
                    isMax = true;
                }
            }
            else if (!isStrt)
            {
                t = 0f;
            }
            if (isfinish)
            {
                if (curentRecoveryTime > 0)
                {
                    curentRecoveryTime -= Time.deltaTime;
                }
                if (curentRecoveryTime <= 0)
                {
                    isfinish = false;
                    curentRecoveryTime = StrongRecoveryTime;
                }
            }
            Move();
        }



        float mag = inputVer.magnitude;
        animator.SetFloat("Speed", mag);
        animator.SetBool("IsChage", isStrt);
        animator.SetBool("IsAttack1", isAttack1);
        animator.SetBool("IsAttack2", isAttack2);
    }

    void Move()
    {
        if (isfinish) { return; }
        //if (reception != null && reception.isKnockback) return;

        if (isPrese)
        {
            curentSpeed = speed2;
            curentRotSpeed = rotSpeed2;
        }
        if (!isPrese)
        {
            curentSpeed = speed;
            curentRotSpeed = rotSpeed;
        }

        if (!isTackling)
        {
            Vector3 move = new Vector3(inputVer.x, 0f, inputVer.y) * curentSpeed * Time.deltaTime;
            //transform.position += move;
            rb.MovePosition(rb.position + move);



            if (move != Vector3.zero)
            {
                Quaternion Rot = Quaternion.LookRotation(move, Vector3.up);
                //transform.rotation = Quaternion.Slerp(transform.rotation,Rot,curentRotSpeed * Time.deltaTime);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Rot, curentRotSpeed * Time.fixedDeltaTime));
            }
        }
    }

    void Tackle()
    {
        if (isfinish) { return; }
        isStrt = false;
        if (reception != null && reception.isKnockback) return;
        isTackling = true;
        lastTackleTime = Time.time;

        if (isMax)
        {
            curentknockbackForce = StrongKnockbackForce;
            strong.Play();
            isAttack2 = true;
        }
        else
        {
            curentknockbackForce = WeakKnockbackForce;
            weak.Play();
            isAttack1 = true;
        }

        rb.AddForce(transform.forward * tackleForce, ForceMode.Impulse);

        Invoke("EndTackle", tackleDuration);

    }



    void EndTackle()
    {
        rb.linearVelocity = Vector3.zero;
        isTackling = false;
        strong.Stop();
        weak.Stop();

        isAttack1 = false;
        isAttack2 = false;

        //ここで硬直処理
        if (isMax)
        {
            isAttack2 = false;
            isfinish = true;
        }

        isMax = false;

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 posDir = other.transform.position - this.transform.position;
            float target_angle = Vector3.Angle(this.transform.forward, posDir);

            var dist = Vector3.Distance(other.transform.position, transform.position);

            if (target_angle > angle) { return; }

            if (target_angle <= angle)
            {
                if (Physics.Raycast(this.transform.position + Vector3.up * 1.2f, posDir, out RaycastHit hit))
                {
                    if (hit.collider == other)
                    {
                        if (isTackling)
                        {
                            Reception1 p = other.gameObject.GetComponent<Reception1>();
                            if (p.isHit) { return; }
                            p.KnockBack(rb.linearVelocity.normalized, curentknockbackForce);
                           

                            //当たった時点でInvokeをキャンセルしてタックルを止める
                            CancelInvoke("EndTackle");
                            EndTackle();
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var pos = transform.position;
        pos.y = 1.0f;
        Handles.color = Color.red;
        Handles.DrawSolidArc(pos, Vector3.up, Quaternion.Euler(0.0f, -angle, 0f) * transform.forward, angle * 2f, searchArea.radius);
    }
#endif


    // リスポーン時などに呼び出すリセット処理
    public void ResetPlayerState()
    {
        
        inputVer = Vector2.zero;
        isPrese = false;
        isStrt = false;
        isTackling = false;
        isMax = false;
        isfinish = false;
        isAttack1 = false;
        isAttack2 = false;
        
        t = 0f;

        if (run != null) run.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (chage != null) chage.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (strong != null) strong.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (weak != null) weak.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChage", false);
            animator.SetBool("IsAttack1", false);
            animator.SetBool("IsAttack2", false);
        }
    }
}


