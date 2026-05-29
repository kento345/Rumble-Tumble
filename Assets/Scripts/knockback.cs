using UnityEngine;

public class knockback : MonoBehaviour
{
    [Header("吹き飛ばし設定")]
    [SerializeField] private float kkForce = 50f;
    [SerializeField] private float minAngle = -45f;
    [SerializeField] private float maxAngle = 45f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KnockBack(collision.gameObject);
        }
    }

    private void KnockBack(GameObject player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();

        float rndAngle = Random.Range(minAngle, maxAngle);

        Quaternion rotation = Quaternion.Euler(rndAngle, 0, rndAngle);
        Vector3 kkDirection = rotation * Vector3.up;

        rb.AddForce(kkDirection * kkForce, ForceMode.Impulse);
    }
}
