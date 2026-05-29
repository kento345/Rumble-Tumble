using UnityEngine;
using UnityEngine.Audio;

public class Landmine : MonoBehaviour
{

    public float explosionForce = 10.0f;
    public float explosionRadius = 3.0f;

    public GameObject Effect;
    public GameObject Model;

    [SerializeField]
    private AudioSource SE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Effect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(Model);
            Debug.Log("ãNîöÅI");
            Explosion();


        }
    }

        void Explosion()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach(Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    SE.PlayOneShot(SE.clip);

                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(Vector3.up * explosionForce, ForceMode.VelocityChange);
                }
            }
        }

        Effect.SetActive(true);

        Destroy(gameObject, 5f);
    }
}
