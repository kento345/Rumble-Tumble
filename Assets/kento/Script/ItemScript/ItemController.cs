using UnityEngine;



public class ItemController : MonoBehaviour
{
    public Item item;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody> ();
        rb.mass = 0.1f;
    }

    

    private void Update()
    {
        if (transform.position.y <= 1)
        {
           
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerItemEffect player = other.GetComponent<PlayerItemEffect>();

            if (player != null && !player.isEffectActive)
            {
                player.ApplyItem(item);
                Destroy(gameObject);
            }
        }
        if (other.CompareTag("Stage")){
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;

            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        }
    }
}
