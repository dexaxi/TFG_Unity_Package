namespace DUJAL.MovementComponents.DiscreteBased3D
{
    using DG.Tweening;
    using DUJAL.IndependentComponents.LaunchRigidBody;
    using System;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    public enum FPS_Discrete
    {
        FPS_Discrete_Restricted,
        FPS_Discrete_Walking,
        FPS_Discrete_Running,
        FPS_Discrete_Crouching,
        FPS_Discrete_Air,
    }

    public class FirstPersonMovement3D : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] [Range(0, 50)] private float _walkSpeed;
        [SerializeField] [Range(0, 50)] private float _runSpeed;
        [SerializeField] [Range(0, 10)] private float _groundDrag;

        [Header("Air Movement Settings")]
        [SerializeField] [Range(0, 2)] private float _airSpeedMultiplier;
        [SerializeField] [Range(0, 10)] private float _airDrag;

        [Header("Crouch Settings")]
        [SerializeField] [Range(0, 25)] private float _crouchMovementSpeed;
        [SerializeField] [Range(0, 3)] private float _crouchScale;

        [Header("Jump Settings")]
        [SerializeField] [Range(0, 50)] private float _jumpForce;
        [SerializeField] [Range(0, 1)] private float _jumpCooldown;

        [Header("Ground Check Settings")]
        [SerializeField] [Range(0, 3)] private float _playerHeight;
        [SerializeField] public LayerMask GroundCollisionLayer;

        [Header("Slope Settings")]
        [SerializeField] [Range(0, 180)] private int _maxSlope;
        [SerializeField] [Range(0, 10)] private float _speedLossMultiplier;
        [SerializeField] [Range(0, 10)] private float _slopeMomentumDecreaseMultiplier;
        
        [Header("Sensitivity Settings")]
        [SerializeField] private Vector2 _mouseSensitivity;
        [SerializeField] private Vector2 _controllerSensitivity;

        [Header("References")]
        [SerializeField] public Transform Orientation;
        [SerializeField] private Transform CameraHolder;
        [SerializeField] public Camera Camera;

        public FPS_Discrete State { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsRestricted { get; private set; }
        public bool IsExitingSlope { get; private set; }
        public bool UseMouse { get; private set; }
        public Vector2 MovementInput { get; private set; }
        public MovementInput MovementMap { get; private set;}
        
        private Rigidbody _rigidbody;
        private RaycastHit _slopeHit;
        
        private Vector2 _cameraRotation;
        private Vector3 _moveDir;

        private float _currentMoveSpeed;
        private float _desiredMoveSpeed;
        private float _startingScale;

        private bool _readyToJump;        

        private void Awake()
        {
            Physics.gravity = new Vector3(0f, -30f, 0f);
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            _readyToJump = true;
            _startingScale = transform.localScale.y;
            LockCursor();
            HandleInput();
        }

        private void Update()
        {
            HandleStates();
            HandleDrag();
            HandleDeviceChange();
            HandleCameraRotation(MovementMap.FPS.CameraMovement.ReadValue<Vector2>());
        }

        private void FixedUpdate()
        {
            if (State != FPS_Discrete.FPS_Discrete_Restricted) HandleMovement();
            CheckGrounded();
            HandleJump();
            SpeedControl();
        }

        private void HandleDrag() 
        {
            if (State == FPS_Discrete.FPS_Discrete_Walking|| State == FPS_Discrete.FPS_Discrete_Running || State == FPS_Discrete.FPS_Discrete_Crouching) _rigidbody.drag = _groundDrag;
            else _rigidbody.drag = _airDrag;
        }

        private void CheckGrounded()
        {
            IsGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, GroundCollisionLayer);
        }

        private void HandleInput()
        {
            MovementMap = new MovementInput();
            MovementMap.FPS.Enable();

            MovementMap.FPS.Move.performed += ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };
            MovementMap.FPS.Move.canceled += ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };

            MovementMap.FPS.Crouch.performed += ctx => { PerformCrouch(); };
            MovementMap.FPS.Crouch.canceled += ctx => { CancelCrouch(); };
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void HandleCameraRotation(Vector2 mouseDelta)
        {
            Vector2 sensitivity = UseMouse ? _mouseSensitivity : _controllerSensitivity;
            float mouseInputX = mouseDelta.x * Time.deltaTime * sensitivity.x;
            float mouseInputY = mouseDelta.y * Time.deltaTime * sensitivity.y;

            _cameraRotation.y += mouseInputX;

            _cameraRotation.x -= mouseInputY;
            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -90f, 90f);

            CameraHolder.transform.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            Orientation.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
        }

        private void HandleDeviceChange()
        {
            if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                || (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.wasPressedThisFrame))
            {
                UseMouse = true;
            }
            else if (Gamepad.current != null &&
                (Gamepad.current.allControls.Any(x => x is ButtonControl button && x.IsPressed() && !x.synthetic)
                || Gamepad.current.allControls.Any(y => y is StickControl stick && y.IsActuated() && !y.synthetic)))
            {
                UseMouse = false;
            }
        }

        private void SetRunState()
        {
            State = FPS_Discrete.FPS_Discrete_Running;
            _desiredMoveSpeed = _runSpeed;
        }

        private void SetWalkState()
        {
            State = FPS_Discrete.FPS_Discrete_Walking;
            _desiredMoveSpeed = _walkSpeed;
        }

        private void SetCrouchingState() 
        {
            State = FPS_Discrete.FPS_Discrete_Crouching;
            _desiredMoveSpeed = _crouchMovementSpeed;
        }

        private void SetAirState() 
        {
            State = FPS_Discrete.FPS_Discrete_Air;
            if (_desiredMoveSpeed < _runSpeed) _desiredMoveSpeed = _walkSpeed;
            else _desiredMoveSpeed = _runSpeed;
        }

        private void PerformCrouch()
        {
            transform.localScale = new Vector3(transform.localScale.x, _crouchScale, transform.localScale.z);
            _rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            IsCrouching = true;
        }
        private void CancelCrouch()
        {
            IsCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, _startingScale, transform.localScale.z);
        }
        private void PerformMovement(Vector2 movementVector)
        {
            MovementInput = movementVector;
        }
        private void HandleJump()
        {
            if (MovementMap.FPS.Jump.IsPressed() && _readyToJump && IsGrounded)
            {
                _readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), _jumpCooldown);
            }
        }

        private void HandleStates()
        {
            if (IsRestricted)
            {
                State = FPS_Discrete.FPS_Discrete_Restricted;
            }
            else if (IsCrouching)
            {
                SetCrouchingState();
            }
            else if (MovementMap.FPS.Run.IsPressed() && IsGrounded && !IsCrouching)
            {
                SetRunState();
            }
            else if (IsGrounded)
            {
                SetWalkState();
            }
            else if(!IsGrounded)
            {
                SetAirState();
            }

            _currentMoveSpeed = _desiredMoveSpeed;
        }

        private void HandleMovement()
        {
            _moveDir = Orientation.forward * MovementInput.y + Orientation.right * MovementInput.x;

            if (CheckSlope() && !IsExitingSlope)
            {
                _rigidbody.AddForce(CalculateNormalizedSlopeDirection(_moveDir) * _currentMoveSpeed * 20f, ForceMode.Force);

                if (_rigidbody.velocity.y > 0) _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            else if (IsGrounded)
            {
                _rigidbody.velocity = new Vector3(_moveDir.normalized.x * _currentMoveSpeed * 10f, _rigidbody.velocity.y, _moveDir.normalized.z * _currentMoveSpeed * 10f);
            }
            else if (!IsGrounded) 
            {
                _rigidbody.velocity = new Vector3(_moveDir.normalized.x * _currentMoveSpeed * 10f * _airSpeedMultiplier, _rigidbody.velocity.y, _moveDir.normalized.z * _currentMoveSpeed * 10f * _airSpeedMultiplier);
            }
            _rigidbody.useGravity = !CheckSlope();
        }

        private void SpeedControl()
        {
            if (CheckSlope() && !IsExitingSlope)
            {
                if (_rigidbody.velocity.magnitude > _currentMoveSpeed)
                {
                    _rigidbody.velocity = _rigidbody.velocity.normalized * _currentMoveSpeed;
                }
            }
            else
            {
                Vector3 groundVel = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

                if (groundVel.magnitude > _currentMoveSpeed)
                {
                    Vector3 newGroundVel = groundVel.normalized * _currentMoveSpeed;
                    _rigidbody.velocity = new Vector3(newGroundVel.x, _rigidbody.velocity.y, newGroundVel.z);
                }
            }
        }

        private void Jump()
        {
            IsExitingSlope = true;
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            LaunchRigidBody.LaunchRigidBody3D(_rigidbody, transform.up, _jumpForce);
        }

        private void ResetJump()
        {
            _readyToJump = true;
            IsExitingSlope = false;
        }

        public bool CheckSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                return angle < _maxSlope && angle != 0;
            }
            return false;
        }

        public Vector3 CalculateNormalizedSlopeDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
        }

        public void UpdateFOV(float fov)
        {
            Camera.DOFieldOfView(fov, 0.25f);
        }

        public void UpdateTilt(float tilt)
        {
            Camera.transform.DOLocalRotate(new Vector3(0, 0, tilt), 0.25f);
        }

        private void OnDestroy()
        {
            MovementMap.FPS.Disable();
            MovementMap.FPS.Move.performed -= ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };
            MovementMap.FPS.Move.canceled -= ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };

            MovementMap.FPS.Crouch.performed -= ctx => { PerformCrouch(); };
            MovementMap.FPS.Crouch.canceled -= ctx => { CancelCrouch(); };
        }
    }
}