using System.Collections;
using UnityEngine;

public class ScreenShakeFromAnimationCurve3D : MonoBehaviour
{
	public static ScreenShakeFromAnimationCurve3D Instance { get; private set; }

	[Header("Screen Shake Animation Parameters")]
	[SerializeField]
	[Tooltip("This float defines the duration of the shake in seconds")]
	private float shakeDuration;

	[SerializeField]
	[Tooltip("This animation curve represents the strength of the curve during the shake animation")]
	private AnimationCurve shakeStrengthCurve;

	[SerializeField]
	[Tooltip("Power with which the rotation based screenshake will shake")]
	private float rotationPower;

	private bool shaking;
	private void Awake()
    {
		if (Instance == null)
			Instance = this;
    }

    public void ShakeCameraFromCurve() {
		if (!shaking)
		{
			shaking = true;
			StartCoroutine(IShakeCameraFromCurve());
		}
	}
	private IEnumerator IShakeCameraFromCurve()
	{
		Quaternion startRot = transform.rotation;
		float elapsed = 0f;
		while (elapsed < shakeDuration) 
		{
			elapsed += Time.deltaTime;
			float strength = shakeStrengthCurve.Evaluate(elapsed / shakeDuration);
			transform.rotation = Quaternion.Euler(strength * rotationPower * Random.Range(-1, 1),
				strength * rotationPower * Random.Range(-1, 1), strength * rotationPower * Random.Range(-1, 1));
			yield return null;
		}
		shaking = false;
		transform.rotation = startRot;
	}


	public void ShakeCameraFromCurve(AnimationCurve curve, float duration, float power)
	{
		if (!shaking)
		{
			shaking = true;
			StartCoroutine(IShakeCameraFromCurve(curve, duration, power));
		}
	}

	private IEnumerator IShakeCameraFromCurve(AnimationCurve curve, float duration, float power)
	{
		if (curve == null) curve = shakeStrengthCurve;

		Quaternion startRot = transform.rotation;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float strength = curve.Evaluate(elapsed / duration);
			transform.rotation = Quaternion.Euler(strength * power * Random.Range(-1, 1),
				strength * power * Random.Range(-1, 1), strength * power * Random.Range(-1, 1));
			yield return null;
		}
		shaking = false;
		transform.rotation = startRot;
	}


	[ContextMenu("Debug Shake Camera")]
	private void DebugShakeCamera() { StartCoroutine(IShakeCameraFromCurve()); }


}
