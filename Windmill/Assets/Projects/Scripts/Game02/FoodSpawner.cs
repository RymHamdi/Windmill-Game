using UnityEngine;
using UnityEngine.UI;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] prefabs;             // assign prefabs in inspector

    [Header("Spawn mode")]
    public bool autoDetectUI = true;         // try to detect UI prefabs automatically
    public bool spawnInUI = false;           // override if you always want UI/world
    public RectTransform uiParent;           // canvas or panel to parent UI spawns (required for UI)

    [Header("Timing")]
    public float spawnInterval = 1f;

    [Header("World spawn area (local to this transform)")]
    public Vector2 spawnAreaCenter = Vector2.zero;
    public Vector2 spawnAreaSize = new Vector2(8f, 4f);

    [Header("Initial physics (world prefabs)")]
    public bool applyInitialVelocity = true;
    public float minUpVelocity = 3f;
    public float maxUpVelocity = 6f;
    public float minHorizontalVelocity = -1f;
    public float maxHorizontalVelocity = 1f;
    public float maxTorque = 100f;

    float timer;

    void Start()
    {
        if (prefabs == null || prefabs.Length == 0)
            Debug.LogWarning("[RandomSpawner] No prefabs assigned.");

        if (spawnInUI && uiParent == null)
            Debug.LogWarning("[RandomSpawner] spawnInUI is true but uiParent is not assigned.");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRandom();
            timer = 0f;
        }
    }

    void SpawnRandom()
    {
        if (prefabs == null || prefabs.Length == 0) return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        if (prefab == null) return;

        bool isUI = spawnInUI;
        if (autoDetectUI)
        {
            // If prefab (or its children) has an Image or CanvasRenderer, treat as UI
            if (prefab.GetComponentInChildren<Image>(true) != null || prefab.GetComponentInChildren<CanvasRenderer>(true) != null)
                isUI = true;
            else
                isUI = false;
        }

        if (isUI)
            SpawnUI(prefab);
        else
            SpawnWorld(prefab);
    }

    void SpawnWorld(GameObject prefab)
    {
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect;
        float halfHeight = cam.orthographicSize;

        // Spawn near center
        float spawnX = Random.Range(cam.transform.position.x - halfWidth * 0.5f,
                                    cam.transform.position.x + halfWidth * 0.5f);
        float spawnY = Random.Range(cam.transform.position.y - halfHeight * 0.2f,
                                    cam.transform.position.y + halfHeight * 0.2f);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Random scale
        float scale = Random.Range(0.8f, 1.2f);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Smooth arc impulse
            float horizontal = Random.Range(-1f, 1f);
            float vertical = Random.Range(4f, 6f);
            rb.AddForce(new Vector2(horizontal, vertical), ForceMode2D.Impulse);

            // Spin
            rb.AddTorque(Random.Range(-8f, 8f), ForceMode2D.Impulse);

            rb.gravityScale = 0.25f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        Destroy(go, 4f);
    }

    void SpawnUI(GameObject prefab)
    {
        if (uiParent == null)
        {
            Debug.LogWarning("[Spawner] Cannot spawn UI prefab because uiParent is null.");
            return;
        }

        RectTransform parentRect = uiParent;
        Vector2 half = parentRect.rect.size * 0.5f;
        // pick a random anchored position inside the parent rect
        float x = Random.Range(-half.x, half.x) + spawnAreaCenter.x;
        float y = Random.Range(-half.y, half.y) + spawnAreaCenter.y;

        GameObject go = Instantiate(prefab, uiParent);
        RectTransform r = go.GetComponent<RectTransform>();
        if (r == null) r = go.AddComponent<RectTransform>(); // ensure it has one

        r.localScale = Vector3.one;
        r.anchoredPosition = new Vector2(x, y);

        Debug.Log($"[Spawner] Spawned UI prefab '{go.name}' at anchored position {r.anchoredPosition}");
    }

    // Visualize spawn area in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.TransformPoint((Vector3)spawnAreaCenter);
        Gizmos.DrawWireCube(center, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));
    }
}