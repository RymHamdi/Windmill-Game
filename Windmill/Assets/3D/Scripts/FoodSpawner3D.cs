using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] fruitPrefabs; // Assign your 3D fruit prefabs

    private List<Fruit> listOfFruits = new List<Fruit>();

    [Header("Spawn timing")]
    public float spawnInterval = 1f;

    [Header("Spawn area (centered around this object)")]
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(6f, 2f, 2f); // X, Y, Z range

    [Header("Initial physics")]
    public float minUpForce = 4f;
    public float maxUpForce = 6f;
    public float horizontalRange = 1f;
    public float maxTorque = 1f; // Further reduced from 1f to 0.2f

    [Header("Scale")]
    public Vector2 scaleRange = new Vector2(1.5f, 1.8f);

    [Header("Lifetime")]
    public float destroyAfter = 4f;


    private float timer;
    
    void Start()
    {
        timer = 0f;
        foreach (var fruitPrefab in fruitPrefabs)
        {
            var fruitComponent = fruitPrefab.GetComponent<Fruit>();
            if (fruitComponent != null)
            {
                listOfFruits.Add(fruitComponent);
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRandomFruit();
            
            timer = 0f;
        }
    }

    void SpawnRandomFruit()
    {
        if (fruitPrefabs.Length == 0) return;

        int currentRequireFruitIndex = CollectableUIManager.Instance.CollectedItems + 1;
        Fruit requiredFruit = listOfFruits.Find(f => f.id == currentRequireFruitIndex);
        // Let's make sure that percentage chance to spawn required fruit is higher
        GameObject prefab;
        if (requiredFruit != null && Random.value <= 0.3f) // 30% chance to spawn required fruit
        {
            prefab = requiredFruit.gameObject;
        }
        else
        {
            prefab = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
        }

        if (prefab == null) return;

        // Spawn at the bottom of the spawn area
        Vector3 spawnPos = new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f) + spawnAreaCenter.x,
            spawnAreaCenter.y - spawnAreaSize.y / 2f, // bottom Y
            Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f) + spawnAreaCenter.z
        );
         Quaternion rotationQuaternion = Quaternion.Euler(spawnPos);
        GameObject fruit = Instantiate(prefab, spawnPos, rotationQuaternion);

        // Random scale
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        fruit.transform.localScale = new Vector3(scale, scale, scale);

        // Remove physics for floating effect
        //Rigidbody rb = fruit.GetComponent<Rigidbody>();
        //if (rb != null) Destroy(rb);

        // Add floating movement and gentle spin
        var mover = fruit.AddComponent<FruitFloatMover>();
        mover.floatSpeed = 0.8f; // slow upward speed
        mover.spinSpeed = Random.Range(10f, 25f); // gentle spin

        // Auto destroy
        Destroy(fruit, destroyAfter * 100);
    }

    // Optional: visualize spawn area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position + spawnAreaCenter;
        Gizmos.DrawWireCube(center, spawnAreaSize);
    }
}

public class FruitFloatMover : MonoBehaviour
{
    public float floatSpeed = 0.6f;
    public float spinSpeed = 20f;
    private Vector3 spinAxis;
    private bool initialized = false;
    private Vector3 center;
    private float circleRadius;
    private float circleSpeed;
    private float circleAngle;
    private float circleHeight;
    public Rigidbody rb;
    public float startForce = 5f;


    void Start()
    {
        spinAxis = Random.onUnitSphere;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Random angle between -30 and 30 degrees on the X axis
            float angle = Random.Range(-20f, 20f);
            Vector3 up = Quaternion.Euler(0, 0f, angle) * transform.up;
            transform.up = up;
        }
		rb.AddForce(transform.up * startForce , ForceMode.Impulse);
    }

    void Update()
    {

        // Spin gently around a random axis
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}