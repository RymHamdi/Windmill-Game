using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Cleanup : MonoBehaviour
{
    public List<VideoPlayer> videoPlayers;
    public List<RenderTexture> renderTextures;

    private void OnSceneUnloaded(Scene current)
    {
        for (int i = 0; i < videoPlayers.Count; i++)
        {
            VideoPlayer item = videoPlayers[i];
            item.Stop();
            item.clip = null;
            item.targetTexture = null;
            Destroy(item.gameObject);
        }

        for (int i = 0; i < renderTextures.Count; i++)
        {
            RenderTexture item = renderTextures[i];
            item.Release();
            Destroy(item);
        }

        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
