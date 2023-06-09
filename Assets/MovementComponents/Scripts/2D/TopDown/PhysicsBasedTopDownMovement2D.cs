namespace DUJAL.MovementComponents.PhysicsBased2D
{
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using DUJAL.IndependentComponents.LaunchRigidBody;

    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBasedTopDownMovement2D : MonoBehaviour
    {
        [Header("ComponentSettings")]
        [SerializeField] private bool _allowRotation;
        [SerializeField] private Transform _rotatedTransform;
        [SerializeField] private bool _allowDiagonal;
        
        [Header("MovementSettings")]
        [SerializeField] [Range(0, 200)] private int _walkingSpeed;
        [SerializeField] [Range(0, 200)] private int _runningBoost;

        [Header("Dash Settings")]
        [SerializeField] private bool _dashInDirectionOfLook;
        [SerializeField] [Range(0f, 2f)] private float _dashDuration;
        [SerializeField] [Range(0, 500)] private int _dashForce;
        [SerializeField] [Range(0f, 2f)] private float _dashCooldown;

        private MovementInput _movementMap;
        private Rigidbody2D _rigidbody;

        private Vector2 _movementInput;
        private Vector2 _lookInput;
        private Vector2 _previousValidInput;

        private float _runningSum;
        private float _dashTimer;
        public float ActiveDashCooldown { get; private set; }

        public bool IsDashing { get; private set; }
        private bool _useMouse;
        

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            HandleInput();
        }

        private void FixedUpdate()
        {
            HandleRotation();
            UpdateMovement();
        }

        private void Update()
        {
            HandleDeviceChange();
        }


        private void HandleInput()
        {
            _movementMap = new MovementInput();
            _movementMap.TopDown2D.Enable();

            _movementMap.TopDown2D.Move.performed += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movementMap.TopDown2D.Move.canceled += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movementMap.TopDown2D.Look.performed += ctx => { PerformLook(ctx.ReadValue<Vector2>()); };            
            _movementMap.TopDown2D.Look.canceled += ctx => { PerformLook(ctx.ReadValue<Vector2>()); };            
            
            _movementMap.TopDown2D.Dash.performed += ctx => { PerformDash(); };
            _movementMap.TopDown2D.Run.performed += ctx => { PerformRun(); };
            
            _movementMap.TopDown2D.Run.canceled += ctx => { CancelRun(); };

            InputDevice device = InputSystem.devices.FirstOrDefault();
            if (device is Mouse)
            {
                _useMouse = true;
            }
            else if (device is Gamepad)
            {
                _useMouse = false;
            }

        }

        private void HandleDeviceChange()
        {
            if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) 
                || (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.wasPressedThisFrame))
            {
                _useMouse = true;
            }
            else if (Gamepad.current != null &&
                (Gamepad.current.allControls.Any(x => x is ButtonControl button && x.IsPressed() && !x.synthetic)
                || Gamepad.current.allControls.Any(y => y is StickControl stick && y.IsActuated() && !y.synthetic)))
            {
                _useMouse = false;
            }
        }

        private void PerformMoved(Vector2 movementVector)
        {
            _movementInput = movementVector;
            if (!_allowDiagonal) RestrictDiagonalMovement();
            if (_movementInput.magnitude > 0.1f) _previousValidInput = _movementInput;
        }

        private Vector2 RestrictDiagonalMovement()
        {
            if (_movementInput.sqrMagnitude > 1)
            {
                _movementInput.Normalize();
            }

            if (Mathf.Abs(_movementInput.x) > Mathf.Abs(_movementInput.y))
            {
                _movementInput.y = 0;
            }
            else
            {
                _movementInput.x = 0;
            }
            return _movementInput;
        }

        private void PerformLook(Vector2 lookVector)
        {
            _lookInput = lookVector;
        }

        private void PerformDash()
        {
            if (!IsDashing) 
            {
                Vector2 dashDirection = _dashInDirectionOfLook ? _rotatedTransform.up : _previousValidInput;
                StartCoroutine(PerformDashC(dashDirection));
            }
        }

        private IEnumerator PerformDashC(Vector2 dashDirection) 
        {
            if (ActiveDashCooldown <= 0) 
            {
                StartCoroutine(StartDashCooldown());
                IsDashing = true;
                _dashTimer = _dashDuration;
                LaunchRigidBody.LaunchRigidBody2D(_rigidbody, dashDirection,  _dashForce);
                while (_dashTimer >= 0) 
                {
                    yield return new WaitForSeconds(0.01f);
                    _dashTimer -= 0.01f;
                }
                IsDashing = false;
            }
        }

        private IEnumerator StartDashCooldown() 
        {
            if (!IsDashing) 
            {
                ActiveDashCooldown = _dashCooldown;
                while (ActiveDashCooldown > 0) 
                {
                    yield return new WaitForSeconds(0.01f);
                    ActiveDashCooldown -= 0.01f;
                }
            }
        }

        private void PerformRun()
        {
            _runningSum = _runningBoost;
        }

        private void CancelRun()
        {
            _runningSum = 0;
        }

        private void OnDestroy()
        {
            _movementMap.TopDown2D.Disable();

            _movementMap.TopDown2D.Move.performed -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movementMap.TopDown2D.Move.canceled-= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movementMap.TopDown2D.Look.performed -= ctx => { PerformLook(ctx.ReadValue<Vector2>()); };
            _movementMap.TopDown2D.Look.canceled -= ctx => { PerformLook(ctx.ReadValue<Vector2>()); };
            _movementMap.TopDown2D.Dash.performed -= ctx => { PerformDash(); };
            _movementMap.TopDown2D.Run.performed -= ctx => { PerformRun(); };
            _movementMap.TopDown2D.Run.canceled -= ctx => { CancelRun(); };
        }

        private void UpdateMovement() 
        {
           if(!IsDashing)_rigidbody.velocity = _movementInput * (_walkingSpeed + _runningSum);
        }

        private void HandleRotation() 
        {
            if (!_allowRotation) return;

            if (_useMouse)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = _rotatedTransform.position.z;
                Vector2 lookDirection = (mousePosition - transform.position).normalized;
                _rotatedTransform.up = lookDirection;
            }
            else 
            {
                Vector2 lookDirection = _lookInput.normalized;
                _rotatedTransform.up = Vector2.Lerp(_rotatedTransform.up, lookDirection, 20f * Time.deltaTime);
            }
        }

    }
}