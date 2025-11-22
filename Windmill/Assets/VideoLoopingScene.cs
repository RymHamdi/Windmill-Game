using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;

public class VideoLoopingScene : MonoBehaviourPunCallbacks
{
    public List<VideoLoopData> videoLoopDatas;

    private VideoLoopData videoLoopDataToPlay;

    public GameObject serverDisableVideoObject;
    public VideoPlayer videoPlayer1;
    public Text infoText;
    private bool video1Started;

    public int videoTimeToSwitch1 = 4; // seconds

    // REAL second timers (separate for each video)
    float timerVideo1 = 0f;
    public string sceneName;

    void OnEnable()
    {
        video1Started = false;
        timerVideo1 = 0f;

        FindVideoToPlay();
    }

    void Start()
    {
        if (PhotonLauncher.Instance.isServer)
        {
            infoText.gameObject.SetActive(true);
            photonView.RPC("RPC_PlayVideo1", RpcTarget.All);

            serverDisableVideoObject.SetActive(false);
            video1Started = true;

            infoText.text = "Server: Playing Video 1";
        }
    }

    void Update()
    {
        // SERVER ONLY — clients just play the video files
        if (!PhotonLauncher.Instance.isServer)
            return;

        // === VIDEO 1 COUNTDOWN ===
        if (video1Started)
        {
            timerVideo1 += Time.deltaTime;

            if (timerVideo1 >= 1f)
            {
                videoTimeToSwitch1 -= 1;
                timerVideo1 = 0f;
            }

            infoText.text = $"Server: Playing Video 1 - switching Scene in {videoTimeToSwitch1} seconds";

            if (videoTimeToSwitch1 <= 0)
            {
                video1Started = false;
                infoText.text = "";
                if (sceneName != "")
                {
                    PhotonNetwork.LoadLevel(sceneName);
                }
                else
                {
                    if (PhotonLauncher.Instance != null)
                    {
                        if (PhotonLauncher.Instance.isServer)
                        {
                            PhotonLauncher.Instance.EndGame();
                        }
                    }
                }
            }


        }
    }

    // === RPC CALLS ===

    [PunRPC]
    public void RPC_PlayVideo1()
    {
        PlayVideo1();
    }

    // === FIND VIDEO ===
    private void FindVideoToPlay()
{
    string key = $"{PhotonNetwork.LocalPlayer.NickName}_video";

    // If key does NOT exist → fallback to "Video1"
    string assignedVideo = PlayerPrefs.HasKey(key)
        ? PlayerPrefs.GetString(key)
        : "Video1";

    Debug.Log($"Video assigned from prefs: {assignedVideo}");

    // Try to find a match
    bool found = false;
    foreach (var videoData in videoLoopDatas)
    {
        string key = $"{PhotonNetwork.LocalPlayer.NickName}_video";

        // If key does NOT exist → fallback to "Video1"
        string assignedVideo = PlayerPrefs.HasKey(key)
            ? PlayerPrefs.GetString(key)
            : "Video1";

        Debug.Log($"Video assigned from prefs: {assignedVideo}");

        // Try to find a match
        bool found = false;
        foreach (var videoData in videoLoopDatas)
        {
            if (videoData.videoName == "Video1")
            {
                videoLoopDataToPlay = videoData;
                found = true;
                Debug.Log($"Found video to play: {videoLoopDataToPlay.videoName}");
                break;
            }
        }

        // If NOT found → fallback to "Video1"
        if (!found)
        {
            Debug.LogWarning($"Video '{assignedVideo}' not found. Defaulting to Video1.");

            foreach (var videoData in videoLoopDatas)
            {
                if (videoData.videoName == "Video1")
                {
                    videoLoopDataToPlay = videoData;
                    Debug.Log("Fallback selected video: Video1");
                    break;
                }
            }
        }
    }
}



    // === PLAY VIDEO LOCALLY ===
    private void PlayVideo1()
    {
        if (PhotonLauncher.Instance.isServer)
            return;

        videoPlayer1.clip = videoLoopDataToPlay.videoClip1;
        videoPlayer1.isLooping = true;
        videoPlayer1.Play();
    }

}

[System.Serializable]
public struct VideoLoopData
{
    public string videoName;
    public VideoClip videoClip1;
}