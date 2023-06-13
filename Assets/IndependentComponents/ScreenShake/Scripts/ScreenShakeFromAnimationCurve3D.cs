namespace DUJAL.IndependentComponents.ScreenShake
{
	using System.Collections;
	using UnityEngine;
	using Cinemachine;
	/// <summary>
	//	This class uses 3D rotation to shake the screen to prevent clipping and perspective issues. 
	/// </summary>
	public class ScreenShakeFromAnimationCurve3D : MonoBehaviour
	{
		private const float _shakeSpeed = 10f;
		private const float _angleThreshold = 30f;

		public static ScreenShakeFromAnimationCurve3D Instance { get; private set; }

		[Header("Screen Shake Animation Parameters")]
		[SerializeField]
		[Tooltip("This animation curve type the shape of the curve during the shake animation")]
		private AnimationCurveType shakeCurveType;

		[SerializeField]
		[Tooltip("This float defines the duration of the shake in seconds")]
		[Range(0f, 2f)]
		private float shakeDuration;

		[SerializeField]
		[Range(0f, 15f)]
		[Tooltip("Power with which the rotation based screenshake will shake")]
		private float rotationPower;

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
			Quaternion startRot = transform.rotation;
			Quaternion newRot = startRot;
			float elapsed = 0f;
			while (elapsed < shakeDuration)
			{
				elapsed += Time.deltaTime;
				//Evaluate Curve to get strength multiplier from it
				float strength = AnimationCurveSelector.Instance.GetCurve(shakeCurveType).Evaluate(elapsed / shakeDuration);
				//if angle from last rot to current rot <= (strength*power)/threshold, apply new rotation, else, lerp to current destination.
				if (Quaternion.Angle(newRot, transform.rotation) <= (strength * rotationPower) / _angleThreshold)
				{
					newRot = Quaternion.Euler(startRot.eulerAngles.x + strength * rotationPower * Random.Range(-1, 1),
						startRot.eulerAngles.y + strength * rotationPower * Random.Range(-1, 1), startRot.eulerAngles.z + strength * rotationPower * Random.Range(-1, 1));
				}
				transform.rotation = Quaternion.Lerp(transform.rotation, newRot, elapsed * _shakeSpeed);
				yield return null;
			}
			Shaking = false;
			transform.rotation = startRot;
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

			Quaternion startRot = transform.rotation;
			Quaternion newRot = startRot;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float strength = curve.Evaluate(elapsed / duration);
				if (Quaternion.Angle(newRot, transform.rotation) <= (strength * power) / _angleThreshold)
				{
					newRot = Quaternion.Euler(startRot.eulerAngles.x + strength * power * Random.Range(-1, 1),
						startRot.eulerAngles.y + strength * power * Random.Range(-1, 1), startRot.eulerAngles.z + strength * power * Random.Range(-1, 1));
				}
				transform.rotation = Quaternion.Lerp(transform.rotation, newRot, elapsed * _shakeSpeed);
				yield return null;
			}
			Shaking = false;
			transform.rotation = startRot;
		}

		/// <summary>
		//Void function to Shake the Screen using Cinemachine virtual cameras using component params
		/// </summary>
		public void ShakeScreenCinemachine(CinemachineVirtualCamera camera, AnimationCurve curve)
		{
			if (camera == null) return;
			if (!Shaking)
			{
				Shaking = true;
				StartCoroutine(IShakeScreenCinemachine(camera, curve, shakeDuration, rotationPower));
			}
		}

		/// <summary>
		//Void function to Shake the Screen using Cinemachine free look cameras using component params
		/// </summary>
		public void ShakeScreenCinemachine(CinemachineFreeLook camera, AnimationCurve curve)
		{
			if (camera == null) return;
			if (!Shaking)
			{
				Shaking = true;
				StartCoroutine(IShakeScreenCinemachine(camera.GetRig(0), curve, shakeDuration, rotationPower));
				StartCoroutine(IShakeScreenCinemachine(camera.GetRig(1), curve, shakeDuration, rotationPower));
				StartCoroutine(IShakeScreenCinemachine(camera.GetRig(2), curve, shakeDuration, rotationPower));
			}
		}

		/// <summary>
		//Void function to Shake the Screen using Cinemachine virtual cameras using code defined params
		/// </summary>
		public void ShakeScreenCinemachine(CinemachineVirtualCamera camera,AnimationCurve curve, float duration, float power)
		{
			if (camera == null) return;
			if (!Shaking)
			{
				Shaking = true;
				StartCoroutine(IShakeScreenCinemachine(camera, curve, duration, power));
			}
		}

		/// <summary>
		//Void function to Shake the Screen using Cinemachine free look cameras using code defined params
		/// </summary>
		public void ShakeScreenCinemachine(CinemachineFreeLook camera, AnimationCurve curve, float duration, float power)
		{
			if (camera == null) return;
			if (!Shaking)
			{
				Shaking = true;
				StartCoroutine(IShakeScreenCinemachine(camera.GetRig(0), curve, duration, power));
				StartCoroutine(IShakeScreenCinemachine(camera.GetRig(1), curve, duration, power));
				StartCoroutine(IShakeScreenCinemachine(camera.GetRig(2), curve, duration, power));
			}
		}
		
		/// <summary>
		//Coroutine to Shake Screen using a Cinemachine Virtual Camera
		/// </summary>
		private IEnumerator IShakeScreenCinemachine(CinemachineVirtualCamera camera, AnimationCurve curve, float duration, float power)
		{
			CinemachineBasicMultiChannelPerlin perlin = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			float _startingAmplitudeGain = perlin.m_AmplitudeGain;
			float _startingFrequencyGain = perlin.m_FrequencyGain;

			if (curve == null) curve = AnimationCurveSelector.Instance.GetCurve(shakeCurveType);

			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float strength = curve.Evaluate(elapsed / duration);
				perlin.m_AmplitudeGain = strength * power;
				perlin.m_FrequencyGain = strength * power;
				yield return null;
			}

			Shaking = false;
			perlin.m_AmplitudeGain = _startingAmplitudeGain;
			perlin.m_FrequencyGain = _startingFrequencyGain;
		}

		/// <summary>
		// ContextMenu debug function to force a screen shake
		/// </summary>
		[ContextMenu("Debug Shake Camera")]
		private void DebugShakeCamera() { StartCoroutine(IShakeScreen()); }
	}
}
