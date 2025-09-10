using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public GameObject trashPrefab;
    public Line[] lanes; // 4 lane positions
    public float spawnInterval = 1.5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0f;
        }
    }

    void SpawnNote()
    {
        int laneIndex = Random.Range(0, lanes.Length);
        Vector3 spawnPos = lanes[laneIndex].startPoint.position;
        Vector3 endPos = lanes[laneIndex].endPoint.position;
        Vector3 h = lanes[laneIndex].hitZonePoint.position;
        float travelTime = Random.Range(1.5f, 2.0f);
        GameObject prefabToSpawn = (Random.value < 0.9f) ? notePrefab : trashPrefab;
        GameObject note = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        
        note.GetComponent<Note>().Initialize(spawnPos, endPos, h, travelTime);
    }
}
