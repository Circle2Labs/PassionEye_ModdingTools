using System;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class StretchToDriver : BaseDriver
	{
		[Header("Stretch To")]
		[SerializeField]
		public float OriginalLength = 1;

		[Space(5)]
		[SerializeField]
		public bool MaintainX;
		
		[SerializeField]
		public bool MaintainZ;

		private Vector3 originalRotation;
		private Vector3 originalScale;
		
		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalRotation = Object.localEulerAngles;
			originalScale = Object.localScale;
			
			if (OriginalLength < 0.0001f)
				OriginalLength = 0.0001f;
			
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localScale = originalScale;
			Object.localEulerAngles = originalRotation;
			return true;
		}
		
		public override void Run()
		{
			var direction = GetTargetPosition() - Object.position;
			if (direction.sqrMagnitude < 0.001f)
				return;
			
			Object.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
			
			var stretch = direction.magnitude / OriginalLength;
			var thickness = 1f / stretch;

			var newScale = originalScale;
			newScale.y = originalScale.y * stretch;
			
			if (MaintainX)
				newScale.x = originalScale.x * thickness;

			if (MaintainZ)
				newScale.z = originalScale.z * thickness;

			Object.localScale = newScale;
		}
	}
}