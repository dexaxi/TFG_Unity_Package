using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DUJAL.Systems.Dialogue 
{
    public class DialogueSystemGroupError
    {
        public DialogueSystemError ErrorData { get; set; }
        public List<CustomGroup> Groups { get; set; }

        public DialogueSystemGroupError() 
        {
            ErrorData = new DialogueSystemError();
            Groups = new List<CustomGroup>();
        }
    }
}
