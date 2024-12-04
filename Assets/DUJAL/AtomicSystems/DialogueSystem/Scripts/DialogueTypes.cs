namespace DUJAL.Systems.Dialogue.Types
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
        NoChoice = -1,
        Choice1 = 0,
        Choice2 = 1,
        Choice3 = 2,
        Choice4 = 3,
    }

    public enum TextEffects
    {
        Invalid = -1,
        Wobble,
        Rainbow,
        Jitter,
        FadeIn,
    }

    public class EffectInstance
    {
        public EffectInstance()
        {
            TextStartIdx = -1;
            Tag = "";
            Text = "";
            tagType = TextEffects.Invalid;
            RemainingText = "";
            CutoffIdx = -1;
            PendingDeletion = false;
        }

        public int TextStartIdx;
        public string Tag;
        public string Text;
        public TextEffects tagType;
        public string RemainingText;
        public int CutoffIdx;
        public bool PendingDeletion;

        public int GetTextEndIndex()
        {
            return CutoffIdx != -1 ? CutoffIdx : TextStartIdx + Text.Length;
        }

        public bool IsValid() 
        {
            if (tagType == TextEffects.Invalid) return false;
            if (TextStartIdx == -1) return false;
            if (Tag.Length == 0) return false;
            if (Text.Length == 0) return false;
            return true;
        }
    }
}