
namespace DUJAL.IndependentComponents.Interactables 
{   
    using UnityEngine;
    public class Interactor3D : MonoBehaviour
    {
        private InteractionMap _inputActions;
        private Interactable3D _interactable;
        private void Awake()
        {
            _inputActions = new InteractionMap();
            _inputActions.Interact.Interact.Enable();
            _inputActions.Interact.Interact.performed += ctx => { Interact(); };
        }

        private void Interact()
        {
            _interactable?.ExecuteInteract();
        }

        private void OnTriggerEnter(Collider other)
        {
            Interactable3D interactable = other.GetComponent<Interactable3D>();
            if (interactable != null)
            {
                _interactable = interactable;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Interactable3D interactable = other.GetComponent<Interactable3D>();
            if (interactable == _interactable)
            {
                _interactable = null;
            }
        }
    }
}
