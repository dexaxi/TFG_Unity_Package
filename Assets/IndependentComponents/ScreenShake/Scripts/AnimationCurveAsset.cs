using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
//  Animation Curve Asset to store different types of curve.
/// </summary>
[CreateAssetMenu(fileName = "New Animation Curve", menuName = "Animation Curve")]
public class AnimationCurveAsset : ScriptableObject
{
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    public static implicit operator AnimationCurve(AnimationCurveAsset me)
    {
        return me.curve;
    }
    public static implicit operator AnimationCurveAsset(AnimationCurve curve)
    {
        AnimationCurveAsset asset = ScriptableObject.CreateInstance<AnimationCurveAsset>();
        asset.curve = curve;
        return asset;
    }
}
