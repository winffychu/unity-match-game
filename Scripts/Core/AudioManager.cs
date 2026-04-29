using UnityEngine;
using System.Collections.Generic;

namespace MemoryMatch.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip flipSound;
        [SerializeField] private AudioClip matchSuccessSound;
        [SerializeField] private AudioClip matchFailSound;
        [SerializeField] private AudioClip levelCompleteSound;
        [SerializeField] private AudioClip levelFailSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip menuBGM;
        [SerializeField] private AudioClip gameBGM;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            UpdateVolumes();
        }

        public void PlayBGM()
        {
            if (bgmSource != null && gameBGM != null)
            {
                bgmSource.clip = gameBGM;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void PlayMenuBGM()
        {
            if (bgmSource != null && menuBGM != null)
            {
                bgmSource.clip = menuBGM;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

        public void PlayFlipSound()
        {
            PlaySFX(flipSound);
        }

        public void PlayMatchSuccessSound()
        {
            PlaySFX(matchSuccessSound);
        }

        public void PlayMatchFailSound()
        {
            PlaySFX(matchFailSound);
        }

        public void PlayLevelCompleteSound()
        {
            PlaySFX(levelCompleteSound);
        }

        public void PlayLevelFailSound()
        {
            PlaySFX(levelFailSound);
        }

        public void PlayButtonClickSound()
        {
            PlayUI(buttonClickSound);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
                sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        private void PlayUI(AudioClip clip)
        {
            if (uiSource != null && clip != null)
                uiSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (bgmSource != null)
                bgmSource.volume = bgmVolume * masterVolume;
        }
    }
}