using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class AnimatorController : MonoBehaviour
{
    [HideInInspector] public bool isStart;
    [HideInInspector] public bool isAttack1;
    [HideInInspector] public bool isAttack2;
    [HideInInspector] public bool isHit;
    private PlayerInputController inputCon;
    private BOTController bot;

    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputCon = GetComponent<PlayerInputController>();
        bot = GetComponent<BOTController>();

        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float mag = 0;
        if (inputCon != null)
        {
            mag = inputCon.InputVeer.magnitude;
            
        }
        else if (bot != null)
        {
            mag = bot.MoveInput.magnitude;
        }
        animator.SetFloat("Speed", mag);
        animator.SetBool("IsChage", isStart);
        animator.SetBool("IsAttack1", isAttack1);
        animator.SetBool("IsAttack2", isAttack2);
        animator.SetBool("IsHit", isHit);
    }
}
