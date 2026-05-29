using UnityEngine;

public class Ruin : MonoBehaviour
{
    private int con = 0;

    public int durability = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (durability == con)
        {
            Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Meteor"))
        {
            con++;
        }
    }
}
