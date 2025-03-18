using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioInfo {
    public AudioClip clip;
    public string name;
    public float volumeAdjustment;
    public float startTime;
}

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource musicTrack1;
    public AudioSource musicTrack2;
    public AudioSource effectsTrack;

    [Header("Settings")]
    public float fadeSpeed = 1.0f;

    [Header("Clips")]
    public List<AudioInfo> musicClips;
    public List<AudioInfo> soundEffectClips;


    [NonSerialized]
    public static AudioManager instance;
    private bool isPlayingTrack1 = true;
    private AudioInfo currentMusicInfo;

    void Awake()
    {
        if (instance == null) instance = this;
        // DontDestroyOnLoad(this);
    }

    public void CrossfadeMusic(string newMusicName, float delay = 0) {
        if (newMusicName == currentMusicInfo?.name)
        {
            Debug.Log($"Already playing ${newMusicName}. Continuing playback...");
            return;
        }

        #nullable enable
        AudioInfo? infoOrNull = musicClips.FirstOrDefault(a => a.name == newMusicName || a.clip.name == newMusicName);
        if (infoOrNull == null) {
            Debug.LogWarning($"Couldn't find music: {newMusicName}");
            return;
        }
        AudioInfo audioInfo = infoOrNull;
        #nullable disable

        StopAllCoroutines();

        if (isPlayingTrack1)
        {
            if (currentMusicInfo != null) currentMusicInfo.startTime = musicTrack1.time;
            StartCoroutine(FadeOut(musicTrack1));

            musicTrack2.clip = audioInfo.clip;
            StartCoroutine(FadeIn(musicTrack2, audioInfo.startTime, 1.0f + audioInfo.volumeAdjustment, delay));

            isPlayingTrack1 = false;
        }
        else {
            if (currentMusicInfo != null) currentMusicInfo.startTime = musicTrack2.time;
            StartCoroutine(FadeOut(musicTrack2));

            musicTrack1.clip = audioInfo.clip;
            StartCoroutine(FadeIn(musicTrack1, audioInfo.startTime, 1.0f + audioInfo.volumeAdjustment, delay));

            isPlayingTrack1 = true;
        }

        currentMusicInfo = audioInfo;
    }
    public void StopMusic()
    {
        StartCoroutine(FadeOut(isPlayingTrack1 ? musicTrack1 : musicTrack2));
    }

    public void PlaySFX(string sfxName, Vector3? position = null)
    {
        if (effectsTrack.isPlaying) StopSFX();

        AudioInfo? infoOrNull = soundEffectClips.FirstOrDefault(a => a.name == sfxName || a.clip.name == sfxName); 
        if (infoOrNull == null) {
            Debug.LogWarning($"Couldn't find SFX: {sfxName}");
            return;
        }
        AudioInfo audioInfo = infoOrNull;

        audioMixer.SetFloat("EffectVolume", 1.0f + audioInfo.volumeAdjustment);

        effectsTrack.clip = audioInfo.clip;
        effectsTrack.time = audioInfo.startTime;

        if (position != null)
        {
            effectsTrack.transform.position = (Vector3)position;
            effectsTrack.spatialBlend = 1.0f;

        } else
        {
            effectsTrack.spatialBlend = 0;
        }

        effectsTrack.Play();
    }
    public void StopSFX()
    {
        if (effectsTrack.isPlaying) effectsTrack.Stop();
    }

    public void MuffleMusic()
    {
        audioMixer.SetFloat("LowpassCutoff", 600);
    }
    public void UnmuffleMusic()
    {
        audioMixer.SetFloat("LowpassCutoff", 22000);
    }
    public void ResetMusicEffects()
    {
        UnmuffleMusic();
        UnslowMusic();
    }
    public void SlowMusic()
    {
        musicTrack1.pitch = .75f;
        musicTrack2.pitch = .75f;
    }
    public void UnslowMusic()
    {
        musicTrack1.pitch = 1.0f;
        musicTrack2.pitch = 1.0f;
    }

    public void MuteVoiceovers()
    {
        audioMixer.SetFloat("VoiceoverVolume", -80);
    }
    public void UnmuteVoiceovers()
    {
        audioMixer.SetFloat("VoiceoverVolume", 0);
    }

    IEnumerator FadeOut(AudioSource audioSource, float delay = 0)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        while (audioSource.volume > 0)
        {
            audioSource.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        audioSource.Stop();
        yield return null;
    }

    IEnumerator FadeIn(AudioSource audioSource, float startTime = 0, float maxVolume = 1f, float delay = 0)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        audioSource.volume = 0;

        if (startTime > 0) audioSource.time = startTime;
        audioSource.Play();

        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        yield return null;
    }
}