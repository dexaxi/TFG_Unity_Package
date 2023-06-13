namespace DUJAL.MovementComponents
{
    using UnityEngine;

    public enum CameraFollowType 
    {
        CameraFollowType_SmoothDamp,
        CameraFollowType_Lerp,
        CameraFollowType_SLerp,
        CameraFollowType_MoveTowards
    }

    /// <summary>
    //	This class enables following of an object smoothly on each axis, it also allows defining a deadzone and constraints on three axes.
    /// </summary>
    public class CameraFollower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _targetTransform;

        [Header("Settings")]
        [SerializeField] public CameraFollowType CameraFollowType;
        [SerializeField] public bool FollowX;
        [SerializeField] public bool FollowY;
        [SerializeField] public bool FollowZ;
        
        [Header("Parameters")]
        [Tooltip("Offset added to follow position, useful for when the center of the screen is not the player.")]
        [SerializeField] public Vector3 Offset;
        
        [Tooltip("If a constraint value is not zero, it will become the maximum value the camera will follow the taget to in that axis.")]
        [SerializeField] public Vector3 Constraints;
        
        [Tooltip("Minimum value the target has to move relative to the camera for the camera to move.")]
        [SerializeField] public Vector3 Deadzone;
        
        [Tooltip("Smoothness value.")]
        [SerializeField] [Range(0, 1)] private float _smoothness;

        private Vector3 _velocity;
        private float _currentSmoothness;
        private void Awake()
        {
            _velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (!FollowX && !FollowY && !FollowZ) return;

            Vector3 newPos = GetNewCameraPosition();
            _currentSmoothness = NormalizeCameraSmoothness();
            switch (CameraFollowType)
            {
                case CameraFollowType.CameraFollowType_SmoothDamp:
                    CameraFollowSmoothDamp(newPos);
                    break;
                case CameraFollowType.CameraFollowType_Lerp:
                    CameraFollowLerp(newPos);
                    break;
                case CameraFollowType.CameraFollowType_SLerp:
                    CameraFollowSLerp(newPos);
                    break;
                case CameraFollowType.CameraFollowType_MoveTowards:
                    CameraFollowMoveTowards(newPos);
                    break;
            }
        }

        private float NormalizeCameraSmoothness() 
        {
            switch (CameraFollowType) 
            {
                case CameraFollowType.CameraFollowType_SmoothDamp:
                    return _smoothness * (500 - 50) + 50;

                case CameraFollowType.CameraFollowType_Lerp:
                case CameraFollowType.CameraFollowType_SLerp:
                    return (1.1f - _smoothness) * (5);

                case CameraFollowType.CameraFollowType_MoveTowards:
                default:
                    return (1.1f - _smoothness) * (10);
            }
        }

        private Vector3 GetNewCameraPosition() 
        {
            float newX, newY, newZ;

            newX = transform.position.x;
            newY = transform.position.y;
            newZ = transform.position.z;

            if (FollowX && Mathf.Abs(newX - _targetTransform.position.x) >= Deadzone.x)
            {
                newX = _targetTransform.position.x + Offset.x;
                if ((Constraints.x > 0 && newX > Constraints.x) || (Constraints.x < 0 && newX < Constraints.x))
                {
                    newX = Constraints.x;
                }
            }

            if (FollowY && Mathf.Abs(newY - _targetTransform.position.y) >= Deadzone.y)
            {
                newY = _targetTransform.position.y + Offset.y;
                if ((Constraints.y > 0 && newY > Constraints.y) || (Constraints.y < 0 && newY < Constraints.y))
                {
                    newY = Constraints.y;
                }
            }

            if (FollowZ && Mathf.Abs(newZ - _targetTransform.position.z) >= Deadzone.z)
            {
                newZ = _targetTransform.position.z + Offset.z;
                if ((Constraints.z > 0 && newZ > Constraints.z) || (Constraints.z < 0 && newZ < Constraints.z))
                {
                    newZ = Constraints.z;
                }
            }

            return new Vector3(newX, newY, newZ);
        }

        private void CameraFollowSmoothDamp(Vector3 newPos) 
        {
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _velocity, _currentSmoothness * Time.deltaTime);
        }        
        
        private void CameraFollowLerp(Vector3 newPos) 
        {
            transform.position = Vector3.Lerp(transform.position, newPos, _currentSmoothness * Time.deltaTime);
        }        
        
        private void CameraFollowSLerp(Vector3 newPos) 
        {
            transform.position = Vector3.Slerp(transform.position, newPos, _currentSmoothness * Time.deltaTime);
        }
        
        private void CameraFollowMoveTowards(Vector3 newPos) 
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, _currentSmoothness * Time.deltaTime);
        }

        /// <summary>
        //	Change follow type, SmoothDamp, Lerp, SLerp, MoveTowards.
        /// </summary>
        public void SetFollowType(CameraFollowType newType) 
        {
            CameraFollowType = newType;
        }

        /// <summary>
        //	Toggle following of transform in the X axis.
        /// </summary>
        public void ToggleFollowX() 
        {
            FollowX = !FollowX;
        }

        /// <summary>
        //	Toggle following of transform in the Y axis.
        /// </summary>
        public void ToggleFollowY() 
        {
            FollowY = !FollowY;
        }

        /// <summary>
        //	Toggle following of transform in the Z axis.
        /// </summary>
        public void ToggleFollowZ() 
        {
            FollowZ = !FollowZ;
        }

        /// <summary>
        //	Toggle following of transform in all axes.
        /// </summary>
        public void ToggleAllFollows() 
        {
            FollowX = !FollowX;
            FollowY = !FollowY;
            FollowZ = !FollowZ;
        }

        /// <summary>
        //	Follow new transform.
        /// </summary>
        public void ChangeFollow(Transform newFollow) 
        {
            _targetTransform = newFollow;
        }
    }
}
