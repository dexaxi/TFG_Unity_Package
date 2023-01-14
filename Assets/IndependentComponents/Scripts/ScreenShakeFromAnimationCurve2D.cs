using System.Collections;
using UnityEngine;

public class ScreenShakeFromAnimationCurve2D : MonoBehaviour
{
	public static ScreenShakeFromAnimationCurve2D Instance { get; private set; }

	[Header("Screen Shake Animation Parameters")]
	[SerializeField]
	[Tooltip("This float defines the duration of the shake in seconds")]
	private float shakeDuration;

	[SerializeField]
	[Tooltip("This animation curve represents the strength of the curve during the shake animation")]
	private AnimationCurve shakeStrengthCurve;

	[SerializeField]
	[Tooltip("Power with which the screenshake will shake")]
	private float shakePower;

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
		Vector3 startPos = transform.position;
		float elapsed = 0f;
		while (elapsed < shakeDuration) 
		{
			elapsed += Time.deltaTime;
			float strength = shakeStrengthCurve.Evaluate(elapsed / shakeDuration);
			transform.position = startPos + Random.insideUnitSphere * strength * shakePower;
			yield return null;
		}
		shaking = false;
		transform.position = startPos;
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

		Vector3 startPos = transform.position;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float strength = curve.Evaluate(elapsed / duration);
			transform.position = startPos + UnityEngine.Random.insideUnitSphere * strength * power;
			yield return null;
		}

		shaking = false;
		transform.position = startPos;
	}


	[ContextMenu("Debug Shake Camera")]
	private void DebugShakeCamera() { StartCoroutine(IShakeCameraFromCurve()); }


}
