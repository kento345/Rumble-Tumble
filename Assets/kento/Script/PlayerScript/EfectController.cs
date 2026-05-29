using UnityEngine;

public class EfectController : MonoBehaviour
{
    [Header("エフェクト設定")]
    [SerializeField] private ParticleSystem run;//走り
    [SerializeField] private ParticleSystem chage;//チャージ
    [SerializeField] private ParticleSystem strong;//強
    [SerializeField] private ParticleSystem weak;//弱

    private PlayerStateManager stateManager;

    private void OnEnable()
    {
        stateManager = GetComponent<PlayerStateManager>();

        stateManager.OnMoveStateChanged += MoveEffect;
        stateManager.OnActionStateChanged += ChargeEffect;
        stateManager.OnAttackPowerChanged += AttackEffect;

        run.Stop();
        chage.Stop();
        strong.Stop();
        weak.Stop();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();

        stateManager.OnMoveStateChanged += MoveEffect;
        stateManager.OnActionStateChanged += ChargeEffect;
        stateManager.OnAttackPowerChanged += AttackEffect;

        run.Stop();
        chage.Stop();
        strong.Stop();
        weak.Stop();
    }

    void MoveEffect(MoveState state)
    {
        if(state == MoveState.Walk)
            run?.Play();
        else
            run?.Stop();
    }
    void ChargeEffect(ActionState state) 
    {
       if(state == ActionState.Charge)
            chage?.Play();
       else
            chage?.Stop();
    }
    void AttackEffect(AttackPower state)
    {
        weak?.Stop();
        strong?.Stop();

        if(state == AttackPower.Weak)
            weak?.Play();
        else if(state == AttackPower.Strong)
            strong?.Play();
    }
}
