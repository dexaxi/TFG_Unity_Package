using System.Collections;
using UnityEngine;

namespace DUJAL.IndependentComponents.ScreenShake
{
	/// <summary>
	//	This class uses 3D translation to shake the screen, as using rotation in 2D environment leads to janky movement. 
	/// </summary>
	public class ScreenShakeFromAnimationCurve2D : MonoBehaviour
	{
		private const float _shakeSpeed = 10f;
		private const float _distanceThreshold = 30f;

		public static ScreenShakeFromAnimationCurve2D Instance { get; private set; }

		[Header("Screen Shake Animation Parameters")]
		[SerializeField]
		[Tooltip("This animation curve type the shape of the curve during the shake animation")]
		private AnimationCurveType shakeCurveType;

		[SerializeField]
		[Tooltip("This float defines the duration of the shake in seconds")]
		[Range(0f, 2f)]
		private float shakeDuration;

		[SerializeField]
		[Range(0f, 3f)]
		[Tooltip("Power with which the screenshake will shake")]
		private float shakePower;

		public bool Shaking { get; private set; }

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else
				Destroy(this.gameObject);
		}

		/// <summary>
		//	Void function to Shake the Screen using the parameters from the inspector
		/// </summary>
		public void ShakeScreen()
		{
			if (!Shaking)
			{
				Shaking = true;
				StartCoroutine(IShakeScreen());
			}
		}

		/// <summary>
		//Coroutine to Shake Screen
		/// </summary>
		private IEnumerator IShakeScreen()
		{
			Vector3 startPos = transform.position;
			Vector3 newPos = startPos;
			float elapsed = 0f;
			while (elapsed < shakeDuration)
			{
				elapsed += Time.deltaTime;
				//Evaluate Curve to get strength multiplier from it
				float strength = AnimationCurveSelector.Instance.GetCurve(shakeCurveType).Evaluate(elapsed / shakeDuration);
				//if distance from last pos to current pos <= (strength*power)/ threshold, apply new position, else, lerp to current destination.
				if (Vector3.Distance(newPos, transform.position) <= (strength * shakePower) / _distanceThreshold)
				{
					newPos = startPos + Random.insideUnitSphere * strength * shakePower;
				}
				transform.position = Vector3.Lerp(transform.position, newPos, elapsed * _shakeSpeed);
				yield return null;
			}

			Shaking = false;
			transform.position = startPos;
		}

		/// <summary>
		//Void function to Shake the Screen using parameters defined from code
		/// </summary>
		public void ShakeScreen(AnimationCurve curve, float duration, float power)
		{
			if (!Shaking)
			{
				Shaking = true;
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
			Vector3 newPos = startPos;
			float elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float strength = curve.Evaluate(elapsed / duration);
				if (Vector3.Distance(newPos, transform.position) <= (strength * power) / _distanceThreshold)
				{
					newPos = startPos + Random.insideUnitSphere * strength * power;
				}
				transform.position = Vector3.Lerp(transform.position, newPos, elapsed * _shakeSpeed);
				yield return null;
			}

			Shaking = false;
			transform.position = startPos;
		}


		[ContextMenu("Debug Shake Camera")]
		private void DebugShakeCamera() { StartCoroutine(IShakeScreen()); }


	}
}
