namespace DUJAL.MovementComponents.PhysicsBased2D
{
    using System.Collections;
    using UnityEngine;
    using DUJAL.IndependentComponents.LaunchRigidBody;

    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrollMovement2D : MonoBehaviour
    {
        [Header("Collision Settings")]
        [SerializeField] private LayerMask _groundCollisionMask;
        [SerializeField] private Transform _groundCheck;

        [Header("Movement Settings")]
        [SerializeField] [Range(0, 200)] int _walkingSpeed;
        [SerializeField] [Range(0, 200)] int _runningBoost;
        [SerializeField] [Range(0, 2f)] float _airMovementMultiplier;

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
            _movement.PhysicsBasedScroll2D.Enable();
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
            _movement.PhysicsBasedScroll2D.Movement.performed += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movement.PhysicsBasedScroll2D.Movement.canceled += ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movement.PhysicsBasedScroll2D.Run.performed += ctx => { PerformStartRunning(); };
            _movement.PhysicsBasedScroll2D.Run.canceled += ctx => { PerformStopRunning(); };

            _movement.PhysicsBasedScroll2D.Jump.performed += ctx => { PerformJump(ctx.started); };
        }

        private void PerformMoved(Vector2 movementVector)
        {
            _movementInput = movementVector;
        }

        private void PerformJump(bool started)
        {
            if (IsGrounded)
            {
                LaunchRigidBody.LaunchRigidBody2D(_rigidbody, new Vector2(0, 1), _jumpForce);
            }
        }

        private IEnumerator DisableJumpInput(float delay)
        {
            _movement.PhysicsBasedScroll2D.Jump.Disable();
            yield return new WaitForSeconds(delay);
            _movement.PhysicsBasedScroll2D.Jump.Enable();
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
            if (_movement.PhysicsBasedScroll2D.Run.IsPressed() && IsGrounded) _runningSum = _runningBoost;
            else if (!IsGrounded) _runningSum = 0;
            
            
        }


        private void UpdateGrounded()
        {
            RaycastHit2D hit = Physics2D.Raycast(_groundCheck.position, Vector3.down, RAYCAST_DISTANCE, _groundCollisionMask.value);
            IsGrounded = hit.collider != null;
        }

        private void HandleAutoFlipCharacter()
        {
            if (_rigidbody.velocity.x < 0) transform.localScale = new Vector2(-_localScale.x, _localScale.y);
            else if (_rigidbody.velocity.x > 0) transform.localScale = new Vector2(_localScale.x, _localScale.y);
        }

        private void OnDestroy()
        {
            _movement.Disable();
            _movement.PhysicsBasedScroll2D.Movement.performed -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };
            _movement.PhysicsBasedScroll2D.Movement.canceled -= ctx => { PerformMoved(ctx.ReadValue<Vector2>()); };

            _movement.PhysicsBasedScroll2D.Run.performed -= ctx => { PerformStartRunning(); };
            _movement.PhysicsBasedScroll2D.Run.canceled -= ctx => { PerformStopRunning(); };

            _movement.PhysicsBasedScroll2D.Jump.performed -= ctx => { PerformJump(ctx.started); };
        }
    }
}
