namespace DUJAL.MovementComponents.PhysicsBased2D
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBasedTopDownMovement2D : MonoBehaviour
    {
        [Header("ComponentSettings")]
        [SerializeField] private bool _allowRotation;
        [SerializeField] private Transform _rotatedTransform;
        [SerializeField] private bool _allowDiagonal;
        

        [Header("MovementSettings")]
        [SerializeField] private float _walkingSpeed;
        [SerializeField] private float _runningBoost;


        private MovementInput _movementMap;
        private Rigidbody2D _rigidbody;

        private Vector2 _movementInput;
        private Vector2 _lookInput;
        private Vector2 _localScale;

        private float _runningSum;
        private bool _useMouse;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _localScale = transform.localScale;

            HandleInput();
        }

        private void FixedUpdate()
        {
            HandleRotation();
            UpdateMovement();
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

            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Mouse || device is Gamepad)
            {
                if (device == Mouse.current)
                {
                    _useMouse = true;
                }
                else if (device == Gamepad.current)
                {
                    _useMouse = false;
                }
            }
        }

        private void PerformMoved(Vector2 movementVector)
        {
            _movementInput = movementVector;
        } 
        
        private void PerformLook(Vector2 lookVector)
        {
            _lookInput = lookVector;
        }

        private void PerformDash()
        {
            Debug.Log("DASH!");
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
            
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void UpdateMovement() 
        {
            _rigidbody.velocity = _movementInput * (_walkingSpeed + _runningSum);
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