namespace DUJAL.Systems.Dialogue 
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;

    public class WobbleText : MonoBehaviour
    {
        public static bool DO;
        public static IEnumerator WobbleTextC(TextMeshProUGUI textComponent, int startingIndex, int endIndex)
        {
            DO = true;
            while(DO) 
            {
                Debug.Log("Animating Wobble from " + startingIndex + " to " + endIndex);

                var textInfo = textComponent.textInfo;
                for (int i = startingIndex; i < endIndex; ++i)
                {
                    var charInfo = textInfo.characterInfo[i];

                    if (charInfo.isVisible) 
                    {
                        var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                        for (int j = 0; j < 4; ++j)
                        {
                            var origin = vertices[charInfo.vertexIndex + j];
                            vertices[charInfo.vertexIndex + j] = origin + new Vector3(0, Mathf.Sin(Time.time * 2f + origin.x * 0.01f) * 10f, 0);
                        }
                    }
                }

                for (int i = 0; i < textInfo.meshInfo.Length; ++i)
                {
                    var meshinfo = textInfo.meshInfo[i];
                    meshinfo.mesh.vertices = meshinfo.vertices;
                    textComponent.UpdateGeometry(meshinfo.mesh, i);
                    textComponent.ForceMeshUpdate();
                }

                yield return null;
            }
        }
    }
}
