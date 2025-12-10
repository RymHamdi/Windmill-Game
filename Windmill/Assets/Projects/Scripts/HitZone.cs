using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HitZone : MonoBehaviour
{
    public KeyCode key; // assign per lane (A, S, D, F)

    public GameObject goodEffect;
    public GameObject missEffect;
    private Vector3 originScale;

    public Image keyImage;


    void OnEnable()
    {

    }

    void Start()
    {
        //MultiTouchActions.Instance.OnTouchPress += OnTouchPressed;
        //MultiTouchActions.Instance.OnMultiTouchPress += OnMultiTouchPressed;
        originScale = keyImage.transform.localScale;
    }

    private void OnMultiTouchPressed(Vector2 vector, int arg2)
    {
        Ray ray = Camera.main.ScreenPointToRay(vector);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.collider == GetComponent<Collider>())
            {
                //CheckHit();
            }

        }
    }

    private void OnTouchPressed(Vector2 vector)
    {
        Ray ray = Camera.main.ScreenPointToRay(vector);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, ~0, QueryTriggerInteraction.Collide))
        {
            // Only accept trigger colliders
            if (hitInfo.collider.isTrigger && hitInfo.collider.gameObject == gameObject)
            {
                //CheckHit();
            }
            else
            {
                Debug.Log($"Hit non-trigger: {hitInfo.collider.name}");
            }
        }
    }

    private bool _isPressing;
    private bool _pressStarted;
    private bool _hasHitDuringPress;

    private void Update()
    {
        if (_isPressing)
        {
            if (!_pressStarted)
            {
                _pressStarted = true;
                _hasHitDuringPress = false; // reset
                OnPressStart();
            }

            // continuous hit detection WHILE holding
            ContinuousHitCheck();
        }
        else
        {
            if (_pressStarted)
            {
                _pressStarted = false;
                OnPressEnd();
            }
        }
    }

    public void CheckHit(bool isPressing)
    {
        _isPressing = isPressing;
    }

    private void OnPressStart()
    {
        // scale only once
        ScaleUpAndDown();
    }

    private void OnPressEnd()
    {
        if (!_hasHitDuringPress)
        {
            Debug.Log("Miss " + key);
            if (missEffect != null)
                missEffect.SetActive(true);
            Invoke(nameof(DisableMissEffect), 0.2f);
        }
    }

    private void ContinuousHitCheck()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, new Vector3(0.4f, 1.0f, 0.9f));

        foreach (var h in hits)
        {
            Note note = h.GetComponent<Note>();
            if (note != null)
            {
                _hasHitDuringPress = true;

                Debug.Log("Hit " + key);
                if (goodEffect != null) goodEffect.SetActive(true);
                Invoke(nameof(DisableGoodEffect), 0.2f);

                note.Hit();
            }
        }
    }

    private void ScaleUpAndDown()
    {
        keyImage.transform.localScale = originScale * 1.2f;
        keyImage.transform.DOScale(originScale, 0.1f).SetEase(Ease.OutBack);
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
        //MultiTouchActions.Instance.OnTouchPress -= OnTouchPressed;
        //MultiTouchActions.Instance.OnMultiTouchPress -= OnMultiTouchPressed;
    }
}
