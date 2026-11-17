using System;
using Code.Frameworks.DriverSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class CopyRotationDriver : BaseDriver
	{
		[Header("Copy Rotation")]
		[SerializeField]
		public bool ObjectLocalSpace;
		
		[SerializeField]
		public bool TargetLocalSpace;

		[Space(5)]
		[SerializeField]
		public SAxis XAxis;
		
		[SerializeField]
		public SAxis YAxis;
		
		[SerializeField]
		public SAxis ZAxis;

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
			var targetMatrix = TargetLocalSpace ? Matrix4x4.TRS(GetTargetLocalPosition(), Target.localRotation, Target.localScale) : Target.localToWorldMatrix;
			targetMatrix = Tools.Math.Orthogonalize(targetMatrix, 1);

			if (TargetLocalSpace != ObjectLocalSpace)
			{
				if (TargetLocalSpace && !ObjectLocalSpace)
				{
					targetMatrix = Target.localToWorldMatrix;
					targetMatrix = Tools.Math.Orthogonalize(targetMatrix, 1);
				}
				else
				{
					var objectParent = Object.parent;
					if (objectParent != null)
						targetMatrix = objectParent.worldToLocalMatrix * targetMatrix;
				}
			}

			var target = targetMatrix.rotation.eulerAngles;
			
			if (ObjectLocalSpace)
			{
				var objectValue = Object.localEulerAngles;
				
				if (XAxis.Enabled)
					objectValue.x = XAxis.Inverted ? -target.x : target.x;
				
				if (YAxis.Enabled)
					objectValue.y = YAxis.Inverted ? -target.y : target.y;
				
				if (ZAxis.Enabled)
					objectValue.z = ZAxis.Inverted ? -target.z : target.z;
				
				Object.localEulerAngles = objectValue;
			}
			else
			{
				var objectValue = Object.eulerAngles;
				
				if (XAxis.Enabled)
					objectValue.x = XAxis.Inverted ? -target.x : target.x;
				
				if (YAxis.Enabled)
					objectValue.y = YAxis.Inverted ? -target.y : target.y;
				
				if (ZAxis.Enabled)
					objectValue.z = ZAxis.Inverted ? -target.z : target.z;
				
				Object.eulerAngles = objectValue;
			}
		}
	}
}