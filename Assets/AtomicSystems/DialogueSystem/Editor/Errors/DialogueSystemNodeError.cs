namespace DUJAL.Systems.Dialogue 
{
    using System.Collections.Generic;
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
