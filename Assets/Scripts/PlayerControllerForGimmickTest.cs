using UnityEngine;
using UnityEngine.InputSystem;

namespace PCFGT
{
    public class PlayerControllerForGimmickTest : MonoBehaviour
    {
        [SerializeField] private float _moveSpd = 5.0f;

        private Rigidbody rb;
        private Vector2 _moveInput;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        void FixedUpdate()
        {
            var moveDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            rb.MovePosition(rb.position + moveDir * _moveSpd * Time.fixedDeltaTime);
        }
    }

}
