using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

public class Reception : MonoBehaviour,Initalize
{
    /*変更点あり*/
    [Header("ノックバック,無敵設定")]
    private float knockbackTime = 0.3f;
    private float knockbackCounter;

    private Vector3 knockbackDir;

    [HideInInspector] public float smallKnockback = 1;

    [SerializeField] private ParticleSystem hit;
    [SerializeField] private ParticleSystem knock;

    [SerializeField] private float StunInvincibleTime = 1.0f; //無敵時間
    bool isKonckback = false;
    private bool isHit = false;
    Collider col;
    Rigidbody rb;

    //-----Script参照-----
    private PlayerStateManager stateManager;
    private ChargeSpike cs;
    private AnimatorController animeCon;

    private AtackController ac;
    private PlayerInputController playerCon;
    private BOTController botCon;

    public void Inited()
    {
        StopAllCoroutines();

        hit.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        knock.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        knockbackCounter = 0;
        knockbackDir = Vector3.zero;

        isHit = false;
        isKonckback = false;

        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;

            rb.useGravity = true;
        }
        if(col != null)
        col.enabled = true;

        if (animeCon != null)
        {
            animeCon.isHit = false;
        }
    }
    private void Start()
    {
        hit.Stop();
        knock.Stop();

        rb = GetComponent<Rigidbody>();
        //animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        stateManager = GetComponent<PlayerStateManager>();
        cs = GetComponent<ChargeSpike>();
        animeCon = GetComponent<AnimatorController>();
        ac = GetComponent<AtackController>();
        playerCon = GetComponent<PlayerInputController>();
        botCon = GetComponent<BOTController>();
    }

    private void Update()
    {
        if (isKonckback)
        {
            knockbackCounter -= Time.deltaTime;
            if (knockbackCounter <= 0)
            {
                isKonckback = false;
                //stateManager.SetState(State.None);
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
    private void FixedUpdate()
    {
        if(isKonckback)
        {
            rb.linearVelocity = knockbackDir;
        }
    }

    public void KnockBack(Vector3 pos,float force)
    {
        if (isHit) return;
        animeCon.isHit = true;
        isKonckback = true;
        if (ac != null)
        {
            ac.SetCharge(0);
        }
        if(stateManager != null)
        {
            stateManager.SetActionState(ActionState.None);
        }
        knockbackCounter = knockbackTime;
        knockbackDir = pos.normalized * force * smallKnockback;
        rb.linearVelocity = Vector3.zero;

        if(botCon != null)
        {
            botCon.OnMove(Vector2.zero);
        }

        StartCoroutine(Hit());
    }

    IEnumerator Hit()
    {
        isHit = true;
        stateManager.SetState(State.Knockback);
        if(hit && !hit.isPlaying)
        hit.Play();
        yield return new WaitForSeconds(0.05f);
     
        if(hit &&  hit.isPlaying)
        hit.Stop();
        col.enabled = false;
        rb.useGravity = false;
        knock.Play();
        
        yield return new WaitForSeconds(StunInvincibleTime);

        rb.useGravity = true;
        col.enabled = true;
        knock.Stop();
        stateManager.SetState(State.None);
        isHit = false;
        if (animeCon.isHit)
            animeCon.isHit = false;
    }
}
