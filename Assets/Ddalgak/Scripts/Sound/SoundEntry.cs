using System;
using UnityEngine;

namespace Ddalgak
{
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField]
        private string id;
        [SerializeField]
        private AudioClip clip;
        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 1f;

#if UNITY_EDITOR
        private AudioClip _lastSyncedClip;
#endif

        public string Id => id;
        public AudioClip Clip => clip;
        public float Volume => volume;

#if UNITY_EDITOR
        public void SyncWithClip()
        {
            if (clip == _lastSyncedClip)
            {
                return;
            }

            _lastSyncedClip = clip;

            if (clip == null)
            {
                return;
            }

            id = clip.name;
            volume = 1f;
        }
#endif
    }
}
