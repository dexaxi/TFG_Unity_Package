namespace DUJAL.MovementComponents.PhysicsBased2D
{
    using System.Collections;
    using UnityEngine;
    using DUJAL.IndependentComponents.LaunchRigidBody;

    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBasedScrollMovement2D : MonoBehaviour
    {
        [Header("Collision Settings")]
        [SerializeField] private LayerMask _groundCollisionMask;
        [SerializeField] private LayerMask _wallJumpMask;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _wallJumpCheck;

        [Header("Movement Settings")]
        [SerializeField] [Range(0, 200)] int _walkingSpeed;
        [SerializeField] [Range(0, 200)] int _runningBoost;
        [SerializeField] [Range(0, 2f)] float _airMovementMultiplier;

        [Header("Jump Settings")]
        [SerializeField] [Range(0, 50)] int _jumpForce;
        [SerializeField] [Range(0, 50)] int _wallJumpForce;
        [SerializeField] Vector2 _wallJumpVerticality;
        [SerializeField] [Range(0, 1)] float _wallJumpDelay;

        [Header("WallHang Settings")]
        [SerializeField] float _wallHangDelay;
        [SerializeField] [Range(0, 1)] float _wallHangStrength;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingWall { get; private set; }

        private Rigidbody2D _rigidbody;
        private MovementInput _movement;

        private Vector2 _movementInput;
        private Vector2 _localScale;

        private float _runningSum;
        private int _wallJumpDirection;

        private float _currentWaitForLoseHang;
        private bool _hang;

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
            UpdateVelocity();
            UpdateGrounded();
            UpdateWallJump();
            HandleAutoFlipCharacter();
            HandleGrip();
        }

        private void HandleInput()
        {
            _movement.Scroll2D.Movement.performed += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movement.Scroll2D.Movement.canceled += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movement.Scroll2D.Run.performed += ctx => { PerformStartRunning(); };
            _movement.Scroll2D.Run.canceled += ctx => { PerformStopRunning(); };

            _movement.Scroll2D.Jump.performed += ctx => { PerformJump(); };
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
            else if (IsTouchingWall)
            {
                StartCoroutine(DisableJumpInput(_wallJumpDelay));
                LaunchRigidBody.LaunchRigidBody2D(_rigidbody, new Vector2(_wallJumpDirection * _wallJumpVerticality.x, 1 * _wallJumpVerticality.y), _wallJumpForce);
            }
        }

        private IEnumerator DisableJumpInput(float delay)
        {
            _movement.Scroll2D.Jump.Disable();
            yield return new WaitForSeconds(delay);
            _movement.Scroll2D.Jump.Enable();
        }

        private void PerformStartRunning()
        {
            if (IsGrounded) _runningSum = _runningBoost;
        }

        private void PerformStopRunning()
        {
            _runningSum = 0;
        }

        private void UpdateVelocity()
        {
            if (_movement.Scroll2D.Run.IsPressed() && IsGrounded) _runningSum = _runningBoost;
            else if (!IsGrounded) _runningSum = 0;

            float velocityXCoord = _movementInput.normalized.x * (_walkingSpeed + _runningSum);

            RaycastHit2D hit = Physics2D.Raycast(_wallJumpCheck.position, _movementInput, RAYCAST_DISTANCE, _wallJumpMask);
            if (hit.collider != null && !_hang)
            {
                velocityXCoord = 0f;
            }

            if (IsGrounded) _rigidbody.AddForce(new Vector2(velocityXCoord, 0));
            else _rigidbody.AddForce(new Vector2(velocityXCoord * _airMovementMultiplier, 0));

        }

        private void HandleGrip()
        {

            if (IsTouchingWall && !IsGrounded)
            {
                StartCoroutine(HandleGripC());
            }
            else
            {
                _currentWaitForLoseHang = 0f;
                _hang = true;
            }

            if (_hang && IsTouchingWall && !IsGrounded && Mathf.Abs(_movementInput.x) > 0.1)
            {
                _rigidbody.AddForce(new Vector2(0, _wallHangStrength), ForceMode2D.Impulse);
            }
        }

        private IEnumerator HandleGripC()
        {
            _currentWaitForLoseHang = _wallHangDelay;
            while (_currentWaitForLoseHang >= 0)
            {
                yield return new WaitForSeconds(0.01f);
                _currentWaitForLoseHang -= 0.01f;
            }
            _hang = false;
        }

        private void UpdateGrounded()
        {
            RaycastHit2D hit = Physics2D.Raycast(_groundCheck.position, Vector3.down, RAYCAST_DISTANCE, _groundCollisionMask.value);
            IsGrounded = hit.collider != null;
        }

        private void UpdateWallJump()
        {
            RaycastHit2D hitLeft = Physics2D.Raycast(_wallJumpCheck.position, Vector2.left, RAYCAST_DISTANCE, _wallJumpMask);
            RaycastHit2D hitRight = Physics2D.Raycast(_wallJumpCheck.position, Vector2.right, RAYCAST_DISTANCE, _wallJumpMask);
            IsTouchingWall = hitLeft.collider != null || hitRight.collider != null;
            _wallJumpDirection = hitRight.collider != null ? -1 : 1;
        }

        private void HandleAutoFlipCharacter()
        {
            if (_rigidbody.velocity.x < 0) transform.localScale = new Vector2(-_localScale.x, _localScale.y);
            else if (_rigidbody.velocity.x > 0) transform.localScale = new Vector2(_localScale.x, _localScale.y);
        }
        private void OnDestroy()
        {
            _movement.Disable();
            _movement.Scroll2D.Movement.performed -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movement.Scroll2D.Movement.canceled -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movement.Scroll2D.Run.performed -= ctx => { PerformStartRunning(); };
            _movement.Scroll2D.Run.canceled -= ctx => { PerformStopRunning(); };

            _movement.Scroll2D.Jump.performed -= ctx => { PerformJump(); };
        }

    }
}
