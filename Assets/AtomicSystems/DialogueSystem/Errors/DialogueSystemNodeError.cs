using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Dialogue 
{
    public class DialogueSystemNodeError
    {
        public DialogueSystemError ErrorData { get; set; }
        public List<BaseNode> BaseNodes { get; set; }

        public DialogueSystemNodeError() 
        {
            ErrorData = new DialogueSystemError();
            BaseNodes = new List<BaseNode>();
        }
    }
}
