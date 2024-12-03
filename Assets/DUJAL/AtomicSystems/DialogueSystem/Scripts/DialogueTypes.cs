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
            tagType = TextEffects.Invalid;
            RemainingText = "";
            CutoffIndex = -1;
        }

        public int TextStartIndex;
        public string Tag;
        public string Text;
        public TextEffects tagType;
        public string RemainingText;
        public int CutoffIndex;
        public int GetTextEndIndex()
        {
            int textStartIdx = CutoffIndex != -1 ? CutoffIndex : TextStartIndex;
            return textStartIdx + Text.Length;
        }

        public bool IsValid() 
        {
            if (tagType == TextEffects.Invalid) return false;
            if (TextStartIndex == -1) return false;
            if (Tag.Length == 0) return false;
            if (Text.Length == 0) return false;
            return true;
        }
    }
}