namespace DUJAL.IndependentComponents.LookAtCamera
{ 
    using UnityEngine;

    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 _posOffset;
        [SerializeField] private Vector3 _rotOffset;
    
        void Start()
        {
            transform.position = transform.position + _posOffset;
        }

        private void Update()
        {
            var camera = Camera.main;
            transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
            transform.Rotate(_rotOffset);
        }
    }
}