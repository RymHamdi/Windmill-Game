using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // for smooth fading
using TMPro;
using Photon.Pun;
using System.Collections;

public class Timer : MonoBehaviourPun
{
    [Header("UI")]
    public TMP_Text timerText;
    public Image alertImage; // drag your alert UI image here

    [Header("Settings")]
    public float startTime = 60f; // 60 seconds

    private float currentTime;
    private bool isRunning = false;
    private bool alertActive = false;

    public GameObject ObjectToHide;
    public GameObject ObjectToShow;

    void Start()
    {
        currentTime = startTime;
        isRunning = true;
        UpdateTimerUI();

        if (alertImage != null)
            alertImage.gameObject.SetActive(false); // hide at start

        if (CollectableUIManager.Instance != null)
        {
            //CollectableUIManager.Instance.OnAllItemsCollected += EndGame;
        }
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            TimerEnded();
        }

        // Start alert at last 10 seconds
        if (!alertActive && currentTime <= 10f)
        {
            alertActive = true;
            StartAlert();
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    private void TimerEnded()
    {
        Debug.Log("Timer Ended!");
        StopAlert();
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("EndTimeAlert");
            Invoke("ShowHidePanelAfterDely", 0.5f);
        }
    }

    private void ShowHidePanelAfterDely()
    {
        photonView.RPC("ShowHidePanel", RpcTarget.AllBuffered);
    }

    private void EndGame()
    {
        isRunning = false;
        StopAlert();
        photonView.RPC("ShowHidePanel", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void ShowHidePanel()
    {
        if (ObjectToHide != null)
            ObjectToHide.SetActive(false);

        if (ObjectToShow != null)
            ObjectToShow.SetActive(true);
    }

    private void StartAlert()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("StartTimeAlert");
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("TimeAlert");
        }
        if (alertImage == null) return;

        alertImage.gameObject.SetActive(true);
        Color c = alertImage.color;
        c.a = 0f;
        alertImage.color = c;

        // Fade alpha between 0 and 1 endlessly
        alertImage.DOFade(1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    private void StopAlert()
    {
        if (alertImage == null) return;

        alertImage.DOKill(); // stop any tweens
        alertImage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (CollectableUIManager.Instance != null)
        {
            //CollectableUIManager.Instance.OnAllItemsCollected -= EndGame;
        }
    }
}
