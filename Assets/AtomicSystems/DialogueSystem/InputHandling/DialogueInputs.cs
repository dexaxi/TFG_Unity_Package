using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Dialogue

{
    public class DialogueInputs : MonoBehaviour
    {
        DialogueInputActions dialogueInputActions;
        InspectorDialogue dialogue;

        private void Start()
        {
            dialogueInputActions = new DialogueInputActions();
            dialogueInputActions.TextBoxActionMap.Enable();

            dialogueInputActions.TextBoxActionMap.NextDialogue.performed += ctx =>
            {

            };  
        }
    }
}
