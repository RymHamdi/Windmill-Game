using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using DG.Tweening;

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
    public GameObject introTitle;
    public FadeCanvas fadeCanvas;
   

    void Start()
    {
        isRunning = false;

        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                canvasServer.SetActive(true);
                GetComponent<CanvasGroup>().alpha = 0;
            }
        }
        fadeCanvas.FadeOut();
        Invoke("Init", 0.3f);
    }

    public bool requireShowGameIndex = true;

    private void Init()
    {
        if (requireShowGameIndex)
        {
            StartCoroutine(GameInDexParent.instance.EnableCanvas(() =>
        {
            isRunning = true;
            panelOfIntro.SetActive(true);
        }));
        }
        else
        {
            isRunning = true;
            panelOfIntro.SetActive(true);
        }
        
        introTitle.SetActive(true);
        timer = introDuration;
        //titleText.text = "Intro will be skipped in:";
        introPanel.SetActive(true);
        gamePanel.SetActive(false);
        
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
            Invoke("ShowYourTurn", 0.1f);

        }
    }

    public CanvasGroup yourTurnCanvasGroup;

    private void ShowYourTurn()
    {
        yourTurnCanvasGroup.gameObject.SetActive(true);
        yourTurnCanvasGroup.DOFade(1, 0.5f).OnComplete(() =>
        {
            panelOfIntro.SetActive(false);
            Invoke("HideYourTurn", 2f);
        });
    }

    public GameObject panelOfIntro;
    public void HideYourTurn()
    {
        yourTurnCanvasGroup.DOFade(0, 0.25f).OnComplete(() =>
        {

            yourTurnCanvasGroup.gameObject.SetActive(false);
            //FadeIn();
            //Invoke("FadeOut", 0.9f);
            Invoke("OnSkipButton", 0.3f);
        });
    }

    private void FadeIn()
    {
        fadeCanvas.FadeIn();
    }

    private void FadeOut()
    {
        
        fadeCanvas.FadeOut();
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
