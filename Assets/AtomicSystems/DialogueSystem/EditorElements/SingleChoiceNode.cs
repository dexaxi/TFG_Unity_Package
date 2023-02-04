using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DUJAL.Systems.Dialogue
{
    public class SingleChoiceNode : BaseNode
    {
        public override void Initialize(Vector2 pos)
        {
            base.Initialize(pos);
            DialogueType = DialogueType.SingleChoice;
            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            foreach (string choice in Choices) 
            {
                Port outputChoice = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                outputChoice.portName = choice;
                outputContainer.Add(outputChoice);
            }
            RefreshExpandedState();
        }
    }
}