using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BotPlayerController1 : MonoBehaviour
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

    [Header("攻撃設定")]

    [SerializeField] private float tackleForce;    //ブリンク力
    [SerializeField] private float tackleDuration = 0.5f;//持続時間
    [SerializeField] private float tackleCooldown = 1.0f;//クールダウン時間

    //-----硬直-----
    [SerializeField] private float StrongRecoveryTime = 1.0f; //硬直時間
    private float curentRecoveryTime;
    private bool isfinish = false;
    private float r;

    private bool isPrese = false; //攻撃キー入力フラグ
    [HideInInspector] public bool isStrt = false;//チャージ開始フラグ
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

    [Header("サーチ設定")]
    [SerializeField] private float searchInterval = 0.5f;
    [SerializeField] private float searchRange = 15f;

    private float searchTimer = 0f;

    //-------------------------------------
    [Header("ステージ範囲")]
    //四角形
    /* [SerializeField] private Vector3 stageMin; // ステージの最小座標
     [SerializeField] private Vector3 stageMax; // ステージの最大座標*/
    //円形
    [SerializeField] private Vector3 stageCenter; // ステージ中心
    [SerializeField] private float stageRadius = 20f; // ステージ半径
    //-------------------------------------

    [Header("当たり判定設定")]
    [SerializeField] private SphereCollider searchArea;
    [SerializeField] private float angle = 45f;

    [Header("エフェクト")]
    [SerializeField] private ParticleSystem run;
    [SerializeField] private ParticleSystem charge;
    [SerializeField] private ParticleSystem weak;
    [SerializeField] private ParticleSystem strong;

    //-----その他-----
    public List<GameObject> players = new List<GameObject>();  //Player達
    public List<GameObject> outPlayers = new List<GameObject>();  //場外Player達
    private float minDistance = Mathf.Infinity; //最短距離をだすための目安値

    public GameObject target;       //攻撃対象
    private float distance;          //攻撃対象との距離

    Reception1 reception;
    Animator animator;

    GameManager_M gm;
    GameObject ob;


    void Awake()
    {
        speed2 = speed * ChargeMoveSpeedRate;
        rotSpeed2 = rotSpeed * ChargeRotateSpeedRate;
        curentRecoveryTime = StrongRecoveryTime;

        run.Stop();
        charge.Stop();
        weak.Stop();
        strong.Stop();
    }

    public void SetCharge(float value)
    {
        t = value;
    }

    void Start()
    {
        ob = GameObject.Find("Timebox");
        gm = ob.GetComponent<GameManager_M>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        reception = GetComponent<Reception1>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager_M.Instance != null && !GameManager_M.Instance.IsGameStartedProperty) return; 
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            CollectPlayers();
            SearchTarget();
            searchTimer = 0f;
        }
        if (target == null) return;

        // ステージ外チェック
        if (IsOutOfStage(transform.position) || IsOutOfStage(target.transform.position))
        {
            ResetTarget();
            return;
        }

        distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > 5f)
        {
            Move();
        }

        if (distance < 15f /*&& distance > r*/)
        {
            if (IsOutOfStage(target.transform.position))
            {
                ResetTarget();
            }
            Atack(true);
        }

        if (distance < r)
        {
            Atack(false);
        }

        if (isStrt)
        {
            t += Time.deltaTime;

            if (t >= chargeMax)
            {
                t = chargeMax;
                isMax = true;
            }
        }
        else
        {
            t = 0f;
            isMax = false;
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
        float mag = rb.linearVelocity.magnitude;
        if (mag < 0.01f) { run.Stop(); }
        else if(mag > 0.01f)
        {
            run.Play();
        }
            animator.SetFloat("Speed", mag);
        animator.SetBool("IsChage", isStrt);
        animator.SetBool("isAttack1", isAttack1);
        animator.SetBool("isAttack2", isAttack2);
    }
    void Move()
    {
        if (isPrese)
        {
            curentSpeed = speed2;
            curentRotSpeed = rotSpeed2;
        }
        else
        {
            curentSpeed = speed;
            curentRotSpeed = rotSpeed;
        }

        if (!isTackling)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;

            Vector3 move = dir * curentSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);

            if (move != Vector3.zero)
            {
                Quaternion Rot = Quaternion.LookRotation(dir, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Rot, curentRotSpeed * Time.deltaTime));
            }
        }
    }

    void Atack(bool a)
    {
        if (a)
        {
            isfinish = false;

            if (!isTackling && Time.time > lastTackleTime + tackleCooldown)
            {
                if (!isStrt)
                {
                    r = Random.Range(5f, 10f);
                    isStrt = true;
                    charge.Play();
                }

                isPrese = true;
            }
        }
        if (!a)
        {
            isPrese = false;
            if (isStrt && !isTackling && Time.time > lastTackleTime + tackleCooldown)
            {
                charge.Stop();
                Tackle();
            }
            isStrt = false;
        }
    }
    void Tackle()
    {
        if (isfinish) { return; }
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

        if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath)
        {
            curentknockbackForce *= 10f;
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

        //ここで硬直処理
        if (isMax)
        {
            isfinish = true;
        }

        isMax = false;
        isAttack1 = false;
        isAttack2 = false;
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
                if (Physics.Raycast(this.transform.position + Vector3.up * 0.5f, posDir, out RaycastHit hit))
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

        // ===== ステージ範囲（追加） =====
        Handles.color = Color.green;

        Vector3 center = stageCenter;
        center.y = 0f; // XZ平面に固定

        // 円の外枠
        Handles.DrawWireDisc(center, Vector3.up, stageRadius);

        // 中心点
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 0.3f);
    }
#endif

    void CollectPlayers()
    {
        players.Clear();

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj == gameObject) continue; // 自分は除外
            if (IsOutOfStage(obj.transform.position))
            {
                // ステージ外のプレイヤーはoutPlayersに追加
                if (!outPlayers.Contains(obj))
                    outPlayers.Add(obj);
            }
            else
            {
                players.Add(obj);
            }
        }
    }

    void SearchTarget()
    {
        //if (isTackling || isStrt) return;

        target = null;
        minDistance = Mathf.Infinity;

        foreach (GameObject obj in players)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist > searchRange) continue;

            if (dist < minDistance)
            {
                minDistance = dist;
                target = obj;
            }
        }

        // targetがステージ外に出た場合はリセットしてoutPlayersに追加
        if (target != null && IsOutOfStage(target.transform.position))
        {
            if (!outPlayers.Contains(target))
                outPlayers.Add(target);
            target = null;
        }
    }
    //-------------------------------------

    bool IsOutOfStage(Vector3 pos)
    {
        /* // x,z がすべて範囲内かチェック
         if (pos.x < stageMin.x || pos.x > stageMax.x) return true;
         if (pos.z < stageMin.z || pos.z > stageMax.z) return true;

         return false; // 全部範囲内ならステージ内*/

        // Yは無視してXZ平面だけで判定
        //Vector3 centerXZ = new Vector3(stageCenter.x, 0f, stageCenter.z);
        Vector3 posXZ = new Vector3(pos.x, 0f, pos.z);

        float distance = Vector3.Distance(stageCenter, posXZ);

        return distance > stageRadius;
    }
    //-------------------------------------

    void ResetTarget()
    {
        target = null;

        isStrt = false;
        isPrese = false;
        isMax = false;
        t = 0f;
        r = 0f;

        isAttack1 = false;
        isAttack2 = false;

        CancelInvoke(nameof(EndTackle));
        isTackling = false;
    }


    public void ResetBotState()
    {
        // 1. 全ての入力・行動フラグを折る
        isPrese = false;
        isStrt = false;
        isTackling = false;
        isMax = false;
        isfinish = false;
        isAttack1 = false;
        isAttack2 = false;

        // 2. 数値パラメータのリセット
        t = 0f;
        r = 0f;
        searchTimer = 0f;
        lastTackleTime = 0f;
        curentRecoveryTime = StrongRecoveryTime;

        // 3. ターゲットをクリア
        target = null;
        players.Clear();
        outPlayers.Clear();

        // 4. 全エフェクトを強制停止
        if (run != null) run.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (charge != null) charge.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (weak != null) weak.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (strong != null) strong.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 5. Invoke (EndTackle) のキャンセル
        CancelInvoke(nameof(EndTackle));

        // 6. 物理のリセット
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 7. アニメーターのリセット
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChage", false);
            animator.SetBool("isAttack1", false);
            animator.SetBool("isAttack2", false);
        }
    }
}
