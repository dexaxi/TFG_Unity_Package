namespace DUJAL.Systems.Dialogue.Animations
{
    using UnityEngine;
    using DUJAL.Systems.Dialogue.Types;
    using System.Collections.Generic;
    using DUJAL.Systems.Dialogue.Animations.Utils;
    using DUJAL.Systems.Dialogue.Constants;
    using System.Collections;
    using TMPro;
    using UnityEngine.XR;
    using static UnityEditor.Rendering.InspectorCurveEditor;
    using System.Globalization;
    using Unity.IO.LowLevel.Unsafe;

    public class JitterText : TextEffect
    {
        private readonly Dictionary<int, float> _speed = new();
        private readonly Dictionary<int, float> _curve = new();
        private readonly Dictionary<int, float> _angle = new();
        private readonly Dictionary<int, float> _amplitude = new();


        private bool hasTextChanged;
        private bool isRunning;
        public override void GetParamsFromTag()
        {
            base.GetParamsFromTag();
            int effectIdx = 0;
            foreach (EffectInstance effect in effects)
            {
                _speed[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.SPEED_TAG, DialogueConstants.JITTER_DEFAULT_SPEED);
                _curve[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.CURVE_TAG, DialogueConstants.JITTER_DEFAULT_CURVE);
                _angle[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.ANGLE_TAG, DialogueConstants.JITTER_DEFAULT_ANGLE);
                _amplitude[effectIdx] = TextEffectUtils.GetParamFromTag(effect, DialogueConstants.AMPLITUDE_TAG, DialogueConstants.JITTER_DEFAULT_AMPLITUDE);
                effectIdx++;
            }
        }

        public override void UpdateEffect()
        {
            if (!doAnimate || animationHandler.TextInfo == null)
            {
                return;
            }

            if (!isRunning) 
            {
                isRunning = true;
                int effectIdx = 0;
                foreach (EffectInstance effect in effects) 
                {
                    StartCoroutine(AnimateVertexColors(effect, effectIdx));
                    effectIdx++;
                }
            }
        }

        public override void StopAnimation()
        {
            base.StopAnimation();
            isRunning = false;
            StopAllCoroutines();
        }

        void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }

        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj == textComponent)
            { 
                hasTextChanged = true;
            }
        }

        IEnumerator AnimateVertexColors(EffectInstance effect, int effectIdx)
        {
            textComponent.ForceMeshUpdate();
            Matrix4x4 matrix;
            hasTextChanged = true;
            TMP_MeshInfo[] cachedMeshInfo = textComponent.textInfo.CopyMeshInfoVertexData();

            while (true)
            {
                if (hasTextChanged)
                {
                    cachedMeshInfo = textComponent.textInfo.CopyMeshInfoVertexData();

                    hasTextChanged = false;
                }

                for (int i = effect.TextStartIdx; i < effect.GetTextEndIndex(); ++i)
                {
                    TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[i];

                    if (!charInfo.isVisible)
                    { 
                        continue;
                    }

                    int materialIndex = textComponent.textInfo.characterInfo[i].materialReferenceIndex;
                    int vertexIndex = textComponent.textInfo.characterInfo[i].vertexIndex;
                    
                    Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                    Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;
                    Vector3 offset = charMidBasline;
                    Vector3[] destinationVertices = textComponent.textInfo.meshInfo[materialIndex].vertices;

                    for (int j = 0; j < 4; j++) 
                    {
                        int vIdx = vertexIndex + j;
                        destinationVertices[vIdx] = sourceVertices[vIdx] - offset;
                    }

                    Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f) * _amplitude[effectIdx], Random.Range(-.25f, .25f) * _amplitude[effectIdx], 0);
                    matrix = Matrix4x4.TRS(jitterOffset * _curve[effectIdx], Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * _angle[effectIdx]), Vector3.one);

                    for (int j = 0; j < 4; j++) 
                    {
                        int vIdx = vertexIndex + j;
                        destinationVertices[vIdx] = matrix.MultiplyPoint3x4(destinationVertices[vIdx]);
                        destinationVertices[vIdx] += offset;
                    }
                }
                yield return new WaitForSeconds(_speed[effectIdx]);
            }
        }
    }
}
