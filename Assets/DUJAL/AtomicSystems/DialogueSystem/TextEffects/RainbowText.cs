namespace DUJAL.Systems.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.XR;
    using Cysharp.Threading.Tasks;
    using TMPro;
    using DUJAL.Systems.Utils;

    public class RainbowText : TextEffect
    {
        private const float stop = 0.05f;
        private const int minColor = 0;
        private const int maxColor = 255;
        private float timer = stop + 1.0f;
        private Dictionary<int, Color32> prevColor = new();
        private Dictionary<int,int> currentIndexToUpdate = new();
        private int endIndex = 0;
        private int effectIndex = 0;
        public override void UpdateData(EffectInstance effect, TextMeshProUGUI textComponent, TextAnimatorInspector animatorInspector)
        {
            base.UpdateData(effect, textComponent, animatorInspector);
            int effIdx = 0;
            foreach (EffectInstance eff in _effects) 
            {
                currentIndexToUpdate[effIdx] = eff.TextStartIndex;
                effIdx++;
            }
            endIndex = _effects[effectIndex].GetTextEndIndex();
        }


        public override void UpdateEffect()
        {
            if (!_doAnimate || _animationHandler.TextInfo == null)
            {
                return;
            }
            effectIndex = -1;
            foreach (EffectInstance effect in _effects)
            {
                effectIndex++;
                endIndex = effect.GetTextEndIndex();
                int _;
                if (!currentIndexToUpdate.TryGetValue(effectIndex, out _))
                {
                    currentIndexToUpdate[effectIndex] = effect.TextStartIndex;
                }

                for (int i = effect.TextStartIndex; i < endIndex; i++)
                {
                    timer += Time.deltaTime;
                    var charInfo = _animationHandler.TextInfo.characterInfo[i];
                    if (!charInfo.isVisible)
                    {
                        continue;
                    }

                    var meshInfo = _animationHandler.TextInfo.meshInfo[charInfo.materialReferenceIndex];
                    Color32 color;
                    bool hasValue = prevColor.TryGetValue(i, out color);
                    if (!hasValue) 
                    {
                        color = GetRandomColor();
                        prevColor[i] = color;
                    }

                    if (timer >= stop && currentIndexToUpdate[effectIndex] == i)
                    {
                        color = GetRandomColor();
                        prevColor[i] = color;
                        currentIndexToUpdate[effectIndex] = CalculateNextIdx(effect);
                    }

                    for (int j = 0; j < 4; ++j)
                    {
                        int vertexIndex = charInfo.vertexIndex + j;
                        meshInfo.colors32[vertexIndex] = color;
                    }

                    if (timer > stop)
                    {
                        timer = 0;
                    }
                }
            }
        }

        private int CalculateNextIdx(EffectInstance effect) 
        {
            int currentIndex = currentIndexToUpdate[effectIndex];
            int nextIndex;
            if (currentIndex + 1 >= endIndex)
            {
                nextIndex = effect.TextStartIndex;
            }
            else
            {
                nextIndex = currentIndex + 1;
            }

            if (_animationHandler.TextInfo.characterInfo[nextIndex].character.IsWhitespace())
            {
                currentIndexToUpdate[effectIndex]++;
                return CalculateNextIdx(effect);
            }
            else 
            {
                return nextIndex;
            }
        }

        private Color32 GetRandomColor() 
        {
            Color32[] colors = { Color.red, Color.blue, Color.magenta, Color.yellow, Color.blue, Color.cyan, Color.green};
            int idx1 = Random.Range(0, colors.Length);
            int idx2 = Random.Range(0, colors.Length);
            int lerpVal = Random.Range(0, 1);
            return Color32.Lerp(colors[idx1], colors[idx2], lerpVal);
        }
    }
}