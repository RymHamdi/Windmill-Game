using UnityEngine;

public class InitiPhotonLauncher : MonoBehaviour
{
    public GameObject photonLauncherPrefab;

    void Start()
    {
        if (PhotonLauncher.Instance == null)
        {
            Instantiate(photonLauncherPrefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
