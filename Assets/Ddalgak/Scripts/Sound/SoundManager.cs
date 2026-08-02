using JxModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public sealed class SoundManager : GlobalSingleton<SoundManager>
    {
        [Header("BGM")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private List<SoundEntry> bgmClips = new();
        [SerializeField] private float bgmFadeDuration = 1f;

        [Header("SFX")]
        [SerializeField] private List<SoundEntry> sfxClips = new();
        [SerializeField] private int sfxSourcePoolSize = 8;

        [Header("Volume")]
        [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField][Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

        private readonly Dictionary<string, SoundEntry> _bgmLookup = new();
        private readonly Dictionary<string, SoundEntry> _sfxLookup = new();
        private readonly List<AudioSource> _sfxSources = new();
        private int _sfxSourceIndex;
        private Coroutine _bgmFadeCoroutine;
        private string _currentBgmId;

        public bool IsBgmMuted { get; private set; }
        public bool IsSfxMuted { get; private set; }
        public string CurrentBgmId => _currentBgmId;

        protected override void Awake()
        {
            base.Awake();

            BuildLookup(bgmClips, _bgmLookup);
            BuildLookup(sfxClips, _sfxLookup);

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }

            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            CreateSfxSourcePool();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SyncEntries(bgmClips);
            SyncEntries(sfxClips);
        }

        private static void SyncEntries(List<SoundEntry> entries)
        {
            foreach (SoundEntry entry in entries)
            {
                entry?.SyncWithClip();
            }
        }
#endif

        private static void BuildLookup(List<SoundEntry> entries, Dictionary<string, SoundEntry> lookup)
        {
            lookup.Clear();

            foreach (SoundEntry entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
                {
                    continue;
                }

                if (!lookup.TryAdd(entry.Id, entry))
                {
                    Debug.LogWarning($"[SoundManager] Duplicate sound id ignored: {entry.Id}");
                }
            }
        }

        private void CreateSfxSourcePool()
        {
            for (var i = 0; i < sfxSourcePoolSize; i++)
            {
                GameObject sourceObject = new($"SfxSource_{i}");
                sourceObject.transform.SetParent(transform);

                AudioSource source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;

                _sfxSources.Add(source);
            }
        }

        public void PlayBgm(string id, bool fade = true)
        {
            if (!_bgmLookup.TryGetValue(id, out SoundEntry entry))
            {
                Debug.LogWarning($"[SoundManager] BGM not found: {id}");
                return;
            }

            if (_currentBgmId == id && bgmSource.isPlaying)
            {
                return;
            }

            _currentBgmId = id;

            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }

            _bgmFadeCoroutine = StartCoroutine(fade ? CrossfadeBgm(entry) : SwapBgmImmediate(entry));
        }

        public void StopBgm(bool fade = true)
        {
            if (!bgmSource.isPlaying)
            {
                return;
            }

            _currentBgmId = null;

            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }

            _bgmFadeCoroutine = StartCoroutine(fade ? FadeOutAndStopBgm() : StopBgmImmediate());
        }

        private IEnumerator CrossfadeBgm(SoundEntry entry)
        {
            if (bgmSource.isPlaying)
            {
                yield return FadeBgmVolume(0f, bgmFadeDuration);
            }

            bgmSource.clip = entry.Clip;
            bgmSource.Play();
            yield return FadeBgmVolume(GetBgmOutputVolume(entry), bgmFadeDuration);
            _bgmFadeCoroutine = null;
        }

        private IEnumerator SwapBgmImmediate(SoundEntry entry)
        {
            bgmSource.clip = entry.Clip;
            bgmSource.volume = GetBgmOutputVolume(entry);
            bgmSource.Play();
            yield return null;
            _bgmFadeCoroutine = null;
        }

        private IEnumerator FadeOutAndStopBgm()
        {
            yield return FadeBgmVolume(0f, bgmFadeDuration);
            bgmSource.Stop();
            bgmSource.clip = null;
            _bgmFadeCoroutine = null;
        }

        private IEnumerator StopBgmImmediate()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            yield return null;
            _bgmFadeCoroutine = null;
        }

        private IEnumerator FadeBgmVolume(float targetVolume, float duration)
        {
            var startVolume = bgmSource.volume;

            if (duration <= 0f)
            {
                bgmSource.volume = targetVolume;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            bgmSource.volume = targetVolume;
        }

        public void PlaySfx(string id)
        {
            if (IsSfxMuted)
            {
                return;
            }

            if (!_sfxLookup.TryGetValue(id, out SoundEntry entry) || entry.Clip == null)
            {
                Debug.LogWarning($"[SoundManager] SFX not found: {id}");
                return;
            }

            AudioSource source = GetAvailableSfxSource();
            source.clip = entry.Clip;
            source.volume = entry.Volume * sfxVolume * masterVolume;
            source.Play();
        }

        private AudioSource GetAvailableSfxSource()
        {
            foreach (AudioSource source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            AudioSource reused = _sfxSources[_sfxSourceIndex];
            _sfxSourceIndex = (_sfxSourceIndex + 1) % _sfxSources.Count;
            return reused;
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyCurrentBgmVolume();
        }

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            ApplyCurrentBgmVolume();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
        }

        public void SetBgmMuted(bool muted)
        {
            IsBgmMuted = muted;
            bgmSource.mute = muted;
        }

        public void SetSfxMuted(bool muted)
        {
            IsSfxMuted = muted;
        }

        private void ApplyCurrentBgmVolume()
        {
            if (_bgmFadeCoroutine != null || _currentBgmId == null)
            {
                return;
            }

            if (_bgmLookup.TryGetValue(_currentBgmId, out SoundEntry entry))
            {
                bgmSource.volume = GetBgmOutputVolume(entry);
            }
        }

        private float GetBgmOutputVolume(SoundEntry entry)
        {
            return entry.Volume * bgmVolume * masterVolume;
        }
    }
}
