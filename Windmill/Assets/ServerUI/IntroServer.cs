using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
public class IntroServer : MonoBehaviourPunCallbacks
{
    public Text infoText;
    public GameObject serverDisableVideoObject;
    public float introDuration = 10f;
    private bool introFinished = false;
    void Start()
    {
        if (PhotonLauncher.Instance == null)
        {
            this.enabled = false;
            return;
        }
        if (PhotonLauncher.Instance.isServer)
        {
            infoText.gameObject.SetActive(true);
            serverDisableVideoObject.SetActive(false);
            infoText.text = "Into Start";
        }
        if (ShowControlTrigger.Instance != null && PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance.SendTrigger("SYS_LAUNCHER");
        }
    }

    void Update()
    {
        if (!PhotonLauncher.Instance.isServer)
            return;
        if (!introFinished)
        {
            introDuration -= Time.deltaTime;
            infoText.text = "Intro Time Left: " + Mathf.CeilToInt(introDuration).ToString();
            if (introDuration <= 0f && !introFinished)
            {
                introFinished = true;
                //PhotonNetwork.LoadLevel("MainMenu");
            }
        }

    }
}
