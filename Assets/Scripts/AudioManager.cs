using UnityEngine;

// A simple persistent audio hub: one looping channel for music, one
// looping channel for ambience, a shared one-shot channel for SFX, and a
// voice channel for the story voiceovers.
// Survives scene loads (DontDestroyOnLoad), so Chapter1 -> Chapter2 can
// either keep music playing or deliberately swap it.
//
// Nothing needs to be added to any scene by hand -- call AudioManager.Get()
// from anywhere and it creates itself the first time.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 1f;

    AudioSource music;
    AudioSource ambience;
    AudioSource sfx;
    AudioSource voice;

    public static AudioManager Get()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>(); // Awake() runs immediately and sets Instance
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        music.volume = musicVolume;

        ambience = gameObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.volume = ambienceVolume;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.volume = sfxVolume;

        voice = gameObject.AddComponent<AudioSource>();
        voice.loop = false;
        voice.playOnAwake = false;
        voice.volume = voiceVolume;
    }

    // ---------- Music ----------

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) { music.Stop(); return; }
        if (music.clip == clip && music.isPlaying) return;
        music.clip = clip;
        music.volume = musicVolume;
        music.Play();
    }

    public void StopMusic() => music.Stop();

    // ---------- Ambience ----------

    public void PlayAmbience(AudioClip clip)
    {
        if (clip == null) { ambience.Stop(); return; }
        if (ambience.clip == clip && ambience.isPlaying) return;
        ambience.clip = clip;
        ambience.volume = ambienceVolume;
        ambience.Play();
    }

    public void StopAmbience() => ambience.Stop();

    // ---------- Voice ----------

    // The story voiceover read over the chapter banner. Plays once.
    public void PlayVoice(AudioClip clip)
    {
        voice.Stop();
        if (clip == null) return;
        voice.clip = clip;
        voice.volume = voiceVolume;
        voice.Play();
    }

    public void StopVoice() => voice.Stop();

    // For the pause menu: pause and carry on from the same spot.
    public void PauseVoice(bool pause)
    {
        if (pause) voice.Pause();
        else voice.UnPause();
    }

    public bool IsVoicePlaying => voice.isPlaying;

    // ---------- SFX ----------

    // Safe to call with a null clip (e.g. before you've assigned real
    // audio yet) -- it just does nothing.
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfx.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // ---------- Volume ----------
    // Wire these up to a settings menu later if you want one.

    public void SetMusicVolume(float v) { musicVolume = Mathf.Clamp01(v); music.volume = musicVolume; }
    public void SetAmbienceVolume(float v) { ambienceVolume = Mathf.Clamp01(v); ambience.volume = ambienceVolume; }
    public void SetSfxVolume(float v) { sfxVolume = Mathf.Clamp01(v); }
}