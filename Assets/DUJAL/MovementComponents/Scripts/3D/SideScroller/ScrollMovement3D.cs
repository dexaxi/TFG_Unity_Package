namespace DUJAL.MovementComponents.DiscreteBased3D
{
    using DG.Tweening;
    using DUJAL.IndependentComponents.LaunchRigidBody;
    using System;
    using System.Collections;
    using UnityEngine;

    public enum SideScroll_Discrete
    {
        SideScroll_Discrete_Restricted,
        SideScroll_Discrete_Walking,
        SideScroll_Discrete_Running,
        SideScroll_Discrete_Crouching,
        SideScroll_Discrete_Air
    }

    public class ScrollMovement3D : MovementComponent
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

        [Header("Camera Settings")]
        [SerializeField] [Range(0, 0.2f)]private float _rotationTime;
        [SerializeField] private Transform _orientation;
        [SerializeField] private Camera _camera;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsRestricted { get; private set; }
        public bool IsExitingSlope { get; private set; }

        public SideScroll_Discrete State { get; private set; }

        private Rigidbody _rigidbody;
        private RaycastHit _slopeHit;

        private Vector3 _moveDir;

        private float _currentMoveSpeed;
        private float _desiredMoveSpeed;
        private float _startingScale;
        private float _speedLossFactor;
        
        private bool _readyToJump;

        private void Awake()
        {
            Physics.gravity = new Vector3(0f, -30f, 0f);
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            _readyToJump = true;
            _startingScale = transform.localScale.y;
            InputHanlder.Instance.LockCursor();
            HandleInput();
        }

        private void Update()
        {
            HandleStates();
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            HandleCameraRotation();
            HandleDrag();
            HandleJump();
            
            if (State != SideScroll_Discrete.SideScroll_Discrete_Restricted) HandleMovement();
            
            SpeedControl();
        }
        private void HandleInput()
        {
            MovementMap = new MovementInput();
            MovementMap.ThirdPersonMovement.Enable();

            MovementMap.ThirdPersonMovement.Move.performed += ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };
            MovementMap.ThirdPersonMovement.Move.canceled += ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };

            MovementMap.ThirdPersonMovement.Crouch.performed += ctx => { PerformCrouch(); };
            MovementMap.ThirdPersonMovement.Crouch.canceled += ctx => { CancelCrouch(); };
        }
        private void HandleDrag()
        {
            if (State == SideScroll_Discrete.SideScroll_Discrete_Walking || State == SideScroll_Discrete.SideScroll_Discrete_Running || State == SideScroll_Discrete.SideScroll_Discrete_Crouching) _rigidbody.drag = _groundDrag;
            else _rigidbody.drag = _airDrag;
        }

        private void HandleCameraRotation()
        {
            HandleXYFixedTopDownCharacterRotation();
        }

        private void HandleXYFixedTopDownCharacterRotation()
        {
            _orientation.rotation = Quaternion.Euler(_camera.transform.rotation.x, _camera.transform.rotation.y, _camera.transform.rotation.z);

            Vector3 inputDirection = _orientation.forward * MovementInput.y + _orientation.right * MovementInput.x;
            
            if (inputDirection != Vector3.zero) 
            {
                Vector3 rigidBodyVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                transform.forward = Vector3.SmoothDamp(transform.forward, inputDirection, ref rigidBodyVelocity, _rotationTime);
            }
        }

        private void PerformMovement(Vector2 movementVector)
        {
            MovementInput = new Vector2(movementVector.x, 0f);
        }

        private void HandleMovement()
        {
            _moveDir = _orientation.forward * MovementInput.y + _orientation.right * MovementInput.x;

            if (CheckSlope() && !IsExitingSlope)
            {
                _rigidbody.AddForce(CalculateNormalizedSlopeDirection(_moveDir) * _currentMoveSpeed * 20f, ForceMode.Force);

                if (_rigidbody.velocity.y > 0) _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            else if (IsGrounded)
            {
                _rigidbody.velocity = new Vector3(_moveDir.normalized.x * _currentMoveSpeed * 10f, _rigidbody.velocity.y, 0f);
            }
            else if (!IsGrounded)
            {
                _rigidbody.velocity = new Vector3(_moveDir.normalized.x * _currentMoveSpeed * 10f * _airSpeedMultiplier, _rigidbody.velocity.y, 0f);
            }
            _rigidbody.useGravity = !CheckSlope();

            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0f);
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.RoundToInt(transform.position.z));
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

        private void CheckGrounded()
        {
            IsGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, GroundCollisionLayer);
        }

        private void HandleJump()
        {
            if (MovementMap.ThirdPersonMovement.Jump.IsPressed() && _readyToJump && IsGrounded)
            {
                _readyToJump = false;

                PerformJump();

                Invoke(nameof(ResetJump), _jumpCooldown);
            }
        }

        private void PerformJump() 
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

        private void SetRunState()
        {
            State = SideScroll_Discrete.SideScroll_Discrete_Running;
            _desiredMoveSpeed = _runSpeed;
        }

        private void SetWalkState()
        {
            State = SideScroll_Discrete.SideScroll_Discrete_Walking;
            _desiredMoveSpeed = _walkSpeed;
        }

        private void SetCrouchingState()
        {
            State = SideScroll_Discrete.SideScroll_Discrete_Crouching;
            _desiredMoveSpeed = _crouchMovementSpeed;
        }

        private void SetAirState()
        {
            State = SideScroll_Discrete.SideScroll_Discrete_Air;
            if (_desiredMoveSpeed < _runSpeed) _desiredMoveSpeed = _walkSpeed;
            else _desiredMoveSpeed = _runSpeed;
        }

        private void HandleStates()
        {
            if (IsRestricted)
            {
                State = SideScroll_Discrete.SideScroll_Discrete_Restricted;
            }
            else if (IsCrouching)
            {
                SetCrouchingState();
            }
            else if (MovementMap.ThirdPersonMovement.Run.IsPressed() && IsGrounded && !IsCrouching)
            {
                SetRunState();
            }
            else if (IsGrounded)
            {
                SetWalkState();
            }
            else if (!IsGrounded)
            {
                SetAirState();
            }

            _currentMoveSpeed = _desiredMoveSpeed;
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
            _camera.DOFieldOfView(fov, 0.25f);
        }

        public void UpdateTilt(float tilt)
        {
            _camera.transform.DOLocalRotate(new Vector3(0, 0, tilt), 0.25f);
        }

        private void OnDestroy()
        {
            MovementMap.ThirdPersonMovement.Disable();

            MovementMap.ThirdPersonMovement.Move.performed -= ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };
            MovementMap.ThirdPersonMovement.Move.canceled -= ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };

            MovementMap.ThirdPersonMovement.Crouch.performed -= ctx => { PerformCrouch(); };
            MovementMap.ThirdPersonMovement.Crouch.canceled -= ctx => { CancelCrouch(); };
        }
    }
}