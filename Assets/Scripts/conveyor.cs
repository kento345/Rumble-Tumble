using System.Collections.Generic;
using UnityEngine;

namespace ConveyorBelt
{
    public class conveyor : MonoBehaviour
    {
        [Header("コンベアの設定")]
        public float TargetDriveSpeed = 3.0f;
        public float CurrentSpeed { get { return _currentSpeed; } }
        public Vector3 DriveDirection = Vector3.forward;

        [SerializeField] private float _forcePower = 50f;

        private float _currentSpeed = 0;
        private List<Rigidbody> _rbs = new List<Rigidbody>();

        void Start()
        {
            DriveDirection = DriveDirection.normalized;
        }

        void FixedUpdate()
        {
            foreach (var r in _rbs)
            {
                var objSpeed = Vector3.Dot(r.linearVelocity, DriveDirection);

                if (objSpeed < TargetDriveSpeed)
                {
                    r.AddForce(DriveDirection * _forcePower, ForceMode.Acceleration);
                }
            }
        }

        // コンベアに乗ったオブジェクトを登録
        void OnCollisionEnter(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb != null && !_rbs.Contains(rb))
            {
                _rbs.Add(rb);
            }
        }

        // コンベアから離れたオブジェクトを削除
        void OnCollisionExit(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb != null)
            {
                _rbs.Remove(rb);
            }
        }
    }
}