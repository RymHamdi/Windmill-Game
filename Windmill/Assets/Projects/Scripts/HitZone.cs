using UnityEngine;

public class HitZone : MonoBehaviour
{
    public KeyCode key; // assign per lane (A, S, D, F)

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
                note.Hit();
                return;
            }
        }

        Debug.Log("Miss " + key);
    }

    void OnMouseDown()
    {
        CheckHit();
    }
}
