namespace DUJAL.MovementComponents.DiscreteBased2D
{
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using DUJAL.IndependentComponents.LaunchRigidBody;

    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownMovement2D : MovementComponent
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

        private Vector2 _lookInput;
        private Vector2 _previousValidInput;

        private float _runningSum;
        private float _dashTimer;
        public float ActiveDashCooldown { get; private set; }

        public bool IsDashing { get; private set; }        

        private void Awake()
        {
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
            MovementMap = new MovementInput();
            MovementMap.TopDown2D.Enable();

            MovementMap.TopDown2D.Move.performed += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            MovementMap.TopDown2D.Move.canceled += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            MovementMap.TopDown2D.Look.performed += ctx => { PerformLook(ctx.ReadValue<Vector2>()); };            
            MovementMap.TopDown2D.Look.canceled += ctx => { PerformLook(ctx.ReadValue<Vector2>()); };            
            
            MovementMap.TopDown2D.Dash.performed += ctx => { PerformDash(); };
            MovementMap.TopDown2D.Run.performed += ctx => { PerformRun(); };
            
            MovementMap.TopDown2D.Run.canceled += ctx => { CancelRun(); };
        }

        private void PerformMoved(Vector2 movementVector)
        {
            MovementInput = movementVector;
            if (!_allowDiagonal) RestrictDiagonalMovement();
            if (MovementInput.magnitude > 0.1f) _previousValidInput = MovementInput;
        }

        private Vector2 RestrictDiagonalMovement()
        {
            if (MovementInput.sqrMagnitude > 1)
            {
                MovementInput.Normalize();
            }

            if (Mathf.Abs(MovementInput.x) > Mathf.Abs(MovementInput.y))
            {
                MovementInput.y = 0;
            }
            else
            {
                MovementInput.x = 0;
            }
            return MovementInput;
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
                Vector2 dashTargetPosition = (Vector2)transform.position + dashDirection * _dashForce;
                while (_dashTimer >= 0) 
                {
                    transform.position = Vector2.MoveTowards(transform.position, dashTargetPosition, _dashTimer/_dashDuration);
                    _dashTimer -= Time.deltaTime;
                    yield return null;
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
            MovementMap.TopDown2D.Disable();

            MovementMap.TopDown2D.Move.performed -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            MovementMap.TopDown2D.Move.canceled-= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            MovementMap.TopDown2D.Look.performed -= ctx => { PerformLook(ctx.ReadValue<Vector2>()); };
            MovementMap.TopDown2D.Look.canceled -= ctx => { PerformLook(ctx.ReadValue<Vector2>()); };
            MovementMap.TopDown2D.Dash.performed -= ctx => { PerformDash(); };
            MovementMap.TopDown2D.Run.performed -= ctx => { PerformRun(); };
            MovementMap.TopDown2D.Run.canceled -= ctx => { CancelRun(); };
        }

        private void UpdateMovement() 
        {
            if (!IsDashing) transform.Translate(MovementInput.normalized * (_walkingSpeed + _runningSum) * Time.deltaTime);
        }

        private void HandleRotation() 
        {
            if (!_allowRotation) return;

            if (UseMouse)
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