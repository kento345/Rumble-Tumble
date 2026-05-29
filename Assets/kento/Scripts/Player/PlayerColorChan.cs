using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColorChan : MonoBehaviour
{
    [SerializeField] private PlayerInput input;
    [SerializeField] private Texture2D red;
    [SerializeField] private Texture2D blue;
    [SerializeField] private Texture2D green;
    [SerializeField] private Texture2D yellow;

    private Material material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        material = GetComponent<Material>();
        
        if(input.playerIndex == 0)
        {
            material.SetTexture("_BaseMap", red);
        }
        if (input.playerIndex == 1)
        {
            material.SetTexture("_BaseMap", blue);
        }
        if(input.playerIndex == 2)
        {
            material.SetTexture("_BaseMap", green);
        }
        if(input.playerIndex == 3)
        {
            material.SetTexture("_BaseMap", yellow);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
