using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;

public class VideoLoopingScene : MonoBehaviourPunCallbacks
{
    public List<VideoLoopData> videoLoopDatas;

    private VideoLoopData videoLoopDataToPlay;

    public GameObject serverDisableVideoObject;
    public VideoPlayer videoPlayer1;
    public Text infoText;
    private bool video1Started;

    public float videoTimeToSwitch1 = 60.4f; // seconds
    public VideoType videoType;

    float timerVideo1 = 0f;
    public string sceneName;

    public string eventTriggerstart;
    public string eventTriggerEnd;

    // 🔹 NEW: Loaded from JSON
    private Dictionary<string, string> videoPathMap = new Dictionary<string, string>();

    void OnEnable()
    {
        video1Started = false;
        timerVideo1 = 0f;

        LoadVideoConfig();
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

            if (ShowControlTrigger.Instance != null)
            {
                ShowControlTrigger.Instance.SendTrigger(eventTriggerstart);
            }
            OnVideoEnd(videoPlayer1);
        }
    }

    void Update()
    {
        if (!PhotonLauncher.Instance.isServer)
            return;

        if (video1Started)
        {
            timerVideo1 += Time.deltaTime;

            if (timerVideo1 >= 1f)
            {
                videoTimeToSwitch1 -= 1;
                timerVideo1 = 0f;
            }

            if (videoTimeToSwitch1 <= 0.04f)
            {
                videoTimeToSwitch1 -= Time.deltaTime;
            }

            infoText.text =
                $"Server: Playing Video 1 - switching Scene in {videoTimeToSwitch1} seconds";

            if (videoTimeToSwitch1 <= 0)
            {
                
                infoText.text = "";

                if (ShowControlTrigger.Instance != null)
                {
                    ShowControlTrigger.Instance.SendTrigger(eventTriggerEnd);
                }

                if (!string.IsNullOrEmpty(sceneName))
                {
                    //PhotonNetwork.LoadLevel(sceneName);
                    //Let's check if all player have finished the video before to change the scene
                    bool allFinished = true;
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        object playedVideoObj;
                        if (player.CustomProperties.TryGetValue("playedVideo", out playedVideoObj))
                        {
                            bool playedVideo = (bool)playedVideoObj;
                            if (playedVideo)
                            {
                                allFinished = false;
                                break;
                            }
                        }
                    }
                    if (allFinished)
                    {
                        video1Started = false;
                        PhotonNetwork.LoadLevel(sceneName);
                    }
                }
                else
                {
                    if (PhotonLauncher.Instance != null && PhotonLauncher.Instance.isServer)
                    {
                        bool allFinished = true;
                        foreach (var player in PhotonNetwork.PlayerList)
                        {
                            object playedVideoObj;
                            if (player.CustomProperties.TryGetValue("playedVideo", out playedVideoObj))
                            {
                                bool playedVideo = (bool)playedVideoObj;
                                if (playedVideo)
                                {
                                    allFinished = false;
                                    break;
                                }
                            }
                        }
                        if (allFinished)
                        {
                            video1Started = false;
                            PhotonLauncher.Instance.EndGame();
                        }
                        
                    }
                }
            }
        }
    }

    // ================= RPC =================

    [PunRPC]
    public void RPC_PlayVideo1()
    {
        PlayVideo1();
    }

    // ================= VIDEO CONFIG =================

    private void LoadVideoConfig()
    {
        string configPath = "";
        if (videoType == VideoType.Intro)
        {
            configPath = Path.Combine(
               Application.dataPath, "../video_intro.json"
           );
        }
        else if (videoType == VideoType.Outro)
        {
            configPath = Path.Combine(
               Application.dataPath, "../video_outro.json"
           );
        }
        Debug.Log($"Loading video config from: {configPath}");

        if (!File.Exists(configPath))
        {
            Debug.LogError("❌ video_intro.json not found!");
            return;
        }

        string json = File.ReadAllText(configPath);
        VideoConfigWrapper wrapper =
            JsonUtility.FromJson<VideoConfigWrapper>(json);

        videoPathMap.Clear();

        foreach (var entry in wrapper.videos)
        {
            videoPathMap[entry.key] = entry.path;
        }

        Debug.Log("✅ Video config loaded");
    }

    // ================= FIND VIDEO =================

    private void FindVideoToPlay()
    {
        string key = $"{PhotonNetwork.LocalPlayer.NickName}_video";

        string assignedVideo = PlayerPrefs.HasKey(key)
            ? PlayerPrefs.GetString(key)
            : "Video1";

        Debug.Log($"Video assigned from prefs: {assignedVideo}");

        bool found = false;

        foreach (var videoData in videoLoopDatas)
        {
            if (videoData.videoName == assignedVideo)
            {
                videoLoopDataToPlay = videoData;
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning(
                $"Video '{assignedVideo}' not found. Defaulting to Video1."
            );

            foreach (var videoData in videoLoopDatas)
            {
                if (videoData.videoName == "Video1")
                {
                    videoLoopDataToPlay = videoData;
                    break;
                }
            }
        }

    }

    // ================= PLAY VIDEO =================

    private void PlayVideo1()
    {
        if (PhotonLauncher.Instance.isServer)
            return;

        if (!videoPathMap.ContainsKey(videoLoopDataToPlay.videoName))
        {
            Debug.LogError(
                $"❌ No video path found for {videoLoopDataToPlay.videoName}"
            );
            return;
        }

        string fullPath = Path.Combine(
            Application.dataPath,
            "../",
            videoPathMap[videoLoopDataToPlay.videoName]
        );

        videoPlayer1.source = VideoSource.Url;
        videoPlayer1.url = fullPath;

        // Subscribe to video end event
        videoPlayer1.loopPointReached += OnVideoEnd;

        videoPlayer1.Play();
        //Let's update the  custop propertie for this player that he start playing a video(any video just we want to know that he played a video)
        ExitGames.Client.Photon.Hashtable props =
            new ExitGames.Client.Photon.Hashtable
            {
                { "playedVideo", true }
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // let create a function or action that know when the video ended
    public void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video Ended");
        ExitGames.Client.Photon.Hashtable props =
            new ExitGames.Client.Photon.Hashtable
            {
                { "playedVideo", false }
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void OnDisable()
    {
        // Unsubscribe from video end event
        if (videoPlayer1 != null)
        {
            videoPlayer1.loopPointReached -= OnVideoEnd;
        }
    }
}

[System.Serializable]
public struct VideoLoopData
{
    public string videoName; // MUST match JSON key
}

[System.Serializable]
public class VideoEntry
{
    public string key;
    public string path;
}

[System.Serializable]
public class VideoConfigWrapper
{
    public List<VideoEntry> videos;
}

public enum VideoType
{
    Intro,
    Outro
}
