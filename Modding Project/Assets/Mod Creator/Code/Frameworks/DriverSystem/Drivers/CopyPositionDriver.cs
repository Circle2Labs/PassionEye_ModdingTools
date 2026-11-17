using System;
using Code.Frameworks.DriverSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class CopyPositionDriver : BaseDriver
	{
		[Header("Copy Position")]
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

		private Vector3 originalPosition;
		
		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalPosition = Object.localPosition;
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localPosition = originalPosition;
			return true;
		}

		public override void Run()
		{
			Vector3 target;

			if (TargetLocalSpace == ObjectLocalSpace)
			{
				target = TargetLocalSpace ? GetTargetLocalPosition() : GetTargetPosition();
			}
			else if (TargetLocalSpace && !ObjectLocalSpace)
			{
				target = GetTargetPosition();
			}
			else
			{
				var objectParent = Object.parent;
				target = objectParent != null ? objectParent.InverseTransformPoint(GetTargetPosition()) : GetTargetPosition();
			}
			
			if (ObjectLocalSpace)
			{
				var objectValue = Object.localPosition;
				
				if (XAxis.Enabled)
					objectValue.x = XAxis.Inverted ? -target.x : target.x;
				
				if (YAxis.Enabled)
					objectValue.y = YAxis.Inverted ? -target.y : target.y;
				
				if (ZAxis.Enabled)
					objectValue.z = ZAxis.Inverted ? -target.z : target.z;
				
				Object.localPosition = objectValue;
			}
			else
			{
				var objectValue = Object.position;
				
				if (XAxis.Enabled)
					objectValue.x = XAxis.Inverted ? -target.x : target.x;
				
				if (YAxis.Enabled)
					objectValue.y = YAxis.Inverted ? -target.y : target.y;
				
				if (ZAxis.Enabled)
					objectValue.z = ZAxis.Inverted ? -target.z : target.z;
				
				Object.position = objectValue;
			}
		}
	}
}