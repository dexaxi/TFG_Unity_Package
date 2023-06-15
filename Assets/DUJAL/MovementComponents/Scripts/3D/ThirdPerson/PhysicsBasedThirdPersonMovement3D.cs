namespace DUJAL.MovementComponents.PhysicsBased3D
{
    using Cinemachine;
    using DG.Tweening;
    using DUJAL.IndependentComponents.LaunchRigidBody;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    public enum ThirdPerson
    {
        ThirdPerson_Restricted,
        ThirdPerson_Walking,
        ThirdPerson_Running,
        ThirdPerson_Crouching,
        ThirdPerson_Sliding,
        ThirdPerson_Air
    }

    public enum CameraMode 
    {
        CameraMode_Normal,
        CameraMode_Shoulder,
        CameraMode_TopDown,
        CameraMode_FixedYTopDown,
        CameraMode_FixedXYTopDown,
        CameraMode_Isometric
    }

    public class PhysicsBasedThirdPersonMovement3D : MovementComponent
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

        [Header("Slide Settings")]
        [SerializeField] [Range(0, 50)] private float _slideSpeed;
        [SerializeField] [Range(0, 500)] private float _slideForce;
        [SerializeField] [Range(0, 3)] private float _slideTime;
        [SerializeField] [Range(0, 2)] public float _slideScale;

        [Header("Ground Check Settings")]
        [SerializeField] [Range(0, 3)] private float _playerHeight;
        [SerializeField] public LayerMask GroundCollisionLayer;

        [Header("Slope Settings")]
        [SerializeField] [Range(0, 180)] private int _maxSlope;
        [SerializeField] [Range(0, 10)] private float _speedLossMultiplier;
        [SerializeField] [Range(0, 10)] private float _slopeMomentumDecreaseMultiplier;

        [Header("Camera Type Settings")]
        [SerializeField] private CameraMode _currentCameraMode;
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
        public bool IsSliding { get; private set; }
        public bool IsExitingSlope { get; private set; }

        public ThirdPerson State { get; private set; }

        private Rigidbody _rigidbody;
        private RaycastHit _slopeHit;
        private ThirdPerson _previousState;

        private Vector3 _moveDir;
        public GameObject CurrentCamera { get; private set; }
        private CinemachineFreeLook _freeLook;

        private float _currentMoveSpeed;
        private float _desiredMoveSpeed;
        private float _lastDesiredMoveSpeed;
        private float _startingScale;
        private float _currentSlideTimer;
        private float _speedLossFactor;
        
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
            HandleSliding();
            
            if (State != ThirdPerson.ThirdPerson_Restricted) HandleMovement();
            
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

            MovementMap.ThirdPersonMovement.Slide.performed += ctx => { PerformSlide(); };
            MovementMap.ThirdPersonMovement.Slide.canceled += ctx => { CancelSlide(); };
        }
        private void HandleDrag()
        {
            if (State == ThirdPerson.ThirdPerson_Walking || State == ThirdPerson.ThirdPerson_Running|| State == ThirdPerson.ThirdPerson_Crouching) _rigidbody.drag = _groundDrag;
            else _rigidbody.drag = _airDrag;
        }

        private void HandleCameraRotation()
        {
            //Camera Type Logic
            switch (_currentCameraMode) 
            {
                case CameraMode.CameraMode_Normal:
                case CameraMode.CameraMode_TopDown:
                case CameraMode.CameraMode_FixedYTopDown:
                    HandleNormalCameraCharacterRotation();
                    break;
                
                case CameraMode.CameraMode_FixedXYTopDown:
                    HandleXYFixedTopDownCharacterRotation();
                    break;

                case CameraMode.CameraMode_Isometric:
                    HandleIsometricCharacterRotation();
                    break;

                case CameraMode.CameraMode_Shoulder:
                    HandleShoulderCameraCharacterRotation();
                    break;
            }

            //Sensitivity logic
            if (_currentCameraMode == CameraMode.CameraMode_Isometric || _currentCameraMode == CameraMode.CameraMode_FixedXYTopDown) return;
            Vector2 sensitivity = UseMouse ? _mouseSensitivity : _controllerSensitivity;
            _freeLook.m_XAxis.m_MaxSpeed = 300 * sensitivity.x;
            if (_currentCameraMode == CameraMode.CameraMode_FixedYTopDown) return;
            _freeLook.m_YAxis.m_MaxSpeed = 2 * sensitivity.y;
        }

        public void ChangeCameraMode(CameraMode cameraMode) 
        {

            foreach (GameObject camera in _cameraPrefabs) 
            {
                camera.SetActive(false);
            }
            
            _currentCameraMode = cameraMode;

            switch (_currentCameraMode)
            {
                case CameraMode.CameraMode_Normal:
                    _cameraPrefabs[(int)CameraMode.CameraMode_Normal].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode.CameraMode_Normal];
                    break;

                case CameraMode.CameraMode_Shoulder:
                    _cameraPrefabs[(int)CameraMode.CameraMode_Shoulder].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode.CameraMode_Shoulder];
                    break;

                case CameraMode.CameraMode_TopDown:
                    _cameraPrefabs[(int)CameraMode.CameraMode_TopDown].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode.CameraMode_TopDown];
                    break;   

                case CameraMode.CameraMode_FixedYTopDown:
                    _cameraPrefabs[(int)CameraMode.CameraMode_FixedYTopDown].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode.CameraMode_FixedYTopDown];
                    break;

                case CameraMode.CameraMode_FixedXYTopDown:
                    _cameraPrefabs[(int)CameraMode.CameraMode_FixedXYTopDown].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode.CameraMode_FixedXYTopDown];
                    break;

                case CameraMode.CameraMode_Isometric:
                    _cameraPrefabs[(int)CameraMode.CameraMode_Isometric].SetActive(true);
                    CurrentCamera = _cameraPrefabs[(int)CameraMode.CameraMode_Isometric];
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
            _moveDir = _orientation.forward.normalized * MovementInput.normalized.y + _orientation.right.normalized * MovementInput.normalized.x;

            if (CheckSlope() && !IsExitingSlope)
            {
                _rigidbody.AddForce(CalculateNormalizedSlopeDirection(_moveDir) * _currentMoveSpeed * 20f, ForceMode.Force);

                if (_rigidbody.velocity.y > 0) _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            else if (IsGrounded)
            {
                _rigidbody.AddForce(_moveDir.normalized * _currentMoveSpeed * 10f, ForceMode.Force);
            }
            else if (!IsGrounded)
            {
                _rigidbody.AddForce(_moveDir.normalized * _currentMoveSpeed * 10f * _airSpeedMultiplier, ForceMode.Force);
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

        private void PerformSlide()
        {
            IsSliding = true;

            transform.localScale = new Vector3(transform.localScale.x, _slideScale, transform.localScale.z);
            _rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            _currentSlideTimer = _slideTime;
        }

        private void CancelSlide()
        {
            IsSliding = false;
            transform.localScale = new Vector3(transform.localScale.x, _startingScale, transform.localScale.z);
        }

        private void HandleSliding()
        {
            if (!IsSliding) return;

            if (!CheckSlope() || _rigidbody.velocity.y > -0.1f)
            {
                _rigidbody.AddForce(_moveDir.normalized * _slideForce, ForceMode.Force);
                _currentSlideTimer -= Time.deltaTime;
            }
            else
            {
                _rigidbody.AddForce(CalculateNormalizedSlopeDirection(_moveDir) * _slideForce, ForceMode.Force);
            }

            if (_currentSlideTimer <= 0) CancelSlide();
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
            State = ThirdPerson.ThirdPerson_Running;
            _desiredMoveSpeed = _runSpeed;
        }

        private void SetWalkState()
        {
            State = ThirdPerson.ThirdPerson_Walking;
            _desiredMoveSpeed = _walkSpeed;
        }

        private void SetCrouchingState()
        {
            State = ThirdPerson.ThirdPerson_Crouching;
            _desiredMoveSpeed = _crouchMovementSpeed;
        }

        private void SetSlidingState()
        {
            State = ThirdPerson.ThirdPerson_Sliding;
            _speedLossFactor = _speedLossMultiplier;

            if (CheckSlope() && _rigidbody.velocity.y < 0.1f) _desiredMoveSpeed = _slideSpeed;
            else _desiredMoveSpeed = _runSpeed;
        }

        private void SetAirState()
        {
            State = ThirdPerson.ThirdPerson_Air;
            if (_desiredMoveSpeed < _runSpeed) _desiredMoveSpeed = _walkSpeed;
            else _desiredMoveSpeed = _runSpeed;
        }

        private void HandleStates()
        {
            if (IsRestricted)
            {
                State = ThirdPerson.ThirdPerson_Restricted;
            }
            else if (IsSliding)
            {
                SetSlidingState();
            }
            else if (IsCrouching)
            {
                SetCrouchingState();
            }
            else if (MovementMap.ThirdPersonMovement.Run.IsPressed() && IsGrounded && !IsCrouching && !IsSliding)
            {
                SetRunState();
            }
            else if (IsGrounded && !IsSliding)
            {
                SetWalkState();
            }
            else if (!IsGrounded)
            {
                SetAirState();
            }

            if (_previousState == ThirdPerson.ThirdPerson_Sliding && CheckSlope()) _keepMomentum = true;
            if (Mathf.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > Mathf.Abs(_walkSpeed - _runSpeed)
                && _currentMoveSpeed != 0 && _keepMomentum)
            {
                StopCoroutine(LerpMoveSpeedKeepMomentum());
                StartCoroutine(LerpMoveSpeedKeepMomentum());
            }
            else
            {
                StopCoroutine(LerpMoveSpeedKeepMomentum());
                _currentMoveSpeed = _desiredMoveSpeed;
            }

            _lastDesiredMoveSpeed = _desiredMoveSpeed;
            _previousState = State;
        }

        private IEnumerator LerpMoveSpeedKeepMomentum()
        {
            float timer = 0;
            float delta = Mathf.Abs(_desiredMoveSpeed - _currentMoveSpeed);
            float startValue = _currentMoveSpeed;

            float speedLossFactor = _speedLossFactor;

            while (timer < delta)
            {
                _currentMoveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, timer / delta);

                if (CheckSlope())
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                    float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                    timer += Time.deltaTime * speedLossFactor * _slopeMomentumDecreaseMultiplier * slopeAngleIncrease;
                }
                else timer += Time.deltaTime * speedLossFactor;
                yield return null;
            }

            _speedLossFactor = 1f;
            _currentMoveSpeed = _desiredMoveSpeed;
            _keepMomentum = false;
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
            if (_currentCameraMode == CameraMode.CameraMode_Isometric) 
            {
                StartCoroutine(SetIsometricCameraAngleC(newPanAngle, rotationTime));
            }
        }        
        
        public void RotateIsometricCamera(float angleToSum, float rotationTime) 
        {
            if (_currentCameraMode == CameraMode.CameraMode_Isometric) 
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

            MovementMap.ThirdPersonMovement.Slide.performed -= ctx => { PerformSlide(); };
            MovementMap.ThirdPersonMovement.Slide.canceled -= ctx => { CancelSlide(); };
        }

        [ContextMenu("Debug RotateIsoCam")]
        public void DebugRotateIsometricCamera90Degrees() 
        {
            if(_currentCameraMode == CameraMode.CameraMode_Isometric) RotateIsometricCamera(90, 1f);
        }
    }
}