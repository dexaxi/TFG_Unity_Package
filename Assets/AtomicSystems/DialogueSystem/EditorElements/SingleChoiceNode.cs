using DUJAL.Systems.Dialogue.Constants;
using DUJAL.Systems.Dialogue.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DUJAL.Systems.Dialogue
{
    public class SingleChoiceNode : BaseNode
    {
        public override void Initialize(DialogueSystemGraphView graphView, Vector2 pos)
        {
            base.Initialize(graphView, pos);
            DialogueType = DialogueType.SingleChoice;
            Choices.Add(DialogueConstants.NodeFirstChoicDefaultText);
        }

        public override void Draw()
        {
            base.Draw();

            foreach (string choice in Choices) 
            {
                Port outputChoice = this.CreatePort(choice);
                outputContainer.Add(outputChoice);
            }
            RefreshExpandedState();
        }
    }
}