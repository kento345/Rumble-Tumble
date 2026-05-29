using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class BOTController : MonoBehaviour
{
    [SerializeField] private GameObject point;
    GameObject pointTarget;
    GameObject playerTarget;
    GameObject near = null;
    float minDist;

    [Header("ターゲット選択")]
    [SerializeField] private float minHoldTime = 2f;   // ターゲット保持の最短時間（秒）
    [SerializeField] private float maxHoldTime = 4f;   // ターゲット保持の最長時間（秒）
    [SerializeField] private float penaltyRate = 0.15f; // 同じ相手を狙い続けると加算されるペナルティ/秒
    private float targetHoldTimer = 0f;
    private float fairnessPenalty = 0f;


/*    //Player操作反転
    public int playerID;
    private bool isInverted = false;*/

    Vector2 moveInput;
    public Vector2 MoveInput => moveInput;


    private float attackPrepareTime = 1f;
    bool preparingAttack = false;
    float prepareCounter = 0f;

    Quaternion targetRotation;
    bool isRota = false;

    bool attackRest = false;
    private float restTime = 3f;

    private PlayerStateManager stateManager;
    private MoveController move;
    private AtackController atack;
    private Seencer sencer;

    Rigidbody rb;



    void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();
        move = GetComponent<MoveController>();
        atack = GetComponent<AtackController>();
        sencer = GetComponentInChildren<Seencer>();
        rb = GetComponent<Rigidbody>();

        CreatePoint();
    }

    void Update()
    {
        if(stateManager.State == State.Knockback) { return; }
        //false
        if (!sencer.CheckLayer())
        {
            StopAndCreate();
            if (!isRota)
            {
                targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
                isRota = true;
            }

            Rota();
            return;
        }
        Serch();

        if (!attackRest && near != null && minDist < 10f)
        {
            playerTarget = near;
        }

        if (pointTarget == null)
        {
            CreatePoint();
        }
        if(playerTarget != null)
        {
            Attack();
        }

        MoveToPoint();

        if(pointTarget != null)
        {
            float dist = Vector3.Distance(transform.position, pointTarget.transform.position);
            if (dist < 1f)
            {
                Destroy(pointTarget);
                CreatePoint();
            }
        }
      
        if(!attackRest && playerTarget != null && stateManager.ActionState == ActionState.Attack)
        {
            StartCoroutine(RestTime());
        }
        if (stateManager.ActionState == ActionState.Attack)
        {
            preparingAttack = false;
        }
    }

    void MoveToPoint()
    {
        GameObject target = null;

        if(playerTarget != null)
        {
            target = playerTarget;
            Destroy(pointTarget);
        }
        else if(pointTarget != null)
        {
            target = pointTarget;
        }

        if (target == null) return;

        Vector3 dir = target.transform.position - transform.position;
        moveInput = new Vector2(dir.normalized.x, dir.normalized.z);
       
        /*if (isInverted)
        {
            //操作反転
            moveInput *= -1;
        }*/
        OnMove(moveInput);
    }

    void CreatePoint()
    {
        Vector3 pos = transform.position;
        Vector2 random = Random.insideUnitCircle * 10f;
        Vector3 pointPos = new Vector3(pos.x + random.x, pos.y, pos.z + random.y);

        GameObject p = Instantiate(point, pointPos, Quaternion.identity);

        pointTarget = p;
    }
    
    void StopAndCreate()
    {
        OnMove(Vector2.zero);

        if(pointTarget != null)
        {
            Destroy(pointTarget);
        }
        CreatePoint();
    }

    void Rota()
    {
        // Slerpで滑らかに回転
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 5 * Time.deltaTime);

        // ほぼ目標向きになったら停止
        if (Quaternion.Angle(rb.rotation, targetRotation) < 0.5f)
        {
            rb.rotation = targetRotation;
            isRota = false;
        }
    }

    void Attack()
    {
        if (playerTarget == null) return;

        Vector3 dir = playerTarget.transform.position - transform.position;
        dir.y = 0;
        transform.forward = dir.normalized;
        float dist = dir.magnitude;
        if (!preparingAttack && dist < 6)
        {
            preparingAttack = true;
            prepareCounter = attackPrepareTime;
            OnMove(Vector2.zero); //止まる
        }
        // 溜め時間
        if (preparingAttack)
        {
            prepareCounter -= Time.deltaTime;

            // チャージ開始
            if (stateManager.ActionState == ActionState.None)
            {
                atack.Shot(0);
            }

            // 溜め完了
            if (prepareCounter <= 0f)
            {
                preparingAttack = false;

                if (stateManager.ActionState == ActionState.Charge)
                {
                    atack.Shot(1);
                }
            }
        }
    }

    void Serch()
    {
        // 保持タイマー中は現ターゲットをそのまま使う
        if (targetHoldTimer > 0f && near != null)
        {
            targetHoldTimer -= Time.deltaTime;
            fairnessPenalty = Mathf.Min(fairnessPenalty + penaltyRate * Time.deltaTime, 1f);
            minDist = Vector3.Distance(transform.position, near.transform.position);
            return;
        }

        // 候補リストを作成（自分以外の全プレイヤー）
        var candidates = new List<GameObject>();
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p != gameObject && p != null)
                candidates.Add(p);
        }

        if (candidates.Count == 0) { near = null; minDist = Mathf.Infinity; return; }

        // 距離の逆数で重み付け。現ターゲットは公平ペナルティを引く
        float totalWeight = 0f;
        float[] weights = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; i++)
        {
            float d = Vector3.Distance(transform.position, candidates[i].transform.position);
            float w = 1f / Mathf.Max(d, 0.1f);
            if (candidates[i] == near) w = Mathf.Max(0.01f, w - fairnessPenalty);
            weights[i] = w;
            totalWeight += w;
        }

        // 重み付きランダムで選択
        float rnd = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        GameObject newTarget = null;
        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (rnd <= cumulative) { newTarget = candidates[i]; break; }
        }

        // ターゲットが変わったらペナルティをリセット
        if (newTarget != near) fairnessPenalty = 0f;
        near = newTarget;
        minDist = near != null ? Vector3.Distance(transform.position, near.transform.position) : Mathf.Infinity;
        targetHoldTimer = Random.Range(minHoldTime, maxHoldTime);
    }

    public void OnMove(Vector2 context)
    {
        stateManager.UpdateMoveState(context);
        move.SetMoveInput(context);
    }
 /*   public void SetReverse(bool value)
    {
        isInverted = value;
    }*/

    IEnumerator RestTime()
    {
        attackRest = true;

        playerTarget = null;
        targetHoldTimer = 0f; // 攻撃後は必ず再評価（ペナルティが積んでいるので自然に分散する）
        CreatePoint();
        yield return new WaitForSeconds(restTime);

        attackRest = false;
    }
}
