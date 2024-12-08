namespace DUJAL.MovementComponents.DiscreteBased3D
{
    using Cinemachine;
    using DG.Tweening;
    using DUJAL.IndependentComponents.LaunchRigidBody;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum ThirdPerson_Discrete
    {
        ThirdPerson_Discrete_Restricted,
        ThirdPerson_Discrete_Walking,
        ThirdPerson_Discrete_Running,
        ThirdPerson_Discrete_Crouching,
        ThirdPerson_Discrete_Air
    }

    public enum CameraMode_Discrete 
    {
        CameraMode_Discrete_Normal,
        CameraMode_Discrete_Shoulder,
        CameraMode_Discrete_TopDown,
        CameraMode_Discrete_FixedYTopDown,
        CameraMode_Discrete_FixedXYTopDown,
        CameraMode_Discrete_Isometric
    }

    public class ThirdPersonMovement3D : MovementComponent
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

        [Header("Camera Type Settings")]
        [SerializeField] private CameraMode_Discrete _currentCameraMode;
        [SerializeField] private Transform _shoulderCamLookAt;
        [SerializeField] private List<GameObject> _cameraPrefabs;

        [Header("Camera Settings")]
        [SerializeField] private Vector2 _mouseSensitivity;
        [SerializeField] private Vector2 _controllerSensitivity;
        [SerializeField] [Range(0, 0.2f)]private float _rotationTime;
        [SerializeField] private Transform _orientation;
        [SerializeField] private Camera _camera;

        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsRestricted { get; private set; }
        public bool IsExitingSlope { get; private set; }

        public ThirdPerson_Discrete State { get; private set; }

        private Rigidbody _rigidbody;
        private RaycastHit _slopeHit;

        private Vector3 _moveDir;
        public GameObject CurrentCamera { get; private set; }
        private CinemachineFreeLook _freeLook;

        private float _currentMoveSpeed;
        private float _desiredMoveSpeed;
        private float _startingScale;
        
        private bool _keepMomentum;
        private bool _readyToJump;

        private void Awake()
        {
            Physics.gravity = new Vector3(0f, -30f, 0f);
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            _readyToJump = true;
            _startingScale = transform.localScale.y;
            ChangeCameraMode(_currentCameraMode);
            InputHanlder.Instance.LockCursor();
            HandleInput();
        }

        private void Update()
        {
            HandleStates();
            HandleDeviceChange();
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            HandleCameraRotation();
            HandleDrag();
            HandleJump();
            
            if (State != ThirdPerson_Discrete.ThirdPerson_Discrete_Restricted) HandleMovement();
            
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
            if (State == ThirdPerson_Discrete.ThirdPerson_Discrete_Walking || State == ThirdPerson_Discrete.ThirdPerson_Discrete_Running|| State == ThirdPerson_Discrete.ThirdPerson_Discrete_Crouching) _rigidbody.drag = _groundDrag;
            else _rigidbody.drag = _airDrag;
        }

        private void HandleCameraRotation()
        {
            //Camera Type Logic
            switch (_currentCameraMode)
            {
                case CameraMode_Discrete.CameraMode_Discrete_Normal:
                case CameraMode_Discrete.CameraMode_Discrete_TopDown:
                case CameraMode_Discrete.CameraMode_Discrete_FixedYTopDown:
                    HandleNormalCameraCharacterRotation();
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_FixedXYTopDown:
                    HandleXYFixedTopDownCharacterRotation();
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_Isometric:
                    HandleIsometricCharacterRotation();
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_Shoulder:
                    HandleShoulderCameraCharacterRotation();
                    break;
            }

            //Sensitivity logic
            if (_currentCameraMode == CameraMode_Discrete.CameraMode_Discrete_Isometric || _currentCameraMode == CameraMode_Discrete.CameraMode_Discrete_FixedXYTopDown) return;
            Vector2 sensitivity = UseMouse ? _mouseSensitivity : _controllerSensitivity;
            _freeLook.m_XAxis.m_MaxSpeed = 300 * sensitivity.x;
            if (_currentCameraMode == CameraMode_Discrete.CameraMode_Discrete_FixedYTopDown) return;
            _freeLook.m_YAxis.m_MaxSpeed = 2 * sensitivity.y;
        }

        public void ChangeCameraMode(CameraMode_Discrete cameraMode)
        {

            foreach (GameObject camera in _cameraPrefabs)
            {
                camera.SetActive(false);
            }

            _currentCameraMode = cameraMode;

            switch (_currentCameraMode)
            {
                case CameraMode_Discrete.CameraMode_Discrete_Normal:
                    _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_Normal].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_Normal];
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_Shoulder:
                    _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_Shoulder].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_Shoulder];
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_TopDown:
                    _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_TopDown].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_TopDown];
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_FixedYTopDown:
                    _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_FixedYTopDown].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_FixedYTopDown];
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_FixedXYTopDown:
                    _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_FixedXYTopDown].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_FixedXYTopDown];
                    break;

                case CameraMode_Discrete.CameraMode_Discrete_Isometric:
                    _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_Isometric].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode_Discrete.CameraMode_Discrete_Isometric];
                    break;
            }

            _freeLook = CurrentCamera.GetComponentInChildren<CinemachineFreeLook>();
        }

        private void HandleNormalCameraCharacterRotation()
        {
            _orientation.forward =  (transform.position - new Vector3(_camera.transform.position.x, transform.position.y, _camera.transform.position.z)).normalized;

            Vector3 inputDirection = _orientation.forward * MovementInput.y + _orientation.right * MovementInput.x;
            
            if (inputDirection != Vector3.zero) 
            {
                Vector3 rigidBodyVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                transform.forward = Vector3.SmoothDamp(transform.forward, inputDirection, ref rigidBodyVelocity, _rotationTime);
            }
        }
        
        private void HandleIsometricCharacterRotation()
        {
            float panAngle = _freeLook.GetComponent<CinemachineRecomposer>().m_Pan;
            _orientation.rotation = Quaternion.Euler(_camera.transform.rotation.x, panAngle, _camera.transform.rotation.z);
            
            Vector3 inputDirection = _orientation.forward * MovementInput.y + _orientation.right * MovementInput.x;
            
            if (inputDirection != Vector3.zero) 
            {
                Vector3 rigidBodyVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                transform.forward = Vector3.SmoothDamp(transform.forward, inputDirection, ref rigidBodyVelocity, _rotationTime);
            }
        }
                
        private void HandleXYFixedTopDownCharacterRotation()
        {
            float panAngle = _freeLook.GetComponent<CinemachineRecomposer>().m_Pan;
            _freeLook.m_XAxis.Value = panAngle;
            _orientation.rotation = Quaternion.Euler(_camera.transform.rotation.x, panAngle, _camera.transform.rotation.z);
            
            Vector3 inputDirection = _orientation.forward * MovementInput.y + _orientation.right * MovementInput.x;
            
            if (inputDirection != Vector3.zero) 
            {
                Vector3 rigidBodyVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                transform.forward = Vector3.SmoothDamp(transform.forward, inputDirection, ref rigidBodyVelocity, _rotationTime);
            }
        }

        private void HandleShoulderCameraCharacterRotation()
        {
            _orientation.forward =  (_shoulderCamLookAt.position - new Vector3(_camera.transform.position.x, _shoulderCamLookAt.position.y, _camera.transform.position.z)).normalized;
            transform.forward = _orientation.forward.normalized;
        }

        private void PerformMovement(Vector2 movementVector)
        {
            MovementInput = movementVector;
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
                _rigidbody.velocity = new Vector3(_moveDir.normalized.x * _currentMoveSpeed * 10f, _rigidbody.velocity.y, _moveDir.normalized.z * _currentMoveSpeed * 10f);
            }
            else if (!IsGrounded) 
            {
                _rigidbody.velocity = new Vector3(_moveDir.normalized.x * _currentMoveSpeed * 10f * _airSpeedMultiplier, _rigidbody.velocity.y, _moveDir.normalized.z * _currentMoveSpeed * 10f * _airSpeedMultiplier);
            }
            _rigidbody.useGravity = !CheckSlope();
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
            State = ThirdPerson_Discrete.ThirdPerson_Discrete_Running;
            _desiredMoveSpeed = _runSpeed;
        }

        private void SetWalkState()
        {
            State = ThirdPerson_Discrete.ThirdPerson_Discrete_Walking;
            _desiredMoveSpeed = _walkSpeed;
        }

        private void SetCrouchingState()
        {
            State = ThirdPerson_Discrete.ThirdPerson_Discrete_Crouching;
            _desiredMoveSpeed = _crouchMovementSpeed;
        }

        private void SetAirState()
        {
            State = ThirdPerson_Discrete.ThirdPerson_Discrete_Air;
            if (_desiredMoveSpeed < _runSpeed) _desiredMoveSpeed = _walkSpeed;
            else _desiredMoveSpeed = _runSpeed;
        }

        private void HandleStates()
        {
            if (IsRestricted)
            {
                State = ThirdPerson_Discrete.ThirdPerson_Discrete_Restricted;
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

        public void SetIsometricCameraAngle(float newPanAngle, float rotationTime) 
        {
            if (_currentCameraMode == CameraMode_Discrete.CameraMode_Discrete_Isometric) 
            {
                StartCoroutine(SetIsometricCameraAngleC(newPanAngle, rotationTime));
            }
        }        
        
        public void RotateIsometricCamera(float angleToSum, float rotationTime) 
        {
            if (_currentCameraMode == CameraMode_Discrete.CameraMode_Discrete_Isometric) 
            {
                float newAngle = _freeLook.GetComponent<CinemachineRecomposer>().m_Pan + angleToSum;
                StartCoroutine(SetIsometricCameraAngleC(newAngle, rotationTime));
            }
        }

        private IEnumerator SetIsometricCameraAngleC(float newPanAngle, float rotationTime) 
        {    
            float time = 0;
            CinemachineRecomposer recomposer = _freeLook.GetComponent<CinemachineRecomposer>();
            float startValue = recomposer.m_Pan;

            while (time < rotationTime) 
            {
                time += Time.deltaTime;
                recomposer.m_Pan = Mathf.Lerp(startValue, newPanAngle, time / rotationTime);
                yield return null;
            }

            _freeLook.GetComponent<CinemachineRecomposer>().m_Pan = newPanAngle;
        }

        private void OnDestroy()
        {
            MovementMap.ThirdPersonMovement.Disable();

            MovementMap.ThirdPersonMovement.Move.performed -= ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };
            MovementMap.ThirdPersonMovement.Move.canceled -= ctx => { PerformMovement(ctx.ReadValue<Vector2>()); };

            MovementMap.ThirdPersonMovement.Crouch.performed -= ctx => { PerformCrouch(); };
            MovementMap.ThirdPersonMovement.Crouch.canceled -= ctx => { CancelCrouch(); };
        }

        [ContextMenu("Debug RotateIsoCam")]
        public void DebugRotateIsometricCamera90Degrees() 
        {
            if(_currentCameraMode == CameraMode_Discrete.CameraMode_Discrete_Isometric) RotateIsometricCamera(90, 1f);
        }
    }
}