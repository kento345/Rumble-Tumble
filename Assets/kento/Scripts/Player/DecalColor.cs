using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class DecalColor : MonoBehaviour
{
    private DecalProjector projector;
    [SerializeField] private Material red;
    [SerializeField] private Material bule;
    [SerializeField] private Material yello;
    [SerializeField] private Material green;
    [SerializeField] private Material black;

    private PlayerInput input;

    void Start()
    {
        projector = GetComponent<DecalProjector>();

        input = transform.parent.GetComponent<PlayerInput>();

        projector.material = black;
        if (input != null)
        {
            if (input.playerIndex == 0)
            {
                projector.material = red;
            }
            else if (input.playerIndex == 1)
            {
                projector.material = bule;
            }
            else if (input.playerIndex == 2)
            {
                projector.material = green;
            }
            else if (input.playerIndex == 3)
            {
                projector.material = yello;
            }
        }

    }
}
