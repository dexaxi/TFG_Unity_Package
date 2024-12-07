
namespace DUJAL.Systems.Dialogue
{
    using UnityEngine;

    public class DialogueInputs : MonoBehaviour
    {
        private DialogueInputActions _dialogueInputActions;
        private DialogueSystem _dialogue;

        private void Start()
        {
            _dialogueInputActions = new DialogueInputActions();
            _dialogueInputActions.TextBoxActionMap.Enable();

            _dialogueInputActions.TextBoxActionMap.NextDialogue.performed += ctx =>
            {

            };  
        }
    }
}
