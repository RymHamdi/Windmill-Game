using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Photon.Pun;


public class VideoSelection : MonoBehaviour
{
    public List<VideoClip> videoClips; // List of video clips to choose from
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                videoPlayer.Stop();
                return;
            }
        }
        int playerLocalSelection = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterId", out object selection) ? (int)selection : 0;
        videoPlayer.clip = videoClips[playerLocalSelection];
        videoPlayer.Play();
    }
}
