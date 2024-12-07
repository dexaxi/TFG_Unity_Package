namespace DUJAL.Systems.Dialogue.Animations
{
    using Dialogue.Types;
    using Dialogue.Constants;
    using Dialogue.Animations.Utils;

    using UnityEngine;
    using TMPro;

    using System.Collections.Generic;

    public class JitterText : TextEffect
    {
        private const int MAX_DISTANCE = 9999999;

        private readonly Dictionary<int, float> _speed = new();
        private readonly Dictionary<int, float> _amplitude = new();

        private readonly Dictionary<int, float> _elapsed = new();
        private readonly Dictionary<int, Vector3[]> _prevPos = new();

        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in effects)
            {
                _speed[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.SPEED_TAG, DialogueConstants.JITTER_DEFAULT_SPEED);
                _amplitude[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.AMPLITUDE_TAG, DialogueConstants.JITTER_DEFAULT_AMPLITUDE);
                _elapsed[effectIdx] = 0.0f;
                effectIdx++;
            }
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
                AnimateVertexColors(effect, effectIdx);
                if (_elapsed[effectIdx] >= 1.0f)
                {
                    _elapsed[effectIdx] = 0.0f;
                }
                else 
                {
                    _elapsed[effectIdx]  += Time.deltaTime * _speed[effectIdx]; 
                }
                effectIdx++;
            }
        }

        public override void StopAnimation()
        {
            base.StopAnimation();
            _elapsed.Clear();
            _speed.Clear();
            _amplitude.Clear();
            _prevPos.Clear();
        }

        private void AnimateVertexColors(EffectInstance effect, int effectIdx)
        {

            for (int i = effect.TextStartIdx; i < effect.GetTextEndIndex(); ++i)
            {
                TMP_CharacterInfo charInfo = animationHandler.TextInfo.characterInfo[i];

                if (!charInfo.isVisible)
                { 
                    continue;
                }

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIdx = charInfo.vertexIndex;
                TMP_MeshInfo meshInfo = animationHandler.TextInfo.meshInfo[materialIndex];
                Vector3[] verts = meshInfo.vertices;

                Vector3[] newVertPos = GetJitteredVertices(effectIdx, vertexIdx, verts);
                for (int j = 0; j < 4; j++) 
                {
                    int vIdx = vertexIdx + j;
                    verts[vIdx] = newVertPos[j];
                }
            }
        }

        private Vector3[] GetJitteredVertices(int effectIdx, int vertexIdx, Vector3[] vertInfo)
        {
            Vector3[] returnVecs = new Vector3[4];
            float amplitudeAmount = _amplitude[effectIdx];
            float elapsedAmount = _elapsed[effectIdx];

            Vector3 jitterOffset;
            if (elapsedAmount <= 1.0f && _prevPos.TryGetValue(vertexIdx, out var _))
            {
                return _prevPos[vertexIdx];
            }

            float jitterX = Random.Range(-.25f, .25f) * amplitudeAmount;
            float jitterY = Random.Range(-.25f, .25f) * amplitudeAmount;
            jitterOffset = new (jitterX, jitterY, 0);

            for (int j = 0; j < 4; j++) 
            {
                int vIdx = vertexIdx + j;
                Vector3 originalVPos = vertInfo[vIdx];
                Vector3 destPos = originalVPos + jitterOffset;

                Vector3 currPos = Vector3.MoveTowards(originalVPos, destPos, MAX_DISTANCE);

                returnVecs[j] = currPos;
            }

            _prevPos[vertexIdx] = returnVecs;
            return returnVecs;
        }
    }
}
