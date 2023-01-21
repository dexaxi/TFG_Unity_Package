using UnityEngine;
using System;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Sound[] sounds;

    public Sound[] randomMusicList;

    [SerializeField] 
    AudioMixerGroup music;
    
    [SerializeField] 
    AudioMixerGroup sfx;

    private int randomSoundIndex;
    private float lastRandomSoundTime;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        foreach (Sound sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
            if (sound.sfx)
            {
                sound.audioSource.outputAudioMixerGroup = sfx;
            }
            else 
            { 
                sound.audioSource.outputAudioMixerGroup = music;
            }
        }
    }

    public Sound FindSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name.ToLower() == name.ToLower());
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        return s;
    }

    public float GetSoundLength(string name) 
    {
        Sound currentSound = FindSound(name);
        if(currentSound != null)
        {
            return currentSound.clip.length;
        }
        return -1;
    }

    public float GetSongProgress(string name)
    {
        Sound currentSound = FindSound(name);
        if (currentSound != null) 
        {
            float length = currentSound.clip.length;
            float time = currentSound.audioSource.time;
            return (time / length) * 100;
        }
        return -1;
    }

    public bool IsPlaying(string name) 
    {
        Sound currentSound = FindSound(name);
        if (currentSound != null) 
        {
            return currentSound.audioSource.isPlaying;
        }
        return false;
    }

    public void Play(string name)
    {
        Sound currentSound = FindSound(name);
        if (currentSound != null) 
        {
            currentSound.audioSource.Play();
        }
    }

    public void PlayRandomMusic() 
    {
        randomMusicList[randomSoundIndex].audioSource.Stop();
        lastRandomSoundTime = 0f;
        randomSoundIndex = UnityEngine.Random.Range(0, randomMusicList.Length);
        Play(randomMusicList[randomSoundIndex].name);
    }

    public void PauseLastRandomMusicClip() 
    {
        Sound currentSound = randomMusicList[randomSoundIndex];
        lastRandomSoundTime = currentSound.audioSource.time;
        randomMusicList[randomSoundIndex].audioSource.Pause();
    }
    public void StopLastRandomMusicClip()
    {
        Sound currentSound = randomMusicList[randomSoundIndex];
        randomMusicList[randomSoundIndex].audioSource.Stop();
    }

    public void ResumeLastRandomMusicClip()
    {
        Sound currentSound = randomMusicList[randomSoundIndex];
        randomMusicList[randomSoundIndex].audioSource.Play();
        if (lastRandomSoundTime != 0 && lastRandomSoundTime != -1) 
        {
            currentSound.audioSource.time = lastRandomSoundTime;
        }
    }

    public void Pause(string name)
    {
        Sound currentSound = FindSound(name);
        if (currentSound != null) 
        {
            currentSound.time = currentSound.audioSource.time;
            currentSound.audioSource.Pause();    
        }
    }

    public void Resume(string name)
    {
        Sound currentSound = FindSound(name);
        if (currentSound != null) 
        {
            currentSound.audioSource.Play();
            if (currentSound.time != 0 && currentSound.time != -1)
            {
                currentSound.audioSource.time = currentSound.time;
            }
        }
    }

    public void Stop(string name)
    {
        Sound currentSound = FindSound(name);
        if (currentSound != null) 
        {
            currentSound.audioSource.Stop();
        }
    }

    public void StopAllMusic(){
        foreach(Sound sound in sounds)
        {
            if (!sound.sfx)
            {
                if (sound.audioSource != null) 
                { 
                    sound.audioSource.Stop();
                }
            }
        }
    }

    public void StopAllSFX() 
    {
        foreach (Sound sound in sounds)
        {
            if (sound.sfx)
            {
                if (sound.audioSource != null)
                {
                    sound.audioSource.Stop();
                }
            }
        }
    }

    public void StopAllAudio() 
    {
        foreach (Sound sound in sounds)
        {
            if (sound.audioSource != null)
            {
                sound.audioSource.Stop();
            }
        }
    }

    public void PauseAllMusic()
    {
        foreach (Sound sound in sounds)
        {
            if (!sound.sfx)
            {
                if (sound.audioSource != null)
                {
                    sound.time = sound.audioSource.time;
                    sound.audioSource.Pause();
                }
            }
        }
    }

    public void PauseAllSFX()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.sfx)
            {
                if (sound.audioSource != null)
                {
                    sound.time = sound.audioSource.time;
                    sound.audioSource.Pause();
                }
            }
        }
    }

    public void PauseAllAudio()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.audioSource != null)
            {
                sound.time = sound.audioSource.time;
                sound.audioSource.Stop();
            }
        }
    }

    public void ResumeAllPausedMusic()
    {
        foreach (Sound sound in sounds)
        {
            if (!sound.sfx)
            {
                if (sound.audioSource != null)
                {
                    if (sound.time != 0 && sound.time != -1)
                    {
                        sound.audioSource.Play();
                        sound.audioSource.time = sound.time;
                    }
                }
            }
        }
    }

    public void ResumeAllPausedSFX()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.sfx)
            {
                if (sound.audioSource != null)
                {
                    if (sound.time != 0 && sound.time != -1)
                    {
                        sound.audioSource.Play();
                        sound.audioSource.time = sound.time;
                    }
                }
            }
        }
    }

    public void ResumeAllPausedAudio()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.audioSource != null)
            {
                if (sound.time != 0 && sound.time != -1)
                {
                    sound.audioSource.Play();
                    sound.audioSource.time = sound.time;
                }
            }
        }
    }
}