namespace DUJAL.IndependentComponents.Interactables 
{
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(SphereCollider))]
    public class Interactable3D : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _collisionRadius;
        [SerializeField] private bool _canInteract;
        [SerializeField] private bool _toggleOnce;

        [Header("Events")]
        [Tooltip("To execute an event on interaction, subscribe to this event")]
        [SerializeField] public UnityEvent Interact;
        
        private SphereCollider _collider;
        private bool _toggled;
        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.radius = _collisionRadius;
            _collider.isTrigger = true;
        }


        public void UpdateInteractableRadius(float newRadius) 
        {
            _collisionRadius = newRadius;
            _collider.radius = _collisionRadius;
        }

        public void ExecuteInteract() 
        {
            if (_canInteract && ((_toggleOnce && !_toggled) || !_toggleOnce)) 
            {
                Interact.Invoke();
                _toggled = true;
            }
        }

        public float GetCollisionRadius() { return _collisionRadius; }

        public void DisableInteraction() { _canInteract = false; }
        public void EnableInteraction() { _canInteract = true; }

        public void ToggleInteraction() { _canInteract = !_canInteract;  }

        public void ResetToggleOnce() { _toggled = false; }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _collisionRadius * transform.lossyScale.x);
        }
    }
}
