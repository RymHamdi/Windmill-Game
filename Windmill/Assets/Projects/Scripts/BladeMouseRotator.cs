using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BladeMouseRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    public bool useCanonicalAngle = true; // Toggle to see raw vs canonical

    private Vector2 previousMousePosition;
    private Vector2 lastTransformPosition;
    bool isDragging = false;
    bool canRotate = false;

    public Button validateButton;
    public Color canRotateColor = Color.green;
    public Color cannotRotateColor = Color.red;

    public Color normalButtonColor = Color.white;

    private bool canInteract;
    public Transform clothes;



    void Start()
    {
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                return;
            }
        }
        MultiTouchActions.Instance.OnTouchPress += GetMousePosition;
        MultiTouchActions.Instance.OnTouchRelease += OnRelease;
        canInteract = false;
    }

    public void GetMousePosition(Vector2 pos)
    {
        if (!canRotate)
        {
            return;
        }
        if (!isDragging)
        {
            previousMousePosition = pos;
            isDragging = true;
            return;
        }

        Vector2 mouseDelta = pos - previousMousePosition;

        // Only rotate if the mouse has moved enough
        if (mouseDelta.magnitude < 0.1f)
        {
            previousMousePosition = pos;
            return;
        }

        // Calculate current direction from blade to mouse
        Vector2 bladeToMouse = (pos - (Vector2)transform.position).normalized;

        // Calculate previous direction from blade to previous mouse position
        Vector2 prevBladeToMouse = (previousMousePosition - (Vector2)transform.position).normalized;

        // Calculate angle difference between previous and current direction
        float angleDelta = Vector2.SignedAngle(prevBladeToMouse, bladeToMouse);

        // Apply rotation only by the angle delta, not absolute angle
        transform.Rotate(0, 0, angleDelta * rotationSpeed);

        previousMousePosition = pos;
    }

    public void OnRelease(Vector2 pos)
    {
        isDragging = false;
        float canonicalAngle = SecretLanguageManager.Instance.GetCanonicalBladeAngle(transform.eulerAngles.z);
        float closestAngle = SecretLanguageManager.Instance.GetClosestCanonicalAngle(canonicalAngle);
        if (useCanonicalAngle)
        {
            // Snap to closest canonical angle
            transform.rotation = Quaternion.Euler(0, 0, closestAngle);
        }
    }

    void Update()
    {
        // HandleMouseRotation();
        //DebugRotationValues();
        clothes.transform.rotation = transform.rotation;
    }



    void DebugRotationValues()
    {
        // Get current rotation (normalized to 0-360)
        //float currentRotation = NormalizeAngle(transform.eulerAngles.z);
        float canonicalAngle = SecretLanguageManager.Instance.GetCanonicalBladeAngle(transform.eulerAngles.z);

        // Debug output
        if (useCanonicalAngle)
        {
            Debug.Log($"Canonical Angle: {canonicalAngle:F1}°");
        }
        else
        {
            //Debug.Log($"Raw Rotation: {currentRotation:F1}° → Canonical: {canonicalAngle:F1}°");
        }
    }

    public void EnableRotation()
    {
        canRotate = true;
        validateButton.image.color = canRotateColor;
        canInteract = true;
    }

    public void DisableRotation()
    {
        OnRelease(Vector2.zero);
        validateButton.image.color = normalButtonColor;
        canRotate = false;
        isDragging = false;
        canInteract = false;
    }

    public float NormalizeAngle(float angle)
    {
        // Normalize any angle to 0-360 range
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    // Optional: Reset rotation with key press for testing
    public void ResetRotation()
    {
        transform.localEulerAngles = new Vector3(0, 0, 5);
        Debug.Log("Rotation reset to 0°");
    }

    public void ValidateRotation()
    {
        if (!canInteract) return;
        DisableRotation();
        float canonicalAngle = SecretLanguageManager.Instance.GetCanonicalBladeAngle(transform.eulerAngles.z);
        SecretLanguageManager.Instance.ValidateBlade(canonicalAngle);
    }

    public float GetCurrentRotation()
    {
        float canonicalAngle = SecretLanguageManager.Instance.GetCanonicalBladeAngle(transform.eulerAngles.z);
        return canonicalAngle;
    }

    void OnDisable()
    {
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                return;
            }
        }
        if (MultiTouchActions.Instance != null)
        {
            MultiTouchActions.Instance.OnTouchPress -= GetMousePosition;
            MultiTouchActions.Instance.OnTouchRelease -= OnRelease;
        }
    }
}
