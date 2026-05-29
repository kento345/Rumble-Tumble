using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    private float moveSpeed = 1000f;
    private Vector2 inputVer;
    private RectTransform trans;

    private GameObject obj;
    private GraphicRaycaster raycaster;
    private PointerEventData eventData;
    private EventSystem eventSystem;

    public InputDevice controlDevice;

    private void Awake()
    {
        trans = GetComponent<RectTransform>();

        obj = GameObject.Find("Canvas");
        raycaster = obj.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.control.device != controlDevice) return;

        inputVer = context.ReadValue<Vector2>();
    }

    public void OnEnter(InputAction.CallbackContext context)
    {
        if (context.control.device != controlDevice) return;
        if (!context.performed) {return; }

        eventData = new PointerEventData(eventSystem);
        eventData.position = trans.position;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                break;
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        Move();
    }

    void Move()
    {
        Vector2 move = inputVer * moveSpeed * Time.deltaTime;

        trans.anchoredPosition += move;
    }
}
