namespace DUJAL.Systems.Dialogue.Animations
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Types;
    using DUJAL.Systems.Dialogue.Animations.Utils;
    using System.Collections.Generic;
    using DUJAL.Systems.Dialogue.Constants;

    public class WobbleText : TextEffect
    {
        private readonly Dictionary<int, float> _speed = new();
        private readonly Dictionary<int,float> _amplitude = new();

        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in effects)
            {
                _speed[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.SPEED_TAG, DialogueConstants.WOBBLE_DEFAULT_SPEED);
                _amplitude[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.AMPLITUDE_TAG, DialogueConstants.WOBBLE_DEFAULT_AMPLITUDE);
                effectIdx++;
            }
        }

        public override void StopAnimation()
        {
            base.StopAnimation();
            _speed.Clear();
            _amplitude.Clear();
        }

        public override void UpdateEffect()
        {
            if (!doAnimate || animationHandler.TextInfo == null) 
            {
                return;
            }

            int effectIdx = 0;
            foreach (EffectInstance effect in effects) 
            {
                for (int i = effect.TextStartIdx; i < effect.GetTextEndIndex(); ++i)
                {
                    var charInfo = animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }
                    
                    var meshInfo = animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
                    var verts = meshInfo.vertices;
                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIdx = charInfo.vertexIndex + j;
                        float newOrigY = Mathf.Sin(Time.time * _speed[effectIdx] + verts[vertexIdx].x * 0.01f) * _amplitude[effectIdx];
                        verts[vertexIdx] = verts[vertexIdx] + new Vector3(0f, newOrigY, 0f);
                    }
                }
                effectIdx++;
            }
        }
    }
}
