namespace DUJAL.Systems.Dialogue.Animations
{
    using Dialogue.Types;
    using Dialogue.Constants;
    using Dialogue.Animations.Utils;

    using UnityEngine;

    using System.Collections;
    using System.Collections.Generic;

    public class FadeInText : TextEffect
    {
        private readonly Dictionary<Vector3Int, Color32> _targetColor = new();
        private readonly Dictionary<Vector3Int, Color32> _transparentColor = new();
        private readonly Dictionary<Vector3Int, float> _progress = new();
        private readonly Dictionary<Vector3Int, int> _finishedCount = new();
        private readonly Dictionary<int, bool> _hasFinished = new();
        private readonly Dictionary<int, float> _speed = new();

        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in effects)
            {
                _speed[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.SPEED_TAG, DialogueConstants.FADEIN_DEFAULT_SPEED);
                _hasFinished[effectIdx] = false;
                effectIdx++;
            }
        }

        public override void StartAnimation()
        {
            base.StartAnimation();
        }

        public override void StopAnimation()
        {
            base.StopAnimation();
            StopAllCoroutines();
            _hasFinished.Clear();
            _finishedCount.Clear();
            _targetColor.Clear();
            _transparentColor.Clear();
            _progress.Clear();
            _speed.Clear();
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
                if (_hasFinished[effectIdx]) 
                {
                    effectIdx++;
                    continue;
                }

                for (int i = effect.TextStartIdx; i < effect.GetTextEndIndex(); ++i)
                {
                    var charInfo = animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];

                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIdx = charInfo.vertexIndex + j;
                        Color32 originalColor = meshInfo.colors32[vertexIdx];

                        Vector3Int instanceIdx = new (effectIdx, i, vertexIdx);

                        if (!_progress.TryGetValue(instanceIdx, out var _)) 
                        {
                            _progress[instanceIdx] = 0.0f;
                        }

                        if (_progress.TryGetValue(instanceIdx, out float progress) && progress > 1.0f)
                        {
                            _finishedCount[instanceIdx] = 0;
                            if (_finishedCount.Keys.Count == _progress.Keys.Count)
                            {
                                _hasFinished[effectIdx] = true;    
                            }
                            continue;
                        }

                        StartCoroutine(UpdateProgress(instanceIdx, effectIdx));

                        if (!_targetColor.TryGetValue(instanceIdx, out var _)) 
                        {
                            _targetColor[instanceIdx] = originalColor;
                        }
                        if (!_transparentColor.TryGetValue(instanceIdx, out var _))
                        {
                            Color32 transparentColor = new (originalColor.r, originalColor.g, originalColor.b, 0);
                            _transparentColor[instanceIdx] = transparentColor;
                        }

                        Color32 newColor = Color32.Lerp(_transparentColor[instanceIdx], _targetColor[instanceIdx], _progress[instanceIdx]);
                        meshInfo.colors32[vertexIdx] = newColor;
                    }
                }
                effectIdx++;
            }
        }

        private IEnumerator UpdateProgress(Vector3Int instanceIdx, int effectIdx) 
        {
            yield return new WaitForSeconds(_speed[effectIdx]);
            _progress[instanceIdx] += _speed[effectIdx];
        }
    }
}
