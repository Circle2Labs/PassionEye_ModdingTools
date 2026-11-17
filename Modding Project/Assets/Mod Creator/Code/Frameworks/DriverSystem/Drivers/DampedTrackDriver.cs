using System;
using Code.Frameworks.DriverSystem.Enums;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class DampedTrackDriver : BaseDriver
	{
		[Header("Damped Track")]
		[SerializeField]
		public EAxis Axis;
		
		private Vector3 originalRotation;
		
		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalRotation = Object.localEulerAngles;
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localEulerAngles = originalRotation;
			return true;
		}
		
		public override void Run()
		{
			var direction = GetTargetPosition() - Object.position;
			if (direction.sqrMagnitude < 0.001f)
				return;
			
			Vector3 trackAxis = default;
			
			switch (Axis)
			{
				case EAxis.PosX:
					trackAxis = Vector3.right;
					break;
				case EAxis.NegX:
					trackAxis = Vector3.left;
					break;
				case EAxis.PosY:
					trackAxis = Vector3.up;
					break;
				case EAxis.NegY:
					trackAxis = Vector3.down;
					break;
				case EAxis.PosZ:
					trackAxis = Vector3.forward;
					break;
				case EAxis.NegZ:
					trackAxis = Vector3.back;
					break;
			}
			
			var trackDirection = Object.TransformDirection(trackAxis);
			var fromToRotation = Quaternion.FromToRotation(trackDirection, direction.normalized);
			
			Object.rotation = fromToRotation * Object.rotation;
		}
	}
}