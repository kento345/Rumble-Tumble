using UnityEngine;

public class Durability : MonoBehaviour
{
    [SerializeField]
    private int durability;

    private int count = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((durability - count) == 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Meteor"))
        {
            count++;
            Debug.Log(this.gameObject.name + " の耐久値の減少 : " + (durability - count) + " / " + durability);

        }
    }
}
