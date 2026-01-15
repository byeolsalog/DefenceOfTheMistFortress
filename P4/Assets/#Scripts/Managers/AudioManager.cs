using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager
{
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    private bool _initialized = false;

    private Dictionary<EBGM, AudioClip> _bgmClips = new Dictionary<EBGM, AudioClip>();
    private Dictionary<string, AudioClip> _sfxClips = new Dictionary<string, AudioClip>();

    public void Init()
    {
        if (_initialized) return;
        _initialized = true;

        GameObject root = GameManager.Instance.gameObject;
        var data = GameManager.Data.OptionData;

        GameObject bgmObj = new GameObject("BGM_AudioSource");
        bgmObj.transform.SetParent(root.transform);
        _bgmSource = bgmObj.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = data.bgmVolume;

        var titleBGM = Resources.Load<AudioClip>("TitleBGM");
        _bgmClips.Add(EBGM.TitleBGM, titleBGM);
        PlayBGM(EBGM.TitleBGM);

        GameObject sfxObj = new GameObject("SFX_AudioSource");
        sfxObj.transform.SetParent(root.transform);
        _sfxSource = sfxObj.AddComponent<AudioSource>();

        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = data.sfxVolume;
    }

    #region Load
    public void LoadBGMs(IReadOnlyList<AudioClip> clips)
    {
        if (clips == null || clips.Count <= 0)
            return;
        foreach (var clip in clips)
        {
            if (clip == null) continue;
            if (System.Enum.TryParse<EBGM>(clip.name, out var key))
            {
                if (_bgmClips.ContainsKey(key)) continue;
                _bgmClips.Add(key, clip);
            }
        }
    }

    public void LoadSFXs(IReadOnlyList<AudioClip> clips)
    {
        if (clips == null || clips.Count <= 0)
            return;

        foreach (var clip in clips)
        {
            if (clip == null) continue;
            if (_sfxClips.ContainsKey(clip.name)) continue;

            _sfxClips.Add(clip.name, clip);
        }
    }

    #endregion

    #region Play

    public void PlayBGM(EBGM key)
    {
        if (!_bgmClips.TryGetValue(key, out var clip))
        {
            Debug.LogWarning($"[AudioManager] BGM not found : {key}");
            return;
        }

        if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            return;

        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    public void StopBGM()
    {
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    public void PlaySFX(string key)
    {
        if (!_sfxClips.TryGetValue(key, out var clip))
        {
            Debug.LogWarning($"[AudioManager] SFX not found : {key}");
            return;
        }

        _sfxSource.PlayOneShot(clip);
    }

    #endregion

    #region Volume

    public void SetBGMVolume(float volume)
    {
        _bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxSource.volume = Mathf.Clamp01(volume);
    }

    #endregion
}