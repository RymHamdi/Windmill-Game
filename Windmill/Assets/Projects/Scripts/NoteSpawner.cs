using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
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
        float travelTime = 2.0f;

        GameObject note =  Instantiate(notePrefab, spawnPos, Quaternion.identity);
        
        note.GetComponent<Note>().Initialize(spawnPos, endPos, travelTime);
    }
}
