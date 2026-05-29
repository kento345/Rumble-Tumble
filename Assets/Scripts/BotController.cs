using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float Speed = 3f;
    //[SerializeField] private float rotate = 120f;

    [Header("行動詳細")]
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;

    [SerializeField] private float minWalkTime = 2f;
    [SerializeField] private float maxWalkTime = 5f;

    [Header("センサー")]
    [SerializeField] private Transform Sensor;

    [SerializeField] private float sensorRadius = 0.2f;
    [SerializeField] private float sensorDistance = 0.5f;

    [SerializeField] private LayerMask StageLayer;

    private bool isMove = true;
    private float actTimer = 0f;
    private float nextActDuration = 0f;


    private Reception1 reception;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        reception = GetComponent<Reception1>();
        animator = GetComponentInChildren<Animator>();
        SetNextAct();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager_M.Instance != null && !GameManager_M.Instance.IsGameStartedProperty)return; // 移動処理などをすべてスキップ
        bool isGround = Check(Sensor);

        if (!isGround)
        {
            Turn();
            return;
        }

        actTimer += Time.deltaTime;

        if (actTimer >= nextActDuration)
        {
            isMove = !isMove;
            actTimer = 0f;
            SetNextAct();
        }

        if (isMove)
        {
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }
      
        float mag = Speed;
        animator.SetFloat("Speed", isMove ? mag : 0f);
    }


    private bool Check(Transform sensor)
    {
        if (sensor == null)
        {
            return false;
        }

        RaycastHit hit;

        if (Physics.SphereCast(sensor.position,sensorRadius,Vector3.down,out hit,sensorDistance,StageLayer))
        {
            return hit.collider.CompareTag("Stage");
        }

        return false;
    }

    private void Turn()
    {
        float rndAngle = Random.Range(90f, 180f);

        if (Random.value > 0.5f)
        {
            rndAngle = -rndAngle;
        }

        transform.Rotate(0, rndAngle, 0);
    }

    private void SetNextAct()
    {
        if(reception.isHit) {return;}
        if (isMove)
        {
            nextActDuration = Random.Range(minWalkTime, maxWalkTime);
        }
        else
        {
            nextActDuration = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (Sensor == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Sensor.position + Vector3.down * sensorDistance, sensorRadius);
    }
}
