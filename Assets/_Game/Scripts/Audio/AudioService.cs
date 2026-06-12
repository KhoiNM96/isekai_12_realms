using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Audio
{
    public class AudioService : MonoBehaviour
    {
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioSource bgmSource;
        private AudioSource sfxSource;

        public bool MusicEnabled { get; private set; } = true;
        public bool SfxEnabled { get; private set; } = true;
        public float MusicVolume { get; private set; } = 0.7f;
        public float SfxVolume { get; private set; } = 0.85f;

        private void Awake()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            sfxSource = gameObject.AddComponent<AudioSource>();
            LoadSettings();
        }

        public void PlaySfx(string id)
        {
            if (!SfxEnabled || string.IsNullOrEmpty(id)) return;
            if (clips.TryGetValue(id, out AudioClip clip) && clip != null)
            {
                sfxSource.PlayOneShot(clip, SfxVolume);
            }
        }

        public void PlayBgm(string id)
        {
            if (!MusicEnabled || string.IsNullOrEmpty(id)) return;
            if (!clips.TryGetValue(id, out AudioClip clip) || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.volume = MusicVolume;
            bgmSource.Play();
        }

        public void StopBgm() => bgmSource?.Stop();
        public void SetMusicVolume(float value) { MusicVolume = Mathf.Clamp01(value); if (bgmSource != null) bgmSource.volume = MusicVolume; SaveSettings(); }
        public void SetSfxVolume(float value) { SfxVolume = Mathf.Clamp01(value); if (sfxSource != null) sfxSource.volume = SfxVolume; SaveSettings(); }
        public void MuteMusic(bool muted) { MusicEnabled = !muted; if (!MusicEnabled) StopBgm(); SaveSettings(); }
        public void MuteSfx(bool muted) { SfxEnabled = !muted; SaveSettings(); }

        private void LoadSettings()
        {
            MusicEnabled = PlayerPrefs.GetInt("music_enabled", 1) == 1;
            SfxEnabled = PlayerPrefs.GetInt("sfx_enabled", 1) == 1;
            MusicVolume = PlayerPrefs.GetFloat("music_volume", 0.7f);
            SfxVolume = PlayerPrefs.GetFloat("sfx_volume", 0.85f);
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt("music_enabled", MusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("sfx_enabled", SfxEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("music_volume", MusicVolume);
            PlayerPrefs.SetFloat("sfx_volume", SfxVolume);
            PlayerPrefs.Save();
        }
    }
}
