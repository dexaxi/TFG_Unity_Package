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
        private readonly Dictionary<Vector2Int, bool> _allowUpdate = new();

        private int _endIdx = 0;
        private int _effectIdx = 0;
        
        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in effects) 
            {
                _speed[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.SPEED_TAG, DialogueConstants.RAINBOW_DEFAULT_SPEED);
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
            _speed.Clear();
        }

        public override void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimationHandler animatorInspector)
        {
            base.UpdateData(effect, textComponent, animatorInspector);
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
                
                for (int i = effect.TextStartIdx; i < _endIdx; i++)
                {
                    var charInfo = animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    Vector2Int instanceIdx = new(_effectIdx, i);

                    var meshInfo = animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
                    Color32 color;
                    bool hasValue = _prevColor.TryGetValue(i, out color);
                    if (!hasValue) 
                    {
                        color = GetRandomColor();
                        _prevColor[i] = color;
                    }

                    bool hasBeenStarted = _allowUpdate.TryGetValue(instanceIdx, out var canUpdate);
                    if (canUpdate || !hasBeenStarted)
                    {
                        color = GetRandomColor();
                        _prevColor[i] = color;
                        _allowUpdate[instanceIdx] = false;
                        StartCoroutine(RunTimer(instanceIdx));
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

        private IEnumerator RunTimer(Vector2Int instanceIdx)
        {
            yield return new WaitForSeconds(_speed[instanceIdx.x]);
            _allowUpdate[instanceIdx] = true;
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