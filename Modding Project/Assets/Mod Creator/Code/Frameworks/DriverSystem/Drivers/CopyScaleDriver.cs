using System;
using Code.Frameworks.DriverSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class CopyScaleDriver : BaseDriver
	{
		[Header("Copy Scale")]
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

		private Vector3 originalScale;
		
		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalScale = Object.localScale;
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localScale = originalScale;
			return true;
		}
		
		public override void Run()
		{
			Vector3 target;

			if (TargetLocalSpace == ObjectLocalSpace)
			{
				target = TargetLocalSpace ? Target.localScale : Target.lossyScale;
			}
			else if (TargetLocalSpace && !ObjectLocalSpace)
			{
				target = Target.lossyScale;
			}
			else
			{
				var objectParent = Object.parent;
				if (objectParent != null)
				{
					var parentScale = objectParent.lossyScale;
					var worldScale = Target.lossyScale;

					target = new Vector3(Tools.Math.SafeDivide(worldScale.x, parentScale.x), Tools.Math.SafeDivide(worldScale.y, parentScale.y), Tools.Math.SafeDivide(worldScale.z, parentScale.z));
				}
				else
				{
					target = Target.lossyScale;
				}
			}
			
			if (ObjectLocalSpace)
			{
				var objectValue = Object.localScale;
				
				if (XAxis.Enabled)
					objectValue.x = XAxis.Inverted ? Tools.Math.InvertScale(target.x) : target.x;
				
				if (YAxis.Enabled)
					objectValue.y = YAxis.Inverted ? Tools.Math.InvertScale(target.y) : target.y;
				
				if (ZAxis.Enabled)
					objectValue.z = ZAxis.Inverted ? Tools.Math.InvertScale(target.z) : target.z;
				
				Object.localScale = objectValue;
			}
			else
			{
				Debug.LogWarning("[CopyScaleDriver] Applying worldspace scale is not supported");
			}
		}
	}
}