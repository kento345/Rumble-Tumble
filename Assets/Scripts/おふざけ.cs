using UnityEngine;

public class おふざけ : MonoBehaviour
{
    public GameObject button;
    public GameObject message;
    public GameObject img;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Active()
    {
        button.SetActive(false);
        img.SetActive(false);
        message.SetActive(true);
    }
}
