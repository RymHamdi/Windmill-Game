using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using System.Collections;

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

    // Safety nets so one client's broken/slow video file can't freeze the whole room forever.
    private const float VideoPrepareTimeoutSeconds = 12f;
    // Small delay before we start trusting "playedVideo" properties, so a leftover "false" from
    // the previous round can't be mistaken for an instant finish and skip the scene immediately.
    private const float MinGraceBeforeCheckSeconds = 3f;
    // Extra time allowed on top of videoTimeToSwitch1 (the expected video length) before we give
    // up on a client that never reports back and force everyone else forward anyway.
    public float maxOvertimeAfterVideoStart = 30f;
    private float elapsedSinceVideoStart = 0f;

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

        if (!video1Started)
            return;

        elapsedSinceVideoStart += Time.deltaTime;
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

        infoText.text = videoTimeToSwitch1 > 0
            ? $"Server: Playing Video 1 - switching Scene in {videoTimeToSwitch1} seconds"
            : "Server: Waiting for all players to finish their video...";

        if (elapsedSinceVideoStart < MinGraceBeforeCheckSeconds)
            return;

        bool allFinished = AllPlayersFinishedVideo();
        // Once videoTimeToSwitch1 crosses zero it keeps counting down into negative numbers,
        // so -videoTimeToSwitch1 is exactly how far past the expected video length we are.
        bool overtime = videoTimeToSwitch1 <= 0 && (-videoTimeToSwitch1) >= maxOvertimeAfterVideoStart;

        if (!allFinished && !overtime)
        {
            return;
        }

        if (!allFinished)
        {
            Debug.LogWarning($"[VideoLoopingScene] Forcing scene advance {maxOvertimeAfterVideoStart}s past the expected video length - at least one client never reported finishing its video.");
        }

        infoText.text = "";

        if (ShowControlTrigger.Instance != null)
        {
            ShowControlTrigger.Instance.SendTrigger(eventTriggerEnd);
        }

        video1Started = false;
        elapsedSinceVideoStart = 0f;

        if (!string.IsNullOrEmpty(sceneName))
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
        else if (PhotonLauncher.Instance != null && PhotonLauncher.Instance.isServer)
        {
            PhotonLauncher.Instance.EndGame();
        }
    }

    private bool AllPlayersFinishedVideo()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("playedVideo", out object playedVideoObj)
                && playedVideoObj is bool playedVideo
                && playedVideo)
            {
                return false;
            }
        }

        return true;
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
        else if (videoType == VideoType.Launcher)
        {
            configPath = Path.Combine(
               Application.dataPath, "../video_launcher.json"
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

            switch (assignedVideo)
            {
                case "Video1":
                AssignID(0);
                break;
                case "Video2":
                    AssignID(1);
                    break;
                case "Video3":
                    AssignID(2);
                    break;
                case "Video4":
                    AssignID(3);
                    break;
                case "Video5":
                    AssignID(4);
                    break;
                default:
                    
                    assignedVideo = "Video1";
                    break;
            }

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

    private void AssignID(int localPlayerindex)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "CharacterId", localPlayerindex }
        };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.LogError($"Assigned CharacterId {localPlayerindex} to player {PhotonNetwork.LocalPlayer.NickName}");
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
        videoPlayer1.errorReceived += OnVideoError;
        videoPlayer1.Prepare();

        // Subscribe to video end event
        //videoPlayer1.loopPointReached += OnVideoEnd;
        StartCoroutine(WaitForVideoEnd(videoPlayer1));

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

    IEnumerator WaitForVideoEnd(VideoPlayer vp)
    {
        // Wait until video is prepared (length available), but never longer than the timeout -
        // a stuck/failed Prepare() (bad path, locked file, slow disk) must not freeze the whole room.
        float prepareElapsed = 0f;
        while (!vp.isPrepared)
        {
            prepareElapsed += Time.deltaTime;
            if (prepareElapsed >= VideoPrepareTimeoutSeconds)
            {
                Debug.LogError($"❌ Video failed to prepare within {VideoPrepareTimeoutSeconds}s ({vp.url}). Skipping it.");
                OnVideoEnd(vp);
                yield break;
            }
            yield return null;
        }

        double triggerTime = vp.length - 2.0; // 2 seconds before real end

        // Wait until video reaches trigger time
        while (vp.time < triggerTime)
        {
            yield return null;
        }

        OnVideoEnd(vp);
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"❌ VideoPlayer error ({source.url}): {message}. Treating as finished.");
        OnVideoEnd(source);
    }

    void OnDisable()
    {
        // Unsubscribe from video end event
        if (videoPlayer1 != null)
        {
            videoPlayer1.loopPointReached -= OnVideoEnd;
            videoPlayer1.errorReceived -= OnVideoError;
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
    Outro,
    Launcher
}
