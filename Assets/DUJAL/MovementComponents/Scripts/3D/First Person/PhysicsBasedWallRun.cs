namespace DUJAL.MovementComponents.PhysicsBased3D 
{
    using UnityEngine;

    public class PhysicsBasedWallRun : MonoBehaviour
    {
        [Header("Wall Run Settings")]
        [SerializeField] [Range(0, 500)] private float _wallRunForce;
        [SerializeField] [Range(0, 10)] private float _wallClimbSpeed;
        [SerializeField] [Range(0, 9999)] private float _maxWallRunDuration;

        [Header("Wall Jump Settings")]
        [SerializeField] [Range(0, 200)] private float _wallJumpHorizontalForce;
        [SerializeField] [Range(0, 200)] private float _wallJumpVerticalForce;
        [SerializeField] [Range(0, 1)] private float _minWallJumpDelay;

        [Header("Wall Detection Settings")]
        [SerializeField] [Range(0, 2)] private float _wallRaycastDistance;
        [SerializeField] [Range(0, 10)] private float _minJumpHeight;
        [SerializeField] [Range(0, 1)] private float _exitWallTime;

        [Header("Camera Effect Settings")]
        [SerializeField] [Range(0, 30)] private float _cameraTiltAngle;
        [SerializeField] [Range(0, 200)] private float _wallRunCameraFov;
        
        [Header("Gravity Settings")]
        [SerializeField] private bool _useGravity;
        [SerializeField] [Range(0, 500)] private float _gravityCounterForce;

        public bool UpwardsRunning { get; private set; }
        public bool DownwardsRunning { get; private set; }

        private PhysicsBasedFirstPersonMovement3D _player;
        private Rigidbody _rigidBody;
        private RaycastHit _leftWallHit;
        private RaycastHit _rightWallHit;

        private float _wallRunTimer;
        private float _exitWallTimer;
        private float _startingCameraFov;
        
        private bool _wallLeft;
        private bool _wallRight;
        private bool isExitingWall;

        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _player = GetComponent<PhysicsBasedFirstPersonMovement3D>();

            _startingCameraFov = _player.Camera.fieldOfView;
        }

        private void Update()
        {
            HandleInputs();
            HandleStates();
        }


        private void FixedUpdate()
        {
            CheckWall();
            if (_player.IsWallRunning) WallRunningMovement();
        }

        private void HandleInputs() 
        {
            UpwardsRunning = _player.MovementMap.FPS.Run.IsPressed();
            DownwardsRunning = _player.MovementMap.FPS.Slide.IsPressed();
        }

        private void CheckWall()
        {
            _wallRight = Physics.Raycast(transform.position, _player.Orientation.right, out _rightWallHit, _wallRaycastDistance, _player.WallCollisionLayer);
            _wallLeft = Physics.Raycast(transform.position, -_player.Orientation.right, out _leftWallHit, _wallRaycastDistance, _player.WallCollisionLayer);
        }

        private bool CheckAboveGround()
        {
            return !Physics.Raycast(transform.position, Vector3.down, _minJumpHeight, _player.GroundCollisionLayer);
        }

        private void HandleStates()
        {
            if ((_wallLeft || _wallRight) && _player.MovementInput.y > 0 && CheckAboveGround() && !isExitingWall && _rigidBody.velocity.magnitude > 0.1f)
            {
                HandleWallRunningState();
                HandleWallJump();
            }
            else if (isExitingWall)
            {
                HandleExitingWallState();
            }
            else
            {
                if (_player.IsWallRunning) StopWallRun();
            }
        }

        private void HandleWallRunningState() 
        {
            if (!_player.IsWallRunning) PerformWallRun();

            if (_wallRunTimer > 0) _wallRunTimer -= Time.deltaTime;

            if (_wallRunTimer <= 0 && _player.IsWallRunning)
            {
                isExitingWall = true;
                _exitWallTimer = _exitWallTime;
            }
        }

        private void HandleWallJump() 
        {
            if (_player.MovementMap.FPS.Jump.IsPressed() && _maxWallRunDuration - _wallRunTimer > _minWallJumpDelay) WallJump();
        } 

        private void HandleExitingWallState() 
        {
            if (_exitWallTimer > 0) _exitWallTimer -= Time.deltaTime;

            if (_player.IsWallRunning) StopWallRun();

            if (_exitWallTimer <= 0) isExitingWall = false;
        }

        private void PerformWallRun()
        {
            _player.IsWallRunning = true;

            _wallRunTimer = _maxWallRunDuration;

            _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, 0f, _rigidBody.velocity.z);

            _player.UpdateFOV(_wallRunCameraFov);
            if (_wallLeft) _player.UpdateTilt(-_cameraTiltAngle);
            if (_wallRight) _player.UpdateTilt(_cameraTiltAngle);
        }

        private void WallRunningMovement()
        {
            _rigidBody.useGravity = _useGravity;

            Vector3 wallNormalVector = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;

            Vector3 wallForwardVector = Vector3.Cross(wallNormalVector, transform.up);

            bool checkForwardOrientation = (_player.Orientation.forward - wallForwardVector).magnitude > (_player.Orientation.forward - -wallForwardVector).magnitude;
            wallForwardVector = checkForwardOrientation ? -wallForwardVector : wallForwardVector;

            _rigidBody.AddForce(wallForwardVector * _wallRunForce, ForceMode.Force);

            if (UpwardsRunning) _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, _wallClimbSpeed, _rigidBody.velocity.z);
            if (DownwardsRunning) _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, -_wallClimbSpeed, _rigidBody.velocity.z);

            if (!(_wallLeft && _player.MovementInput.x > 0) && !(_wallRight && _player.MovementInput.x < 0) && Mathf.Abs(_rigidBody.velocity.x) < 0.1f) _rigidBody.AddForce(-wallNormalVector * 100, ForceMode.Force);

            if (_useGravity) _rigidBody.AddForce(transform.up * _gravityCounterForce, ForceMode.Force);
        }

        private void StopWallRun()
        {
            _player.IsWallRunning = false;
            _player.UpdateFOV(_startingCameraFov);
            _player.UpdateTilt(0f);
        }

        private void WallJump()
        {
            isExitingWall = true;
            _exitWallTimer = _exitWallTime;

            Vector3 wallNormalVector = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;

            Vector3 jumpDirectionForce = transform.up * _wallJumpVerticalForce + wallNormalVector * _wallJumpHorizontalForce;

            _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, 0f, _rigidBody.velocity.z);
            _rigidBody.AddForce(jumpDirectionForce, ForceMode.Impulse);
        }
    }

}