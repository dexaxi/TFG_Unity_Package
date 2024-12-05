namespace DUJAL.Systems.Dialogue.Animations
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using DUJAL.Systems.Utils;
    using DUJAL.Systems.Dialogue.Constants;
    using DUJAL.Systems.Dialogue.Types;
    using DUJAL.Systems.Dialogue.Animations.Utils;

    public class RainbowText : TextEffect
    {
        private readonly Dictionary<int, float> _speed = new();
        private readonly Dictionary<int, Color32> _prevColor = new();
        private readonly Dictionary<int,int> _currentIdxToUpdate = new();
        private readonly Dictionary<int, bool> _allowUpdate = new();

        private int _endIdx = 0;
        private int _effectIdx = 0;
        
        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in effects) 
            {
                _speed[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.SPEED_TAG, DialogueConstants.RAINBOW_DEFAULT_SPEED);
                _allowUpdate[effectIdx] = true;
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
            _allowUpdate.Clear();
            _prevColor.Clear();
            _currentIdxToUpdate.Clear();
            _speed.Clear();
        }

        public override void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimationHandler animatorInspector)
        {
            base.UpdateData(effect, textComponent, animatorInspector);
            int effIdx = 0;
            foreach (EffectInstance eff in effects) 
            {
                _currentIdxToUpdate[effIdx] = eff.TextStartIdx;
                effIdx++;
            }
            _endIdx = effects[_effectIdx].GetTextEndIndex();
        }

        public override void UpdateEffect()
        {
            if (!doAnimate || animationHandler.TextInfo == null)
            {
                return;
            }
            _effectIdx = 0;
            foreach (EffectInstance effect in effects)
            {
                _endIdx = effect.GetTextEndIndex();
                
                if (!_currentIdxToUpdate.TryGetValue(_effectIdx, out var _))
                {
                    _currentIdxToUpdate[_effectIdx] = effect.TextStartIdx;
                }

                for (int i = effect.TextStartIdx; i < _endIdx; i++)
                {
                    var charInfo = animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
                    Color32 color;
                    bool hasValue = _prevColor.TryGetValue(i, out color);
                    if (!hasValue) 
                    {
                        color = GetRandomColor();
                        _prevColor[i] = color;
                    }

                    if (_allowUpdate[_effectIdx] && _currentIdxToUpdate[_effectIdx] == i)
                    {
                        color = GetRandomColor();
                        _prevColor[i] = color;
                        _currentIdxToUpdate[_effectIdx] = CalculateNextIdx(effect);
                        _allowUpdate[_effectIdx] = false;
                        StartCoroutine(RunTimer(_effectIdx));
                    }

                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIdx = charInfo.vertexIndex + j;
                        meshInfo.colors32[vertexIdx] = color;
                    }
                }
                _effectIdx++;
            }
            _effectIdx = 0;
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

            if (animationHandler.TextInfo.characterInfo[nextIdx].character.IsWhitespace())
            {
                _currentIdxToUpdate[_effectIdx]++;
                return CalculateNextIdx(effect);
            }
            else 
            {
                return nextIdx;
            }
        }

        private IEnumerator RunTimer(int effectIdx)
        {
            yield return new WaitForSeconds(_speed[effectIdx]);
            _allowUpdate[effectIdx] = true;
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