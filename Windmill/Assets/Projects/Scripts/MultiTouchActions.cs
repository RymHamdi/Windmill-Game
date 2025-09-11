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
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        input.Gameplay.Enable();
    }

    void OnDisable()
    {
        input.Gameplay.Disable();
    }

    void Update()
    {
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                {
                    Vector2 pos = touch.position.ReadValue();
                    OnMultiTouchPress?.Invoke(pos, touch.touchId.ReadValue());
                }
            }
        }
        // Works with mouse or single touch
        if (input.Gameplay.TouchPress.ReadValue<float>() > 0)
        {
            Vector2 pos = input.Gameplay.TouchPosition.ReadValue<Vector2>();
            OnTouchPress?.Invoke(pos);

        }

        // For multiple touches (needs direct Touchscreen access)
        
    }
}
