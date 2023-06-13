namespace DUJAL.MovementComponents.DiscreteBased2D
{
    using UnityEngine;
    using DUJAL.IndependentComponents.LaunchRigidBody;

    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrollMovement2D : MonoBehaviour
    {
        [Header("Collision Settings")]
        [SerializeField] private LayerMask _groundCollisionMask;
        [SerializeField] private Transform _groundCheck;

        [Header("Movement Settings")]
        [SerializeField] private bool _disableJumping;
        [SerializeField] [Range(0, 200)] int _walkingSpeed;
        [SerializeField] [Range(0, 200)] int _runningBoost;

        [Header("Jump Settings")]
        [SerializeField] [Range(0, 50)] int _jumpForce;

        public bool IsGrounded { get; private set; }

        private Rigidbody2D _rigidbody;
        private MovementInput _movement;

        private Vector2 _movementInput;
        private Vector2 _localScale;

        private float _runningSum;

        private const float RAYCAST_DISTANCE = 0.1f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _movement = new MovementInput();
            _movement.Scroll2D.Enable();
            _localScale = transform.localScale;
            HandleInput();
        }

        private void FixedUpdate()
        {
            UpdateMovement();
            UpdateGrounded();
            HandleAutoFlipCharacter();
        }

        private void HandleInput()
        {
            _movement.Scroll2D.Movement.performed += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movement.Scroll2D.Movement.canceled += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movement.Scroll2D.Run.performed += ctx => { PerformStartRunning(); };
            _movement.Scroll2D.Run.canceled += ctx => { PerformStopRunning(); };

            if(!_disableJumping) EnableJumping();
        }

        private void PerformMoved(Vector2 movementVector)
        {
            _movementInput = movementVector;
        }

        private void PerformJump()
        {
            if (IsGrounded)
            {
                LaunchRigidBody.LaunchRigidBody2D(_rigidbody, new Vector2(0, 1), _jumpForce);
            }
        }

        private void PerformStartRunning()
        {
            if (IsGrounded) _runningSum = _runningBoost;
        }

        private void PerformStopRunning()
        {
            _runningSum = 0;
        }

        private void UpdateMovement()
        {
            if (_movement.Scroll2D.Run.IsPressed() && IsGrounded) _runningSum = _runningBoost;
            else if (!IsGrounded) _runningSum = 0;

            transform.Translate(_movementInput.normalized * (_walkingSpeed + _runningSum) * Time.deltaTime);
        }


        private void UpdateGrounded()
        {
            RaycastHit2D hit = Physics2D.Raycast(_groundCheck.position, Vector3.down, RAYCAST_DISTANCE, _groundCollisionMask.value);
            IsGrounded = hit.collider != null;
        }

        private void HandleAutoFlipCharacter()
        {
            if (_movementInput.x < 0) transform.localScale = new Vector2(-_localScale.x, _localScale.y);
            else if (_movementInput.x > 0) transform.localScale = new Vector2(_localScale.x, _localScale.y);
        }

        public void DisableJumping() 
        {
            _movement.Scroll2D.Jump.performed -= ctx => { PerformJump(); };
        }

        public void EnableJumping() 
        {
            _movement.Scroll2D.Jump.performed += ctx => { PerformJump(); };
        }


        private void OnDestroy()
        {
            _movement.Disable();
            _movement.Scroll2D.Movement.performed -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movement.Scroll2D.Movement.canceled -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movement.Scroll2D.Run.performed -= ctx => { PerformStartRunning(); };
            _movement.Scroll2D.Run.canceled -= ctx => { PerformStopRunning(); };

            DisableJumping();
        }

    }
}
