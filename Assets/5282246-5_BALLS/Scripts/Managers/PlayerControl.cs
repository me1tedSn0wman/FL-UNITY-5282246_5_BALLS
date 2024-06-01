using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : Singleton<PlayerControl>
{
    public Vector2 mousePos;
    public float mousePress;

    public bool isDrag = false;

    public Vector2 pressStartPos = Vector2.zero;
    public float dragMagnitude;

    public float timeStartPressed = -1;
    public float timeDragDuration = 2f;

    public event Action OnPointerPressed;

    [SerializeField] private GameObject canvasGO_GameplayArea; 
    private RectTransform gameplayAreaRectTransf;


    public static bool IS_DRAG
    {
        get { return Instance.isDrag; }
    }

    public override void Awake()
    {
        base.Awake();
        gameplayAreaRectTransf = canvasGO_GameplayArea.GetComponent<RectTransform>();
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    public void OnMousePress(InputAction.CallbackContext context)
    {
        mousePress = context.ReadValue<float>();

        switch (context.phase) {
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Canceled:
                if (MouseInGameplayArea())
                {
                    OnPointerPressed();
                }
                break;
        }

        if (mousePress == 1)
        {
            StartCheckDrag();
        }
        else
        {
            StopCheckDrag();
        }
    }

    private void Update()
    {
        UpdateCheckDrag();
    }

    private void UpdateCheckDrag()
    {
        if (isDrag || timeStartPressed == -1 && pressStartPos == Vector2.zero) return;
        if (
            Time.time > timeStartPressed + timeDragDuration
            || (mousePos - pressStartPos).magnitude >= dragMagnitude
            )
        {
            isDrag = true;
        }
    }

    private void StartCheckDrag()
    {
        timeStartPressed = Time.time;
        pressStartPos = mousePos;
    }

    private void StopCheckDrag()
    {
        timeStartPressed = -1;
        pressStartPos = Vector2.zero;
        isDrag = false;
    }

    public Vector3 GetMouseWorldPos() {
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    public bool MouseInGameplayArea() {
        Vector3[] v = new Vector3[4];
        gameplayAreaRectTransf.GetWorldCorners(v);
        Rect rect = new Rect(v[1].x, v[3].y, v[3].x - v[1].x, v[1].y - v[3].y);
//        Debug.Log(v[1].x + "___"+ v[1].y +"___" +v[3].x + "___" +v[3].y + "___" + mousePos.x+ "___" + mousePos.y);

        if (rect.Contains(mousePos)) {
//            Debug.Log("hit In Area");
            return true;
        }
        return false;
    }
}
