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
    public class CameraFollower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _playerTransform;

        [Header("Settings")]
        [SerializeField] public CameraFollowType CameraFollowType;
        [SerializeField] public bool FollowX;
        [SerializeField] public bool FollowY;
        [SerializeField] public bool FollowZ;
        [SerializeField] private Vector3 _offset;
        [SerializeField] public Vector3 Constraints;
        [SerializeField] public Vector3 Deadzone;
        
        [Tooltip("Camera follow smoothness value, Bear in mind that for SmoothDamp and Move towards, a bigger smoothness implies a slower speed, while in Lerp and SLerp, smaller smoothness means slower speed.")]
        [SerializeField] [Range(0, 100)] private float _smoothness;

        private Vector3 _velocity;

        private void Awake()
        {
            _velocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (!FollowX && !FollowY && !FollowZ) return;
            float newX, newY, newZ;
            newX = _offset.x;
            newY = _offset.y;
            newZ = _offset.z;
            if (FollowX) 
            {
                newX += _playerTransform.position.x;
                if ((Constraints.x > 0 && newX > Constraints.x) || (Constraints.x < 0 && newX < Constraints.x))
                {
                    newX = Constraints.x;
                }
                if (Mathf.Abs(transform.position.x - _playerTransform.position.x) < Deadzone.x) newX = transform.position.x; 
            }
            
            if (FollowY) 
            {
                newY = _playerTransform.position.y + _offset.y;
                if (Constraints.y != 0 && newY > Constraints.y) newY = Constraints.y;
                newY = transform.position.y - newY >= Deadzone.y ? newY : _offset.y;
            }

            if (FollowZ) 
            {
                newZ = _playerTransform.position.z + _offset.z;
                if (Constraints.z != 0 && newZ > Constraints.z) newZ = Constraints.z;
                newZ = transform.position.z - newZ >= Deadzone.z ? newZ : _offset.z;
            }


            
            Vector3 newPos = new Vector3(newX, newY, newZ);

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

        private void CameraFollowSmoothDamp(Vector3 newPos) 
        {
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _velocity, _smoothness * Time.deltaTime);
        }        
        
        private void CameraFollowLerp(Vector3 newPos) 
        {
            transform.position = Vector3.Lerp(transform.position, newPos, _smoothness * Time.deltaTime);
        }        
        
        private void CameraFollowSLerp(Vector3 newPos) 
        {
            transform.position = Vector3.Slerp(transform.position, newPos, _smoothness * Time.deltaTime);
        }
        
        private void CameraFollowMoveTowards(Vector3 newPos) 
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, _smoothness * Time.deltaTime);
        }

        public void SetFollowType(CameraFollowType newType) 
        {
            CameraFollowType = newType;
        }

        public void ToggleFollowX() 
        {
            FollowX = !FollowX;
        }        
        
        public void ToggleFollowY() 
        {
            FollowY = !FollowY;
        }
        
        public void ToggleFollowZ() 
        {
            FollowZ = !FollowZ;
        }
        
        public void ToggleAllFollows() 
        {
            FollowX = !FollowX;
            FollowY = !FollowY;
            FollowZ = !FollowZ;
        }
    }
}
