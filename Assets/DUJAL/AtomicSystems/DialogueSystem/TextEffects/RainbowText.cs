namespace DUJAL.Systems.Dialogue.Animations
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using DUJAL.Systems.Utils;
    using DUJAL.Systems.Dialogue.Constants;
    using DUJAL.Systems.Dialogue.Types;

    public class RainbowText : TextEffect
    {
        private const int _minColor = 0;
        private const int _maxColor = 255;
        private int _endIdx = 0;
        private int _effectIdx = 0;
        private bool _allowUpdate = true;
        private float _speed = 0.025f;

        private Dictionary<int, Color32> _prevColor = new();
        private Dictionary<int,int> _currentIdxToUpdate = new();

        public override void StartAnimation()
        {
            base.StartAnimation();
        }

        public override void StopAnimation()
        {
            base.StopAnimation();
            _allowUpdate = true;
            _prevColor.Clear();
            _currentIdxToUpdate.Clear();
        }

        public override void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimationHandler animatorInspector)
        {
            base.UpdateData(effect, textComponent, animatorInspector);
            int effIdx = 0;
            foreach (EffectInstance eff in _effects) 
            {
                _currentIdxToUpdate[effIdx] = eff.TextStartIdx;
                effIdx++;
            }
            _endIdx = _effects[_effectIdx].GetTextEndIndex();
        }

        public override void UpdateEffect()
        {
            if (!_doAnimate || _animationHandler.TextInfo == null)
            {
                return;
            }
            _effectIdx = -1;
            foreach (EffectInstance effect in _effects)
            {
                _effectIdx++;
                _endIdx = effect.GetTextEndIndex();
                int _;
                if (!_currentIdxToUpdate.TryGetValue(_effectIdx, out _))
                {
                    _currentIdxToUpdate[_effectIdx] = effect.TextStartIdx;
                }

                for (int i = effect.TextStartIdx; i < _endIdx; i++)
                {
                    var charInfo = _animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = _animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
                    Color32 color;
                    bool hasValue = _prevColor.TryGetValue(i, out color);
                    if (!hasValue) 
                    {
                        color = GetRandomColor();
                        _prevColor[i] = color;
                    }

                    if (_allowUpdate && _currentIdxToUpdate[_effectIdx] == i)
                    {
                        color = GetRandomColor();
                        _prevColor[i] = color;
                        _currentIdxToUpdate[_effectIdx] = CalculateNextIdx(effect);
                        _allowUpdate = false;
                        StartCoroutine(RunTimer());
                    }

                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIdx = charInfo.vertexIndex + j;
                        meshInfo.colors32[vertexIdx] = color;
                    }
                }
            }
        }

        private void Update()
        {
 
        }

        private int CalculateNextIdx(EffectInstance effect) 
        {
            int currentIdx = _currentIdxToUpdate[_effectIdx];
            int nextIdx;
            if (currentIdx + 1 >= _endIdx)
            {
                nextIdx = effect.TextStartIdx;
            }
            else
            {
                nextIdx = currentIdx + 1;
            }

            if (_animationHandler.TextInfo.characterInfo[nextIdx].character.IsWhitespace())
            {
                _currentIdxToUpdate[_effectIdx]++;
                return CalculateNextIdx(effect);
            }
            else 
            {
                return nextIdx;
            }
        }

        private IEnumerator RunTimer()
        {
            yield return new WaitForSeconds(_speed);
            _allowUpdate = true;
        }

        private Color32 GetRandomColor() 
        {
            Color32[] colors = DialogueConstants.RainbowOptions;
            int idx1 = Random.Range(0, colors.Length);
            int idx2 = Random.Range(0, colors.Length);
            float lerpVal = Random.Range(0.0f, 1.0f);
            return Color32.Lerp(colors[idx1], colors[idx2], lerpVal);
        }
    }
}