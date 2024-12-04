namespace DUJAL.Systems.Dialogue.Animations
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Types;
    using System.Collections.Generic;
    using DUJAL.Systems.Dialogue.Animations.Utils;
    using DUJAL.Systems.Dialogue.Constants;
    using System.Globalization;
    using TMPro;
    using static UnityEditor.Rendering.InspectorCurveEditor;
    using System.Collections;
    using UnityEngine.UIElements;

    public class JitterText : TextEffect
    {
        private readonly Dictionary<int, float> _speed = new();
        private readonly Dictionary<int, float> _amplitude = new();

        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in _effects)
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
            if (!_doAnimate || _animationHandler.TextInfo == null)
            {
                return;
            }

            int effectIdx = 0;
            foreach (EffectInstance effect in _effects)
            {
                for (int i = effect.TextStartIdx; i < effect.GetTextEndIndex(); ++i)
                {
                    var charInfo = _animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = _animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
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
