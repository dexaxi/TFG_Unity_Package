namespace DUJAL.Systems.Dialogue.Animations.Utils
{
    using Dialogue.Types;
    using Dialogue.Constants;

    using UnityEngine;

    using System;

    public static class TextEffectUtils 
    {

        // New enum values must be added here.
        public static TextEffects GetEnumFromTag(string tag)
        {
            if (tag.Contains(DialogueConstants.WobbleTag)) return TextEffects.Wobble;
            else if (tag.Contains(DialogueConstants.RainbowTag)) return TextEffects.Rainbow;
            else if (tag.Contains(DialogueConstants.VertexJitterTag)) return TextEffects.Jitter;
            else if (tag.Contains(DialogueConstants.FadeInTag)) return TextEffects.FadeIn;
            return TextEffects.Invalid;
        }

        // New Enum values + effect types must be added here
        public static Type GetTypeFromEffect(TextEffects effect)
        {
            switch (effect)
            {
                case TextEffects.Wobble:
                    return typeof(WobbleText);

                case TextEffects.Rainbow:
                    return typeof(RainbowText);

                case TextEffects.Jitter:
                    return typeof(JitterText);

                case TextEffects.FadeIn:
                    return typeof(FadeInText);

                case TextEffects.Invalid:
                default:
                    Debug.LogError("Trying to apply an invalid text effect");
                    return null;
            }
        }

        public static bool IsCustomTag(string tag) 
        {
            return GetEnumFromTag(tag) != TextEffects.Invalid;
        }

        public static int GetParamFromTag(EffectInstance effect, string searchTag, int defaultValue)
        {
            return (int) GetParamFromTag(effect, searchTag, (float) defaultValue);
        }

        public static float GetParamFromTag(EffectInstance effect, string searchTag, float defaultValue) 
        {
            string tag = effect.Tag;
            int tagStartIdx = tag.IndexOf(searchTag);
            if (tagStartIdx != -1)
            {
                int tagEndIdx = tagStartIdx + searchTag.Length;
                int paramEndIdx = tag.IndexOf("\"", tagEndIdx);
                if (paramEndIdx != -1)
                {
                    int paramStartIdx = tagStartIdx + searchTag.Length;
                    string param = tag.Substring(paramStartIdx, paramEndIdx - paramStartIdx);
                    return float.Parse(param);
                }
            }
            return defaultValue;
        }
    }
}
