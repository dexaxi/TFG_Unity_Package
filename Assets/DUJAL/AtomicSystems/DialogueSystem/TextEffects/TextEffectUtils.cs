namespace DUJAL.Systems.Dialogue
{
    using DUJAL.Systems.Dialogue.Constants;
    using System;

    public static class TextEffectUtils 
    {
        public static TextEffects GetEnumFromTag(string tag)
        {
            if (tag.Contains(DialogueConstants.wobbleTag)) return TextEffects.Wobble;
            else if (tag.Contains(DialogueConstants.rainbowTag)) return TextEffects.Rainbow;

            return TextEffects.Invalid;
        }

        public static Type GetTypeFromEffect(TextEffects effect)
        {
            switch (effect)
            {
                case TextEffects.Wobble:
                    return typeof(WobbleText);

                case TextEffects.Rainbow:
                    return null;

                default:
                    return null;
            }
        }

        public static bool IsCustomTag(string tag) 
        {
            return GetEnumFromTag(tag) != TextEffects.Invalid;
        }
    }
}
