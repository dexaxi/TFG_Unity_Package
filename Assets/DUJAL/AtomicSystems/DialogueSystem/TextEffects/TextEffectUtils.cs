namespace DUJAL.Systems.Dialogue.Animations.Utils
{
    using DUJAL.Systems.Dialogue.Constants;
    using DUJAL.Systems.Dialogue.Types;
    using System;
    using UnityEngine;

    public static class TextEffectUtils 
    {
        public static TextEffects GetEnumFromTag(string tag)
        {
            if (tag.Contains(DialogueConstants.WobbleTag)) return TextEffects.Wobble;
            else if (tag.Contains(DialogueConstants.RainbowTag)) return TextEffects.Rainbow;
            else if (tag.Contains(DialogueConstants.VertexJitterTag)) return TextEffects.Jitter;
            else if (tag.Contains(DialogueConstants.FadeInTag)) return TextEffects.FadeIn;
            return TextEffects.Invalid;
        }

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
            int paramIdx = tag.IndexOf(searchTag);
            if (paramIdx != -1)
            {
                int paramEndIdx = tag.IndexOf("\"", paramIdx);
                if (paramEndIdx != -1)
                {
                    string param = tag.Substring(paramIdx + searchTag.Length, (paramEndIdx -1) - paramIdx);
                    return float.Parse(param);
                }
            }
            return defaultValue;
        }
    }
}
