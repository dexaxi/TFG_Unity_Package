namespace DUJAL.Systems.Dialogue 
{
    using System.Collections.Generic;

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
