using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;

public class CloroChange : MonoBehaviour
{
    [SerializeField]private PlayerInput input;
    [SerializeField] private Material red;
    [SerializeField] private Material blue;
    [SerializeField] private Material green;
    [SerializeField] private Material yellow;

    private SkinnedMeshRenderer render;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        render =GetComponent<SkinnedMeshRenderer>();
        if (input.playerIndex == 0)
        {
            render.material = red;
        }
        if (input.playerIndex == 1)
        {
            render.material = blue;
        }
        if (input.playerIndex == 2)
        {
            render.material = green;
        }
        if (input.playerIndex == 3)
        {
            render.material = yellow;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
