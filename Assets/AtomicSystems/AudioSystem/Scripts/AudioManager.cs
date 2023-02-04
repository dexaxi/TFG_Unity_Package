using System;
using UnityEngine.Audio;

namespace DUJAL.Systems.Audio
{
    using UnityEngine;

    /// <summary>
    //  Class that contains an Audio System to play music and Sound Effects in your game.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sound Scriptable Object Arrays")]
        [SerializeField]
        [Tooltip("Sound Array that contains all of the sound scriptable objects you want to play in the game.")]
        private Sound[] sounds;

        [Tooltip("List of sounds that you can use to play any one of them.")]
        [SerializeField]
        private Sound[] randomMusicList;

        [Header("Audio Mixer Groups")]
        [SerializeField]
        [Tooltip("Audio Mixer Group Asset to mix the music in the game.")]
        private AudioMixerGroup music;

        [Tooltip("Audio Mixer Group Asset to mix the sound effects in the game.")]
        [SerializeField]
        private AudioMixerGroup sfx;

        private int _randomSoundIndex;
        private float _lastRandomSoundTime;

        private void Awake()
        {
            //Make Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            //for each sound's audiosource, we modify it to contain the data in the Sound Scriptable Object
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

        /// <summary>
        //  Function that receives the name (Case Insensitive) of a Sound Scriptable Object and looks for it in the sound array. If not found, will return null. 
        /// </summary>
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

        /// <summary>
        //  Function that receives a Sound Scriptable Object's name and returns its length in seconds.
        /// </summary>
        public float GetSoundLength(string name)
        {
            Sound currentSound = FindSound(name);
            if (currentSound != null)
            {
                return currentSound.clip.length;
            }
            return -1;
        }

        /// <summary>
        //   Function that receives a Sound Scriptable Object's name and returns its progress as a percentage (0-100).
        /// </summary>
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

        /// <summary>
        //   Function that receives a Sound Scriptable Object's name and returns if it's playing.
        /// </summary>
        public bool IsPlaying(string name)
        {
            Sound currentSound = FindSound(name);
            if (currentSound != null)
            {
                return currentSound.audioSource.isPlaying;
            }
            return false;
        }

        /// <summary>
        //  Void function that receives a Sound Scriptable Object's name and plays it.
        /// </summary>
        public void Play(string name)
        {
            Sound currentSound = FindSound(name);
            if (currentSound != null)
            {
                currentSound.audioSource.Play();
            }
        }

        /// <summary>
        //  Void function that receives a Sound Scriptable Object's name and pauses it.
        /// </summary>
        public void Pause(string name)
        {
            Sound currentSound = FindSound(name);
            if (currentSound != null)
            {
                currentSound.time = currentSound.audioSource.time;
                currentSound.audioSource.Pause();
            }
        }

        /// <summary>
        //  Void function that receives a Sound Scriptable Object's name and resumes it.
        /// </summary>
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

        /// <summary>
        //  Void function that receives a Sound Scriptable Object's name and stops it.
        /// </summary>
        public void Stop(string name)
        {
            Sound currentSound = FindSound(name);
            if (currentSound != null)
            {
                currentSound.audioSource.Stop();
            }
        }

        /// <summary>
        //  Void function that plays a random element from the random music list.
        /// </summary>
        public void PlayRandomMusic()
        {
            randomMusicList[_randomSoundIndex].audioSource.Stop();
            _lastRandomSoundTime = 0f;
            _randomSoundIndex = UnityEngine.Random.Range(0, randomMusicList.Length);
            Play(randomMusicList[_randomSoundIndex].name);
        }

        /// <summary>
        //  Void function that pauses the last element played from the random music list.
        /// </summary>
        public void PauseLastRandomMusicClip()
        {
            Sound currentSound = randomMusicList[_randomSoundIndex];
            _lastRandomSoundTime = currentSound.audioSource.time;
            randomMusicList[_randomSoundIndex].audioSource.Pause();
        }

        /// <summary>
        //  Void function that stops the last element played from the random music list.
        /// </summary>
        public void StopLastRandomMusicClip()
        {
            Sound currentSound = randomMusicList[_randomSoundIndex];
            randomMusicList[_randomSoundIndex].audioSource.Stop();
        }


        /// <summary>
        //  Void function that resumes the last element played from the random music list.
        /// </summary>
        public void ResumeLastRandomMusicClip()
        {
            Sound currentSound = randomMusicList[_randomSoundIndex];
            randomMusicList[_randomSoundIndex].audioSource.Play();
            if (_lastRandomSoundTime != 0 && _lastRandomSoundTime != -1)
            {
                currentSound.audioSource.time = _lastRandomSoundTime;
            }
        }

        /// <summary>
        //  Void function that stops all current music (NOT SFX).
        /// </summary>
        public void StopAllMusic()
        {
            foreach (Sound sound in sounds)
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

        /// <summary>
        //  Void function that stops all sound effects (NOT MUSIC).
        /// </summary>
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

        /// <summary>
        //  Void function that stops all sounds.
        /// </summary>
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

        /// <summary>
        //  Void function that pauses all music (NOT SFX).
        /// </summary>
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

        /// <summary>
        //  Void function that pauses all Sound Effects (NOT MUSIC).
        /// </summary>
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

        /// <summary>
        //  Void function that pauses all sounds.
        /// </summary>
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

        /// <summary>
        //  Void function that resumes all paused music (NOT SFX).
        /// </summary>
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

        /// <summary>
        //  Void function that resumes all paused SFX (NOT MUSIC).
        /// </summary>
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

        /// <summary>
        //  Void function that resumes all sounds.
        /// </summary>
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
}