using System;
using Code.Frameworks.DriverSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class CopyTransformDriver : BaseDriver
	{
		[Header("Copy Transform")]
		[SerializeField]
		public bool ObjectLocalSpace;
		
		[SerializeField]
		public bool TargetLocalSpace;

		[Space(5)]
		[SerializeField]
		public bool Unshear;

		[Space(5)]
		[SerializeField]
		public SAxis XAxis;
		
		[SerializeField]
		public SAxis YAxis;
		
		[SerializeField]
		public SAxis ZAxis;

		private Vector3 originalPosition;
		private Vector3 originalRotation;
		private Vector3 originalScale;
		
		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalPosition = Object.localPosition;
			originalRotation = Object.localEulerAngles;
			originalScale = Object.localScale;
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localScale = originalScale;
			Object.localEulerAngles = originalRotation;
			Object.localPosition = originalPosition;
			return true;
		}
		
		public override void Run()
		{
			var targetMatrix = TargetLocalSpace ? Matrix4x4.TRS(GetTargetLocalPosition(), Target.localRotation, Target.localScale) : Target.localToWorldMatrix;

			if (Unshear)
				targetMatrix = Tools.Math.Orthogonalize(targetMatrix, 1);

			if (TargetLocalSpace != ObjectLocalSpace)
			{
				if (TargetLocalSpace && !ObjectLocalSpace)
				{
					targetMatrix = Target.localToWorldMatrix;
					
					if (Unshear)
						targetMatrix = Tools.Math.Orthogonalize(targetMatrix, 1);
				}
				else
				{
					if (Object.parent != null)
						targetMatrix = Object.parent.worldToLocalMatrix * targetMatrix;
				}
			}
			
			var targetPosition = targetMatrix.GetPosition();
			var targetRotation = targetMatrix.rotation.eulerAngles;
			var targetScale = targetMatrix.lossyScale;

			if (ObjectLocalSpace)
			{
				var objectPosition = Object.localPosition;
				var objectRotation = Object.localEulerAngles;
				var objectScale = Object.localScale;
				
				if (XAxis.Enabled)
				{
					if (XAxis.Inverted)
					{
						objectPosition.x = -targetPosition.x;
						objectRotation.x = -targetRotation.x;
						objectScale.x = Tools.Math.InvertScale(targetScale.x);
					}
					else
					{
						objectPosition.x = targetPosition.x;
						objectRotation.x = targetRotation.x;
						objectScale.x = targetScale.x;
					}
				}
				
				if (YAxis.Enabled)
				{
					if (YAxis.Inverted)
					{
						objectPosition.y = -targetPosition.y;
						objectRotation.y = -targetRotation.y;
						objectScale.y = Tools.Math.InvertScale(targetScale.y);
					}
					else
					{
						objectPosition.y = targetPosition.y;
						objectRotation.y = targetRotation.y;
						objectScale.y = targetScale.y;
					}
				}
				
				if (ZAxis.Enabled)
				{
					if (ZAxis.Inverted)
					{
						objectPosition.z = -targetPosition.z;
						objectRotation.z = -targetRotation.z;
						objectScale.z = Tools.Math.InvertScale(targetScale.z);
					}
					else
					{
						objectPosition.z = targetPosition.z;
						objectRotation.z = targetRotation.z;
						objectScale.z = targetScale.z;
					}
				}
				
				Object.localPosition = objectPosition;
				Object.localEulerAngles = objectRotation;
				Object.localScale = objectScale;
			}
			else
			{
				var objectPosition = Object.position;
				var objectRotation = Object.eulerAngles;
				
				if (XAxis.Enabled)
				{
					if (XAxis.Inverted)
					{
						objectPosition.x = -targetPosition.x;
						objectRotation.x = -targetRotation.x;
					}
					else
					{
						objectPosition.x = targetPosition.x;
						objectRotation.x = targetRotation.x;
					}
				}
				
				if (YAxis.Enabled)
				{
					if (YAxis.Inverted)
					{
						objectPosition.y = -targetPosition.y;
						objectRotation.y = -targetRotation.y;
					}
					else
					{
						objectPosition.y = targetPosition.y;
						objectRotation.y = targetRotation.y;
					}
				}
				
				if (ZAxis.Enabled)
				{
					if (ZAxis.Inverted)
					{
						objectPosition.z = -targetPosition.z;
						objectRotation.z = -targetRotation.z;
					}
					else
					{
						objectPosition.z = targetPosition.z;
						objectRotation.z = targetRotation.z;
					}
				}
				
				Object.position = objectPosition;
				Object.eulerAngles = objectRotation;
				Debug.LogWarning("[CopyTransformDriver] Applying worldspace scale is not supported");
			}
		}
	}
}