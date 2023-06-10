namespace DUJAL.MovementComponents.PhysicsBased3D 
{
    using UnityEngine;
    using UnityEngine.Events;
    public class DashComponent : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] [Range(0,100)] private float _horizontalDashForce;
        [SerializeField] [Range(0, 100)] private float _verticalDashForce;
        [SerializeField] [Range(0, 100)] private float _maxYSpeedWhileDashing;
        [SerializeField] [Range(0, 5)] private float _dashDuration;
        [SerializeField] [Range(0, 20)] private float _dashCooldown;

        [Header("Camera Effect Settings")]
        [SerializeField] [Range(60, 220)] private float _dashFov;

        [Header("Misc Settings")]
        [SerializeField] private bool _allowUpwardsDash;
        [SerializeField] private bool _allowSidewaysDash ;
        [SerializeField] private bool _disableGravityWhileDashing;
        [SerializeField] private bool _resetVelocityBeforeDashing;

        [Header("Events")]
        [Space(4)]
        [SerializeField] public UnityEvent OnDash;
        [SerializeField] public UnityEvent OnResetDash;
        
        private Rigidbody _rigidbody;
        private PhysicsBasedFirstPersonMovement3D _player;
        
        private Vector3 _delayedForceToApply;
        
        private float _dashTimer;
        private float _startingCameraFov;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _player = GetComponent<PhysicsBasedFirstPersonMovement3D>();


            _startingCameraFov = _player.Camera.fieldOfView;
            
            OnDash.AddListener(() => { _player.UpdateFOV(_dashFov); });
            OnResetDash.AddListener(() => { _player.UpdateFOV(_startingCameraFov); });
        }

        private void Start()
        {
            HandleInput();
        }

        private void Update()
        {
            HandleDashTimer();
        }

        private void HandleDashTimer() 
        {
            if (_dashTimer > 0) _dashTimer -= Time.deltaTime;
        }

        private void HandleInput() 
        {
            _player.MovementMap.FPS.Dash.performed += ctx => { Dash(); };
        }

        private void Dash()
        {
            if (_dashTimer > 0) return;
            else _dashTimer = _dashCooldown;
            
            OnDash.Invoke();

            if (_disableGravityWhileDashing) _rigidbody.useGravity = false;

            _player.IsDashing = true;
            _player.MaxYDashingspeed = _maxYSpeedWhileDashing;

            Transform forwardTransform = _allowUpwardsDash ? _player.Camera.transform : _player.Orientation;
            _delayedForceToApply = GetDirection(forwardTransform) * _horizontalDashForce + _player.Orientation.up * _verticalDashForce;

            Invoke(nameof(DelayedDashForce), 0.025f);
            Invoke(nameof(ResetDash), _dashDuration);
        }

        private void DelayedDashForce()
        {
            if (_resetVelocityBeforeDashing) _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(_delayedForceToApply, ForceMode.Impulse);
        }

        private void ResetDash()
        {
            OnResetDash.Invoke();

            _player.IsDashing = false;
            _player.MaxYDashingspeed = 0;

            if (_disableGravityWhileDashing) _rigidbody.useGravity = true;
        }

        private Vector3 GetDirection(Transform forwardT)
        {
            Vector3 direction;
            if (_allowSidewaysDash)
            {
                direction = forwardT.forward * _player.MovementInput.y + forwardT.right * _player.MovementInput.x;
            }
            else direction = forwardT.forward;

            if (_player.MovementInput.y == 0 && _player.MovementInput.x == 0) 
            {
                direction = forwardT.forward;
            }

            return direction.normalized;
        }
    }
}