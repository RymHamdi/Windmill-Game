using UnityEngine;

public class Food : MonoBehaviour
{
    public GameObject leftHalfPrefab;
    public GameObject rightHalfPrefab;
    public ParticleSystem juicePrefab;
    public int scoreValue = 1;
    public AudioClip sliceSfx;
    public float halfSeparationForce = 200f;
    public float halfUpForce = 100f;
    public float destroyAfter = 3f;

    bool sliced = false;

    public void Slice(Vector2 direction, Vector2 slicePoint)
    {
        if (sliced) return;
        sliced = true;

        // optional: disable original visuals and collider to prevent double hits
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        // spawn juice
        if (juicePrefab)
        {
            ParticleSystem ps = Instantiate(juicePrefab, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        // spawn halves
        if (leftHalfPrefab && rightHalfPrefab)
        {
            Vector3 pos = transform.position;
            GameObject left = Instantiate(leftHalfPrefab, pos, transform.rotation);
            GameObject right = Instantiate(rightHalfPrefab, pos, transform.rotation);

            Rigidbody2D lrb = left.GetComponent<Rigidbody2D>();
            Rigidbody2D rrb = right.GetComponent<Rigidbody2D>();

            Vector2 perp = new Vector2(-direction.y, direction.x).normalized;

            if (lrb != null)
                lrb.AddForce((-perp + direction) * halfSeparationForce);
            if (rrb != null)
                rrb.AddForce((perp + direction) * halfSeparationForce);

            // add slight upwards velocity
            if (lrb != null) lrb.AddForce(Vector2.up * halfUpForce);
            if (rrb != null) rrb.AddForce(Vector2.up * halfUpForce);

            Destroy(left, destroyAfter);
            Destroy(right, destroyAfter);
        }

        // audio & score
        if (AudioManager.Instance != null && sliceSfx != null)
            //AudioManager.Instance.PlaySFX(sliceSfx);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreValue);

        // finally destroy original
        Destroy(gameObject);
    }
}
