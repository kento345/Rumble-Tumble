using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEditorInternal;
#endif
using UnityEngine;

public class AtackController : MonoBehaviour , Initalize
{
    /*変更点あり*/
    [SerializeField] private float curentForce = 15f;
    private float duration = 0.5f;
    private float cooldown = 1.0f; //攻撃クールダウン
    private bool lisCooldown = false;

    [HideInInspector] public float chargeMax = 5.0f; //チャージ上限
    private float t = 0f;
    private bool isMax = false;

    //-----硬直-----
    [SerializeField] private float StrongRecoveryTime = 1.0f; //硬直時間
    private float curentRecoveryTime;
    [HideInInspector] public bool isRigid = false;

    [Header("ノックバック,無敵設定")]
    public float WeakKnockbackForce = 15.0f; //弱ノックバック
    public float StrongKnockbackForce = 30.0f;//強ノックバック
    private float curentknockbackForce = 0f;//現在のノックバック力

    [Header("当たり判定設定")]
    [SerializeField] private SphereCollider searchArea;
    [SerializeField] private float angle = 45f;

    bool hasHit = false;

    Rigidbody rb;
    PlayerStateManager stateManager;
    AnimatorController animeCon;

    //初期化
    public void Inited()
    {
        StopAllCoroutines();
        CancelInvoke();

        t = 0f;
        curentRecoveryTime = StrongRecoveryTime;

        isMax = false;
        isRigid = false;
        hasHit = false;
        lisCooldown = false;
        if (searchArea != null)
        {
            searchArea.enabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        if (animeCon != null)
        {
            animeCon.isStart = false;
            animeCon.isAttack1 = false;
            animeCon.isAttack2 = false;
        }
        if (stateManager != null)
        {
            stateManager.SetActionState(ActionState.None);
            stateManager.SetAttackPower(AttackPower.None);
        }
    }

    public void SetCharge(float value)
    {
        t = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stateManager = GetComponent<PlayerStateManager>();
        animeCon = GetComponent<AnimatorController>();
    }


    private void Start()
    {
        curentRecoveryTime = StrongRecoveryTime;
    }

    void Update()
    {
        if (stateManager.ActionState != ActionState.Attack)
        {
            searchArea.enabled = false;
        }
        if (stateManager.ActionState == ActionState.Charge)
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
        if (stateManager.State == State.Knockback)
        {
            SetCharge(0);
            animeCon.isStart = false;
            CancelInvoke("EndAttack");
            EndAttack();
            stateManager.SetAttackPower(AttackPower.None);
        }
        /*else
        {
            t = 0f;
            //stateManager.SetAttackPower(AttackPower.None);
        }*/
        if (isRigid)
        {
            if (curentRecoveryTime > 0f)
            {
                curentRecoveryTime -= Time.deltaTime;
            }
            if (curentRecoveryTime <= 0f)
            {
                isRigid = false;
                curentRecoveryTime = StrongRecoveryTime;
            }
        }
    }

    public void Shot(int x)
    {
        if (x == 0)
        {
            if (lisCooldown) { return; }
            if (stateManager.ActionState == ActionState.Charge) { return; }
            isRigid = false;

            //チャージ開始,ステート変更
            stateManager.SetActionState(ActionState.Charge);
            animeCon.isStart = true;
        }
        if (x == 1)
        {
            if (lisCooldown) { return; }
            if (isRigid) { return; }
            animeCon.isStart = false;
            if (stateManager.ActionState == ActionState.Charge)
            {
                //チャージを止め攻撃,ステート変更
                stateManager.SetActionState(ActionState.Attack);
                searchArea.enabled = true;
                //? = true , : = false
                //curentknockbackForce = isMax ? StrongKnockbackForce : WeakKnockbackForce;

                float multiplier = 1.0f;
                if (GameManager_M.Instance != null &&
        GameManager_M.Instance.CurrentModeState == GameManager_M.Mode.SuddenDeath)
                {
                    multiplier = GameManager_M.Instance.suddenDeathKnockbackMultiplier;
                }

                if (isMax)
                {

                    curentknockbackForce = StrongKnockbackForce * multiplier;
                    stateManager.SetAttackPower(AttackPower.Strong);
                    animeCon.isAttack2 = true;
                    Debug.Log("強");
                }
                else
                {
                    curentknockbackForce = WeakKnockbackForce * multiplier;
                    stateManager.SetAttackPower(AttackPower.Weak);
                    animeCon.isAttack1 = true;
                    Debug.Log("弱");
                }

                rb.linearVelocity = Vector3.zero;
                rb.AddForce(transform.forward * curentForce, ForceMode.Impulse);

                Invoke("EndAttack", duration);
            }

        }
    }

    void EndAttack()
    {
        rb.linearVelocity = Vector3.zero;
        //ステートをNoneに
        stateManager.SetActionState(ActionState.None);

        hasHit = false;

        if (isMax)
        {
            isRigid = true;
        }

        isMax = false;
        animeCon.isAttack1 = false;
        animeCon.isAttack2 = false;
        stateManager.SetAttackPower(AttackPower.None);
        t = 0f;

        StartCoroutine(CooldownCount());
    }

    IEnumerator CooldownCount()
    {
        lisCooldown = true;
        yield return new WaitForSeconds(cooldown);
        lisCooldown = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasHit) { return; }
        if (stateManager == null || rb == null) { return; }

        if (stateManager.ActionState != ActionState.Attack) { return; }
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 posDir = other.transform.position - this.transform.position;
            float target_angle = Vector3.Angle(this.transform.forward, posDir);

            var dist = Vector3.Distance(other.transform.position, transform.position);

            if (target_angle > angle) { return; }
            float radius = searchArea.radius * transform.localScale.x;
            if (target_angle <= angle && Vector3.Distance(transform.position, other.transform.position) <= radius)
            {
                hasHit = true;

                Reception p = other.GetComponent<Reception>();
                if (p == null) { return; }
                p.KnockBack(rb.linearVelocity.normalized, curentknockbackForce);
                CancelInvoke("EndAttack");
                EndAttack();
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
}