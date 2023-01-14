using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloaterComponent : MonoBehaviour
{
    [Header("Control Bools")]
    [SerializeField]
    [Tooltip("Activate the object's rotation")]
    private bool activateRotation;

    [SerializeField]
    [Tooltip("Activate the object's translation")]
    private bool activateTranslation;

    [Header("Rotation Parameters")]
    [SerializeField]
    [Tooltip("Set degrees per second in all axis, set zero to disable that axis' rotation")]
    private Vector3 degreesPerSecond;

    [Header("Translation Parameters")]

    [SerializeField]
    [Tooltip("Set amplitude of translation in all axis, set to zero to disable that axis translation")]
    private Vector3 amplitude;

    [SerializeField]
    [Tooltip("Set translation frequency in all axis, (if amplitude is set to zero, this value will not matter)")]
    private Vector3 frequency;

    [Header("Position Parameters")]
    [SerializeField]
    [Tooltip("Offset added to position, can be modified by SetOffset while in execution to move the object while maintaining the local translation")]
    private Vector3 offset;

    [Header("Debug Controls")]
    [SerializeField]
    [Tooltip("Check to reset rotation to (0,0,0) while running the game")]
    private bool resetRotation;

    private Vector3 posOffset;
    private Vector3 tempPos;

    private void Start()
    {
        posOffset = transform.position + offset;
    }

    void Update()
    {
        if (activateRotation)
        {
            transform.Rotate(new Vector3(Time.deltaTime * degreesPerSecond.x, Time.deltaTime * degreesPerSecond.y, Time.deltaTime * degreesPerSecond.z), Space.World);
            //DEBUG
            if (resetRotation)
            {
                resetRotation = false;
                transform.rotation = Quaternion.identity;

            }
        }

        if (activateTranslation)
        {
            tempPos = posOffset + offset;

            tempPos.x += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency.x) * amplitude.x;
            tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency.y) * amplitude.y;
            tempPos.z += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency.z) * amplitude.z;

            transform.position = tempPos;
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
