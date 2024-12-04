namespace DUJAL.Systems.Dialogue.Animations
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Types;
    using DUJAL.Systems.Dialogue.Animations.Utils;

    public class WobbleText : TextEffect
    {
        private const string SPEED_TAG = "speed=\"";
        private const string AMPLITUDE_TAG = "amplitude=\"";
        private const float DEFAULT_SPEED = 2f;
        private const float DEFAULT_AMPLITUDE = 10f;

        private float _speed;
        private float _amplitude;

        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            foreach (EffectInstance effect in _effects)
            { 
                _speed = TextEffectUtils.GetParamFromTag(effect, SPEED_TAG, DEFAULT_SPEED);
                _amplitude = TextEffectUtils.GetParamFromTag(effect, AMPLITUDE_TAG, DEFAULT_AMPLITUDE);
            }
        }

        public override void UpdateEffect()
        {
            if (!_doAnimate || _animationHandler.TextInfo == null) 
            {
                return;
            }

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
                        float newOrigY = Mathf.Sin(Time.time * _speed + verts[vertexIdx].x * 0.01f) * _amplitude;
                        verts[vertexIdx] = verts[vertexIdx] + new Vector3(0f, newOrigY, 0f);
                    }
                }
            }
        }
    }
}
