using UnityEngine;

public enum FruitType
{
    Good,
    Bad
}

public class Fruit : MonoBehaviour
{
    [Header("Fruit Halves")]
    public GameObject fruitHalfPrefabL;
    public GameObject fruitHalfPrefabR;

    [Header("Slice FX")]
     [Header("World FX Prefabs")]
    public GameObject sliceVFXPrefab; // Assign particle system or any prefab here
    public AudioClip sliceSound;
    public float explosionForce = 3f;
    public float explosionTorque = 5f;

    [Header("Fruit Type")]
    public FruitType type = FruitType.Good;

    [Header("UI Feedback")]
    public GameObject bingoSpritePrefab; // UI prefab under Canvas
    public GameObject badSpritePrefab; // UI prefab under Canvas

    private bool isSliced = false;

    void OnTriggerEnter(Collider other)
    {
        if (isSliced) return;

        if (other.CompareTag("Blade"))
        {
            Slice();
        }
    }

    void Slice()
    {
        isSliced = true;

        // Spawn halves
        GameObject halfL = Instantiate(fruitHalfPrefabL, transform.position, transform.rotation);
        GameObject halfR = Instantiate(fruitHalfPrefabR, transform.position, transform.rotation);

        Rigidbody rbL = halfL.GetComponent<Rigidbody>() ?? halfL.AddComponent<Rigidbody>();
        Rigidbody rbR = halfR.GetComponent<Rigidbody>() ?? halfR.AddComponent<Rigidbody>();

        // Explosion forces
        rbL.AddForce((-transform.right + Vector3.up) * explosionForce, ForceMode.Impulse);
        rbR.AddForce((transform.right + Vector3.up) * explosionForce, ForceMode.Impulse);

        rbL.AddTorque(Random.insideUnitSphere * explosionTorque * Time.deltaTime, ForceMode.Impulse);
        rbR.AddTorque(Random.insideUnitSphere * explosionTorque * Time.deltaTime, ForceMode.Impulse);

        // ===== 1. WORLD VFX (particles, 3D FX) =====
        if (sliceVFXPrefab != null)
        {
            GameObject vfx = Instantiate(sliceVFXPrefab, transform.position, Quaternion.identity);

            // Optional: auto-destroy after duration if it has a ParticleSystem
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(vfx, ps.main.duration);
            else
                Destroy(vfx, 2f); // fallback
        }
        // Sound
        if (sliceSound != null) AudioSource.PlayClipAtPoint(sliceSound, transform.position);

// Show VFX depending on fruit type
Canvas canvas = FindObjectOfType<Canvas>();
if (canvas != null)
{
    GameObject fxPrefab = null;

    if (type == FruitType.Good && bingoSpritePrefab != null)
    {
        fxPrefab = bingoSpritePrefab; // prefab with SliceFX sliceType = Good
    }
    else if (type == FruitType.Bad && badSpritePrefab != null)
    {
        fxPrefab = badSpritePrefab; // prefab with SliceFX sliceType = Bad
    }

    if (fxPrefab != null)
    {
        GameObject fx = Instantiate(fxPrefab, canvas.transform);

        // Convert world position of fruit into canvas position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );

        fx.GetComponent<RectTransform>().anchoredPosition = localPos;
    }
}
else
{
    Debug.LogError("No Canvas found in the scene for slice UI!");
}

        Destroy(gameObject);
        Destroy(halfL, 3f);
        Destroy(halfR, 3f);
    }
}