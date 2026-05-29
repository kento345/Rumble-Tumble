using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class RenderColor : MonoBehaviour
{
    private Renderer render;
    private PlayerInput input;
    void Start()
    {
        render = GetComponent<Renderer>();
        input = transform.parent.GetComponent<PlayerInput>();

        render.material.color = Color.black;
        if (input != null)
        {
            if (input.playerIndex == 0)
            {
              render.material.color = Color.red;
            }
            else if (input.playerIndex == 1)
            {
                render.material.color = Color.blue;
            }
            else if (input.playerIndex == 2)
            {
                render.material.color = Color.green;
            }
            else if (input.playerIndex == 3)
            {
                render.material.color = Color.yellow;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
