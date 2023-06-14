
namespace DUJAL.Systems.Audio
{
    using UnityEngine;
    /// <summary>
    //  Scriptable Object that contains AudioClip, Audio Source and several params to manage these.
    /// </summary>
    [CreateAssetMenu(fileName = "New Sound Asset", menuName = "DUJAL/Sound")]
    public class Sound : ScriptableObject
    {
        [Header("Sound Scriptable Object Parameters")]
        [Tooltip("Audio Clip asset that contains the actual sound.")]
        public AudioClip clip;

        [Tooltip("Volume for this sound.")]
        [Range(0f, 1f)]
        public float volume;

        [Tooltip("Pitch for this sound.")]
        [Range(.1f, 3f)]
        public float pitch;

        [Tooltip("Select this if you want to loop this sound.")]
        public bool loop;

        [Tooltip("Select this if this sound is a Sound Effect, Unselect it if you want it to be considered as Music.")]
        public bool sfx;

        [HideInInspector]
        public AudioSource audioSource;

        [HideInInspector]
        public float time;
    }
}