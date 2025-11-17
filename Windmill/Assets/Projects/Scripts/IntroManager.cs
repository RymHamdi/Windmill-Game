using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class IntroManager : MonoBehaviourPun
{
    [Header("UI")]
    public TMP_Text timerText;
    //public TMP_Text titleText;

    [Header("Objects")]
    public GameObject introPanel;   // The intro UI
    public GameObject gamePanel;    // The actual game content

    public GameObject skipButton;

    [Header("Settings")]
    public float introDuration = 20f;

    private float timer;
    private bool isRunning = true;

    public List<IntroDivider> introDividers;
    public float timeBeforeRunDivider = 3;
    private int currentDividerIndex = 0;

    public string IntroKey;
    public string GameKey;
    public GameObject canvasServer;

    void Start()
    {
        timer = introDuration;
        //titleText.text = "Intro will be skipped in:";
        introPanel.SetActive(true);
        gamePanel.SetActive(false);
        if (PhotonLauncher.Instance!= null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                canvasServer.SetActive(true);
                GetComponent<CanvasGroup>().alpha = 0;
            }
        }
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(timer).ToString();
        }

        timeBeforeRunDivider -= Time.deltaTime;
        if (timeBeforeRunDivider <= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ShowControlTrigger.Instance?.SendTrigger(IntroKey.ToString());
            }
            RunDivider();
            timeBeforeRunDivider = Mathf.Infinity;
        }

        if (timer <= 0f && PhotonNetwork.IsMasterClient)
        {
            //SkipIntro(); // auto skip when timer ends
        }
        //skipButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    private void RunDivider()
    {
        if (currentDividerIndex < introDividers.Count)
        {
            introDividers[currentDividerIndex].gameObject.SetActive(true);
            introDividers[currentDividerIndex].Init(OnDividerComplete);
            currentDividerIndex++;
        }
    }

    private void OnDividerComplete()
    {
        if (currentDividerIndex < introDividers.Count)
        {
            RunDivider();
        }
        else
        {
            Invoke("OnSkipButton", 0.5f);
        }
    }

    public void OnSkipButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger(GameKey.ToString());
            photonView.RPC("SkipIntroRPC", RpcTarget.AllBuffered);
        }

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
        Debug.Log("Intro finished → Game 2 started");
    }
}
