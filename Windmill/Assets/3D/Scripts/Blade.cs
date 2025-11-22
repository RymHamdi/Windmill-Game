using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Blade : MonoBehaviour
{
    public float minSliceVelocity = 0.1f;
    public float zOffset = 10f;

    private Camera cam;
    private Vector3 lastPos;
    private Collider bladeCollider;

    public ParticleSystem trailParticles;
    public TrailRenderer trailRenderer;

    void Awake()
    {
        cam = Camera.main;
        bladeCollider = GetComponent<Collider>();
        bladeCollider.isTrigger = true;
        bladeCollider.enabled = false;

        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
    }

    void Start()
    {
        EndSlice();
        //trailParticles.useWorldSpace = false;
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSlice();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndSlice();
        }

        if (Input.GetMouseButton(0))
        {
            ContinueSlice();
        }
    }

    void StartSlice()
    {
        trailRenderer.enabled = false;
        trailRenderer.Clear();
        Vector3 startPos = GetWorldPos();
        transform.position = startPos;
        lastPos = startPos;

        

        if (trailParticles != null)
        {
            trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            trailParticles.Play();
        }
        Invoke("StartSlicingAfterFrame", 0.05f);
    }

    void StartSlicingAfterFrame()
    {
        trailRenderer.enabled = true;
        bladeCollider.enabled = true;
    }

    void EndSlice()
    {
        trailRenderer.Clear();
        trailRenderer.enabled = false;
        bladeCollider.enabled = false;

        if (trailParticles != null)
        {
            trailParticles.Stop();
        }
    }

    void ContinueSlice()
    {
        Vector3 newPos = GetWorldPos();
        float velocity = (newPos - lastPos).magnitude / Time.deltaTime;

        bool slicing = velocity >= minSliceVelocity;
        bladeCollider.enabled = slicing;

        if (slicing)
        {
            Vector3 direction = newPos - lastPos;
            float distance = direction.magnitude;

            RaycastHit[] hits = Physics.SphereCastAll(lastPos, 0.5f, direction, distance);

            foreach (var hit in hits)
            {
                Fruit fruit = hit.transform.GetComponent<Fruit>();
                if (fruit != null)
                    fruit.Slice();
            }
        }

        transform.position = newPos;
        lastPos = newPos;
    }

    Vector3 GetWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zOffset;
        return cam.ScreenToWorldPoint(mousePos);
    }
}
