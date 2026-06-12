using System;
using UnityEngine;
using UnityEngine.InputSystem;

/*変更点あり*/
public enum MoveState
{
    Idle,Walk
}
public enum ActionState
{
    None,Charge, Attack
}
public enum AttackPower
{
    None,Weak,Strong
}
public enum State
{
    None,Knockback
}
//ステート管理クラス
public class PlayerStateManager : MonoBehaviour,Initalize
{
    //ステートのメソッド,代入はこのクラスのみ参照は別クラスでも可
    public MoveState MoveState {get; private set;} = MoveState.Idle;
    public ActionState ActionState {get; private set;} = ActionState.None;
    public AttackPower AttackPower { get; private set; } = AttackPower.None;
    public State State {get; private set;} = State.None;

    /* ---------
     
     
     
     ---------*/

    //イベント
    public event Action<MoveState> OnMoveStateChanged;
    public event Action<ActionState> OnActionStateChanged;
    public event Action<AttackPower> OnAttackPowerChanged;
    public event Action<State> OnStateChanged;

    public void Inited()
    {
        UpdateMoveState(Vector2.zero);

        SetActionState(ActionState.None);
        SetAttackPower(AttackPower.None);
        SetState(State.None);
    }

    public void UpdateMoveState(Vector2 inputVere)
    {
        //入力がされたらステートを変更(? = true,: = false) 
        MoveState newState = inputVere.sqrMagnitude > 0.01 ? MoveState.Walk : MoveState.Idle;

        if (MoveState == newState) return;
        MoveState = newState;
        OnMoveStateChanged?.Invoke(MoveState);
    }
    public void SetActionState(ActionState state)
    {
        if (ActionState == state) return;

        ActionState = state;
        OnActionStateChanged?.Invoke(ActionState);
    }
    public void SetAttackPower(AttackPower state)
    {
        if (AttackPower == state) return;

        AttackPower = state;
        OnAttackPowerChanged?.Invoke(AttackPower);
    }
    public void SetState(State state)
    {
        if (State == state) return;

        State = state;
        OnStateChanged?.Invoke(State);
    }

/*    private void Update()
    {
        Debug.Log($"MoveState: {MoveState}, ActionState: {ActionState}");
    }*/
}
