using Photon.Pun;
using UnityEngine;
using TMPro;

public class IntroManager : MonoBehaviourPun
{
    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text titleText;

    [Header("Objects")]
    public GameObject introPanel;   // The intro UI
    public GameObject gamePanel;    // The actual game content

    public GameObject skipButton;

    [Header("Settings")]
    public float introDuration = 20f;

    private float timer;
    private bool isRunning = true;

    void Start()
    {
        timer = introDuration;
        titleText.text = "Intro will be skipped in:";
        introPanel.SetActive(true);
        gamePanel.SetActive(false);
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0f && PhotonNetwork.IsMasterClient)
        {
            SkipIntro(); // auto skip when timer ends
        }
        skipButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void OnSkipButton()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("SkipIntroRPC", RpcTarget.AllBuffered);
    }

    private void SkipIntro()
    {
        photonView.RPC("SkipIntroRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SkipIntroRPC()
    {
        isRunning = false;
        introPanel.SetActive(false);
        gamePanel.SetActive(true);
        Debug.Log("Intro finished → Game 1 started");
    }
}
