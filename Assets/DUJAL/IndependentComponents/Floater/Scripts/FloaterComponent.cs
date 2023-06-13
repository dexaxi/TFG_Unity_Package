using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DUJAL.IndependentComponents.Floater
{
    /// <summary>
    //  Unity Component to make objects float and move in the 3 axis.
    /// </summary>
    public class FloaterComponent : MonoBehaviour
    {
        [Header("Activation Bools")]
        [SerializeField]
        [Tooltip("Activate the object's rotation.")]
        private bool activateRotation;

        [SerializeField]
        [Tooltip("Activate the object's translation.")]
        private bool activateTranslation;

        [Space(15)]

        [Header("Randomization Parameters")]
        [SerializeField]
        [Tooltip("Randomize the object's rotation period by .2 seconds (use these when rotating several objects, only usable at start).")]
        private bool enableRandomRotation;

        [SerializeField]
        [Range(0.8f, 1.2f)]
        [Tooltip("Interval -x to x in which the rotation will be randomized")]
        private float rotationVariation;

        [SerializeField]
        [Tooltip("Ranzomize the object's translation period by .2 seconds (use these when translating several objects, only usable at start).")]
        private bool enableRandomTranslation;

        [SerializeField]
        [Range(0f, 0.5f)]
        [Tooltip("Interval -x to x in which the frequency of the translation will be randomized.")]
        private float translationVariation;

        [Space(15)]

        [Header("Rotation Parameters")]
        [SerializeField]
        [Tooltip("Set degrees per second in all axis, set zero to disable that axis' rotation.")]
        private Vector3 degreesPerSecond;

        [Space(15)]

        [Header("Translation Parameters")]

        [SerializeField]
        [Tooltip("Set amplitude of translation in all axis, set to zero to disable that axis translation.")]
        private Vector3 amplitude;

        [SerializeField]
        [Tooltip("Set translation frequency in all axis, (if amplitude is set to zero, this value will not matter).")]
        private Vector3 frequency;

        [Space(15)]

        [Header("Position Parameters")]
        [SerializeField]
        [Tooltip("Offset added to position, can be modified by SetOffset while in execution to move the object while maintaining the local translation.")]
        private Vector3 offset;

        [Space(15)]

        [Header("Debug Controls")]
        [SerializeField]
        [Tooltip("Check to reset rotation to (0,0,0) while running the game.")]
        private bool resetRotation;

        private Vector3 _posOffset;
        private Vector3 _tempPos;

        private Vector3 _degreesPerSecond;
        private Vector3 _frequency;
        private void Start()
        {
            _posOffset = transform.position + offset;
            if (enableRandomRotation)
            {
                _degreesPerSecond.x = degreesPerSecond.x * Random.Range(-rotationVariation, rotationVariation);
                _degreesPerSecond.y = degreesPerSecond.y * Random.Range(-rotationVariation, rotationVariation);
                _degreesPerSecond.z = degreesPerSecond.z * Random.Range(-rotationVariation, rotationVariation);
            }
            else
            {
                _degreesPerSecond = degreesPerSecond;
            }
            if (enableRandomTranslation)
            {
                _frequency = frequency + new Vector3(Random.Range(-translationVariation, translationVariation),
                    Random.Range(-translationVariation, translationVariation),
                    Random.Range(-translationVariation, translationVariation));
            }
            else
            {
                _frequency = frequency;
            }
        }

        void Update()
        {


            if (activateRotation)
            {
                transform.Rotate(new Vector3(Time.deltaTime * _degreesPerSecond.x, Time.deltaTime * _degreesPerSecond.y, Time.deltaTime * _degreesPerSecond.z), Space.World);
                //DEBUG
                if (resetRotation)
                {
                    resetRotation = false;
                    transform.rotation = Quaternion.identity;

                }
            }

            if (activateTranslation)
            {
                _tempPos = _posOffset + offset;

                _tempPos.x += Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency.x) * amplitude.x;
                _tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency.y) * amplitude.y;
                _tempPos.z += Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency.z) * amplitude.z;

                transform.position = _tempPos;
            }
        }

        public void SetOffset(Vector3 newOffset)
        {
            this.offset = newOffset;
        }

        public void ToggleRotation()
        {
            this.activateRotation = !this.activateRotation;
        }
        public void ToggleTranslation()
        {
            this.activateTranslation = !this.activateTranslation;
        }

        public void SetDegreesPerSecond(Vector3 degreesPerSecond)
        {
            this.degreesPerSecond = degreesPerSecond;
        }

        public void SetAmplitude(Vector3 amplitude)
        {
            this.amplitude = amplitude;
        }

        public void SetFrequency(Vector3 frequency)
        {
            this.frequency = frequency;
        }

        public void ResetRotation()
        {
            this.resetRotation = true;
        }

        public Vector3 GetOffset() { return offset; }

        public Vector3 GetDegreesPerSecond() { return degreesPerSecond; }
        public Vector3 GetAmplitude() { return amplitude; }
        public Vector3 GetFrequency() { return frequency; }
    }
}
