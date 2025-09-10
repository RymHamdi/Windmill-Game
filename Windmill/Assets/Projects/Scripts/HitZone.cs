using System;
using UnityEngine;

public class HitZone : MonoBehaviour
{
    public KeyCode key; // assign per lane (A, S, D, F)

    public GameObject goodEffect;
    public GameObject missEffect;

    void OnEnable()
    {
        MultiTouchActions.Instance.OnTouchPress += OnTouchPressed;
        MultiTouchActions.Instance.OnMultiTouchPress += OnMultiTouchPressed;
    }

    private void OnMultiTouchPressed(Vector2 vector, int arg2)
    {
        Ray ray = Camera.main.ScreenPointToRay(vector);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.collider == GetComponent<Collider>())
            {
                CheckHit();
            }
        }
    }

    private void OnTouchPressed(Vector2 vector)
    {
        Ray ray = Camera.main.ScreenPointToRay(vector);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.collider == GetComponent<Collider>())
            {
                CheckHit();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            CheckHit();
        }
    }

    void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1);
        foreach (var hit in hits)
        {
            Note note = hit.GetComponent<Note>();
            if (note != null)
            {
                Debug.Log("Hit " + key);
                if (goodEffect != null)
                    goodEffect.SetActive(true);
                Invoke("DisableGoodEffect", 0.2f);


                note.Hit();
                return;
            }
        }

        Debug.Log("Miss " + key);
        if (missEffect != null)
            missEffect.SetActive(true);
        Invoke("DisableMissEffect", 0.2f);
    }

    private void DisableGoodEffect()
    {
        if (goodEffect != null)
            goodEffect.SetActive(false);
    }

    private void DisableMissEffect()
    {
        if (missEffect != null)
            missEffect.SetActive(false);
    }

    void OnDisable()
    {
        MultiTouchActions.Instance.OnTouchPress -= OnTouchPressed;
        MultiTouchActions.Instance.OnMultiTouchPress -= OnMultiTouchPressed;
    }
}
