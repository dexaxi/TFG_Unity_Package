namespace DUJAL.Systems.Dialogue.Animations
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Types;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    public class FadeInText : TextEffect
    {
        private readonly Dictionary<Vector2Int, Color32> _targetColor = new();
        private readonly Dictionary<Vector2Int, Color32> _transparentColor = new();
        private readonly Dictionary<Vector2Int, float> _progress = new();
        private readonly Dictionary<Vector2Int, int> _progressCount = new();

        private bool _hasFinished;
        private float _speed = 0.005f;

        public override void StartAnimation()
        {
            base.StartAnimation();
        }

        public override void StopAnimation()
        {
            base.StopAnimation();
            _hasFinished = false;
            _progressCount.Clear();
            _targetColor.Clear();
            _transparentColor.Clear();
            _progress.Clear();
        }

        public override void UpdateEffect()
        {
            if (_hasFinished) return;

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
                        Color32 originalColor = meshInfo.colors32[vertexIdx];

                        Vector2Int instanceIdx = new (i, vertexIdx);

                        if (!_progress.TryGetValue(instanceIdx, out var _)) 
                        {
                            _progress[instanceIdx] = 0.0f;
                        }

                        if (_progress.TryGetValue(instanceIdx, out float progress) && progress > 1.0f)
                        {
                            _progressCount[instanceIdx] = 0;
                            if (_progressCount.Keys.Count == _progress.Keys.Count)
                            {
                                _hasFinished = true;    
                            }
                            continue;
                        }

                        StartCoroutine(UpdateProgress(instanceIdx));

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
            }
        }

        private IEnumerator UpdateProgress(Vector2Int instanceIdx) 
        {
            yield return new WaitForSeconds(_speed);
            _progress[instanceIdx] += _speed;
        }
    }
}
