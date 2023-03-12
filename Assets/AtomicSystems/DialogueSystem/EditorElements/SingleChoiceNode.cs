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
            ChoiceSaveData choiceData= new ChoiceSaveData() 
            {
                Text = DialogueConstants.NodeFirstChoicDefaultText
            };
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            foreach (ChoiceSaveData choice in Choices) 
            {
                Port outputChoice = this.CreatePort(choice.Text);
                outputChoice.userData = choice;
                outputContainer.Add(outputChoice);
            }
            RefreshExpandedState();
        }
    }
}