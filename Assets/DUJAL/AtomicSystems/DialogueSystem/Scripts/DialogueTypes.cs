namespace DUJAL.Systems.Dialogue 
{
    public enum DialogueType
    {
        SingleChoice,
        MultipleChoice
    }

    public enum AudioStyle
    {
        GibberishAudioStyle,
        DubbedAudioStyle,
        OneSoundAudioStyle
    }

    public enum DialogueChoice
    {
        Choice1 = 0,
        Choice2 = 1,
        Choice3 = 2,
        Choice4 = 3,
    }

    public enum TextEffects
    {
        Invalid = -1,
        Wobble,
        Rainbow
    }

    public class EffectInstance
    {
        public EffectInstance()
        {
            TextStartIndex = -1;
            Tag = "";
            Text = "";
        }

        public int TextStartIndex;
        public string Tag;
        public string Text;

        public int GetTextEndIndex()
        {
            return TextStartIndex + Text.Length;
        }
    }
}