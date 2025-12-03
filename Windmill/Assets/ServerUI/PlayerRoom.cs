using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerRoom : MonoBehaviour
{
    public TMP_Text playerID;
    public Dropdown roleDropdown;
    public Dropdown videoDropdown;

    public Button removeButton;
    public Button assignButton;
    public Button flashButton;

    PlayerRoomData _data;

    public void Initialize(PlayerRoomData data)
    {
        _data = data;

        playerID.text = data.playerId;

        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(data.roleOptions);

        videoDropdown.ClearOptions();
        videoDropdown.AddOptions(data.videoOptions);

        roleDropdown.value = data.roleOptions.IndexOf(data.role);
        videoDropdown.value = data.videoOptions.IndexOf(data.video);

        roleDropdown.onValueChanged.RemoveAllListeners();
        videoDropdown.onValueChanged.RemoveAllListeners();

        roleDropdown.onValueChanged.AddListener(i =>
        {
            string role = roleDropdown.options[i].text;
            _data.OnRoleChanged?.Invoke(role);
        });

        videoDropdown.onValueChanged.AddListener(i =>
        {
            string video = videoDropdown.options[i].text;
            _data.OnVideoChanged?.Invoke(video);
        });

        assignButton.onClick.RemoveAllListeners();
        assignButton.onClick.AddListener(() =>
        {
            _data.onAssign?.Invoke(new PlayerRoomData
            {
                playerId = data.playerId,
                role = roleDropdown.options[roleDropdown.value].text,
                video = videoDropdown.options[videoDropdown.value].text
            });
            SavePlayerPrefData();
        });

        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(() =>
        {
            _data.onRemove?.Invoke(data.playerId);
        });

        flashButton.onClick.RemoveAllListeners();
        flashButton.onClick.AddListener(() =>
        {
            _data.onFlash?.Invoke(data.playerId);
        });
        LoadPlayerPrefData();
        //SavePlayerPrefData();
    }

    public void SavePlayerPrefData()
    {
        PlayerPrefs.SetString($"{_data.playerId}_role", roleDropdown.options[roleDropdown.value].text);
        PlayerPrefs.SetString($"{_data.playerId}_video", videoDropdown.options[videoDropdown.value].text);
    }

    public void LoadPlayerPrefData()
    {
        if (PlayerPrefs.HasKey($"{_data.playerId}_role"))
        {
            string role = PlayerPrefs.GetString($"{_data.playerId}_role");
            int roleIndex = _data.roleOptions.IndexOf(role);
            if (roleIndex != -1)
            {
                roleDropdown.value = roleIndex;
            }
        }

        if (PlayerPrefs.HasKey($"{_data.playerId}_video"))
        {
            string video = PlayerPrefs.GetString($"{_data.playerId}_video");
            int videoIndex = _data.videoOptions.IndexOf(video);
            if (videoIndex != -1)
            {
                videoDropdown.value = videoIndex;
            }
        }

    }
}



public struct PlayerRoomData
{
    public string playerId;
    public string role;
    public string video;

    public List<string> roleOptions;
    public List<string> videoOptions;

    public Action<PlayerRoomData> onAssign;
    public Action<string> onRemove;
    public Action<string> onFlash;

    public Action<string> OnRoleChanged;
    public Action<string> OnVideoChanged;
}
