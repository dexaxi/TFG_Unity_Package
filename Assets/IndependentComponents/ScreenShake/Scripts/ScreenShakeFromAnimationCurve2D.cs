using System.Collections;
using UnityEngine;


/// <summary>
//	This class uses 3D translation to shake the screen, as using rotation in 2D environment leads to janky movement. 
/// </summary>
public class ScreenShakeFromAnimationCurve2D : MonoBehaviour
{
	public static ScreenShakeFromAnimationCurve2D Instance { get; private set; }

	[Header("Screen Shake Animation Parameters")]
	[SerializeField]
	[Tooltip("This animation curve type the shape of the curve during the shake animation")]
	private AnimationCurveType shakeCurveType;

	[SerializeField]
	[Tooltip("This float defines the duration of the shake in seconds")]
	[Range(0f,2f)]
	private float shakeDuration;

	[SerializeField]
	[Range(0f,3f)]
	[Tooltip("Power with which the screenshake will shake")]
	private float shakePower;

	private bool shaking;

    private void Awake()
    {
		if (Instance == null)
			Instance = this;
    }

	/// <summary>
	//	Void function to Shake the Screen using the parameters from the inspector
	/// </summary>
	public void ShakeScreen() {
		if (!shaking)
		{
			shaking = true;
			StartCoroutine(IShakeScreen());
		}
	}

	/// <summary>
	//Coroutine to Shake Screen
	/// </summary>
	private IEnumerator IShakeScreen()
	{
		Vector3 startPos = transform.position;
		float elapsed = 0f;
		while (elapsed < shakeDuration) 
		{
			elapsed += Time.deltaTime;
			float strength = AnimationCurveSelector.Instance.GetCurve(shakeCurveType).Evaluate(elapsed / shakeDuration);
			transform.position = startPos + Random.insideUnitSphere * strength * shakePower;
			yield return null;
		}
		shaking = false;
		transform.position = startPos;
	}

	/// <summary>
	//Void function to Shake the Screen using parameters defined from code
	/// </summary>
	public void ShakeScreen(AnimationCurve curve, float duration, float power)
	{
		if (!shaking)
		{
			shaking = true;
			StartCoroutine(IShakeScreen(curve, duration, power));
		}
	}

	/// <summary>
	//Coroutine to Shake Screen
	/// </summary>
	private IEnumerator IShakeScreen(AnimationCurve curve, float duration, float power)
	{
		if (curve == null) curve = AnimationCurveSelector.Instance.GetCurve(shakeCurveType);

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
	private void DebugShakeCamera() { StartCoroutine(IShakeScreen()); }


}
