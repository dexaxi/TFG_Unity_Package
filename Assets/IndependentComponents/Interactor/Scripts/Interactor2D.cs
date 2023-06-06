
namespace DUJAL.IndependentComponents.Interactables 
{   
    using UnityEngine;
    public class Interactor2D : MonoBehaviour
    {
        private InteractionMap _inputActions;
        private Interactable2D _interactable;
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Interactable2D interactable = collision.GetComponent<Interactable2D>();
            if (interactable != null)
            {
                _interactable = interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Interactable2D interactable = collision.GetComponent<Interactable2D>();
            if (interactable == _interactable) 
            {
                _interactable = null;
            }
        }
    }
}
