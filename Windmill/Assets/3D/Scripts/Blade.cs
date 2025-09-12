using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Blade : MonoBehaviour
{
    public float minSliceVelocity = 0.1f;
    public float zOffset = 10f; // distance from camera
    private Camera cam;
    private Vector3 lastPos;
    private Collider bladeCollider;

    void Awake()
    {
        cam = Camera.main;
        bladeCollider = GetComponent<Collider>();
        bladeCollider.isTrigger = true;
        bladeCollider.enabled = false; // only active when moving
    }

   public ParticleSystem trailParticles;

void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        lastPos = GetWorldPos();
        bladeCollider.enabled = true;
        if (trailParticles != null) trailParticles.Play();
    }
    else if (Input.GetMouseButtonUp(0))
    {
        bladeCollider.enabled = false;
        if (trailParticles != null) trailParticles.Stop();
    }

    if (Input.GetMouseButton(0))
    {
        Vector3 newPos = GetWorldPos();
        transform.position = newPos;

        float velocity = (newPos - lastPos).magnitude / Time.deltaTime;
        bladeCollider.enabled = velocity >= minSliceVelocity;

        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            emission.enabled = velocity >= minSliceVelocity;
        }

        lastPos = newPos;
    }
}

    Vector3 GetWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zOffset; // keep in front of camera
        return cam.ScreenToWorldPoint(mousePos);
    }
}