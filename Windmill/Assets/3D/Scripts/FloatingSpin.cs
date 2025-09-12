using UnityEngine;

public class FloatingSpin : MonoBehaviour
{
    private Vector3 spinAxis;
    private float spinSpeed;

    void Start()
    {
        spinAxis = Random.onUnitSphere;
        spinSpeed = Random.Range(10f, 30f); // degrees per second
    }

    void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}
