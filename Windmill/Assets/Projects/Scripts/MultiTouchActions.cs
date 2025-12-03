using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiTouchActions : MonoBehaviour
{

    public static MultiTouchActions Instance { get; private set; }

    private InputSystem_Actions input;

    public Action<Vector2> OnTouchPress;
    public Action<Vector2> OnTouchRelease;
    public Action<Vector2, int> OnMultiTouchPress; // position, touchId
    public Action<Vector2, int> OnMultiTouchRelease;
    public Action<Vector2, Vector2, int> OnTouchMove; // position, delta, touchId
    public Action<Vector2> OnMouseMove; // for editor testing

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("MultiTouchActions: gameObject is null during Awake. Cannot destroy.");
            }
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (input != null && !Equals(input.Gameplay, null))
            input.Gameplay.Enable();
    }

    void OnDisable()
    {
        if (input != null && !Equals(input.Gameplay, null))
            input.Gameplay.Disable();
    }

    void Update()
    {
        if (input == null || !Equals(input.Gameplay, null) || !input.Gameplay.enabled)
            return;

        // Multi-touch
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch == null)
                    continue;
                try
                {
                    var press = touch.press;
                    if (press != null && press.isPressed)
                    {
                        Vector2 pos = touch.position.ReadValue();
                        int id = 0;
                        try { id = touch.touchId.ReadValue(); } catch { id = 0; }
                        OnMultiTouchPress?.Invoke(pos, id);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("MultiTouchActions: touch read error - " + ex.Message);
                }
            }
        }

        // Single touch / mouse
        try
        {
            var touchPressAction = input.Gameplay.TouchPress;
            var touchPosAction = input.Gameplay.TouchPosition;
            if (touchPressAction != null && touchPosAction != null)
            {
                float pressVal = 0f;
                try { pressVal = touchPressAction.ReadValue<float>(); } catch { pressVal = 0f; }

                Vector2 pos = Vector2.zero;
                try { pos = touchPosAction.ReadValue<Vector2>(); } catch { pos = Vector2.zero; }

                if (pressVal > 0f)
                    OnTouchPress?.Invoke(pos);
                else
                    OnTouchRelease?.Invoke(pos);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("MultiTouchActions: single touch read error - " + ex.Message);
        }
    }

}
