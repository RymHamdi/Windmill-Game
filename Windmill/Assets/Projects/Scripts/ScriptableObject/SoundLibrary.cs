using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundItem
    {
        public string name;
        public AudioClip clip;
        public float volume = 1f;
    }

    public List<SoundItem> sounds = new List<SoundItem>();
}
