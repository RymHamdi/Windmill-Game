using UnityEngine;
using Photon.Chat;
using ExitGames.Client.Photon;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevicePhotonChat : MonoBehaviour, IChatClientListener
{
    public string appIdChat = "YOUR_PHOTON_CHAT_APP_ID";
    public string channelName = "event-001";

    public string deviceIdInEvent;   // e.g. Device1, or Tablet_01
    private string deviceRoleInEvent = "";
    private string deviceVideoInEvent = "";

    public Text statusText;



    ChatClient chat;

    void OnEnable()
    {
        
         if (PlayerPrefs.HasKey("assigned_role"))
        {
            deviceRoleInEvent = PlayerPrefs.GetString("assigned_role");
        }
        if (PlayerPrefs.HasKey("assigned_video"))
        {
            deviceVideoInEvent = PlayerPrefs.GetString("assigned_video");
        }

        Debug.Log("Device Role: " + deviceRoleInEvent);
        Debug.Log("Device Video: " + deviceVideoInEvent);
    }

    void Start()
    {
        Connect();

       
    }

    void Update()
    {
        if (chat != null)
            chat.Service(); // REQUIRED for Photon Chat
    }

    void Connect()
    {
        chat = new ChatClient(this);
        chat.Connect(appIdChat, "1.0", new AuthenticationValues(deviceIdInEvent));
        statusText.text = "Connecting to chat...";
    }

    public void OnConnected()
    {
        Debug.Log("Chat Connected!");
        bool subscribed = chat.Subscribe(new string[] { channelName });
        statusText.text = "Connected to chat!";
       Invoke("SendMyId", 1.0f);
        
    }

    private void SendMyId()
    {
        var registerMsg = new Command {
            command = "register",
            target = "all",
            deviceId = deviceIdInEvent,
            name = SystemInfo.deviceName,
            role = deviceRoleInEvent,
            video = deviceVideoInEvent
        };
        statusText.text = "Registering device...";
        
        chat.PublishMessage(channelName, JsonUtility.ToJson(registerMsg));
    }

    public void OnChatStateChange(ChatState state) {}

    public void OnSubscribed(string[] channels, bool[] results) {}

    public void OnDisconnected() {
        Debug.Log("Chat Disconnected!");
        statusText.text = "Disconnected from chat.";

    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        foreach (var msg in messages)
            ProcessCommand(msg);
    }

    void ProcessCommand(object msgObj)
    {
        Debug.Log("Received message: " + msgObj.ToString());
        var json = msgObj.ToString();

        var cmd = JsonUtility.FromJson<Command>(json);
        if (cmd == null) return;

        

        Debug.Log("Command: " + cmd.command);

        switch (cmd.command)
        {
            case "assignConfig":
                PlayerPrefs.SetString("assigned_role", cmd.role);
                PlayerPrefs.SetString("assigned_video", cmd.video);
                statusText.text = "Configuration assigned." + cmd.role + " / " + cmd.video;
                PlayerPrefs.Save();
                break;

            case "showWho":
                StartCoroutine(FlashScreen());
                break;

            case "startGame":
                statusText.text = "Starting game...";
                //SceneManager.LoadScene("GameScene");
                break;

            case "removeDevice":
           chat.Disconnect();
                statusText.text = "Removing device...";
                Application.Quit();
                break;
        }
    }

    [Serializable]
    public class Command {
        public string target;
        public string command;
        public string deviceId;
        public string name;
        public string role;
        public string video;
    }

    System.Collections.IEnumerator FlashScreen()
    {
        Debug.Log("Flash = ShowWho");
        // TODO: highlight UI for 1 second
        statusText.text = "Flashing screen...";
        yield return null;
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        ProcessCommand(message);
    }
    public void OnUnsubscribed(string[] channels) {}
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) {}
    public void OnUserSubscribed(string channel, string user) {}
    public void OnUserUnsubscribed(string channel, string user) {}

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log("PhotonChat: " + message);
    }

    void OnDisable()
    {
        if (chat != null)
        {
            chat.Disconnect();
        }
    }
}
