namespace DUJAL.MovementComponents.PhysicsBased3D 
{
    using UnityEngine;

    public class UpdateCameraPos : MonoBehaviour
    {
        [SerializeField] private Transform _cameraPos;
        // Update is called once per frame
        void Update()
        {
            transform.position = _cameraPos.position;
        }
    }
}
