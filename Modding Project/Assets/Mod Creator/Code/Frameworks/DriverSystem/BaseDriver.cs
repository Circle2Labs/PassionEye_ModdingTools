using System;
using Code.Attributes;
using UnityEngine;

namespace Code.Frameworks.DriverSystem
{
	[Serializable]
	public class BaseDriver
	{
		public virtual bool UseObject => true;
		public virtual bool UseTarget => true;

		[ReadOnly]
		public bool Enabled;
		
		[Header("Base")]
		[SerializeField]
		public Transform Object;

		[SerializeField]
		public Transform Target;

		[SerializeField]
		public float TargetYOffset;

		public virtual bool OnEnable()
		{
			if (Enabled)
				return false;

			if (UseObject && Object == null)
			{
				Debug.LogWarning($"[{GetType().Name}] Required object is not set");
				return false;
			}

			if (UseTarget && Target == null)
			{
				Debug.LogWarning($"[{GetType().Name}] Required target is not set");
				return false;
			}
			
			Enabled = true;
			return true;
		}

		public virtual void Run()
		{

		}

		public virtual bool OnDisable()
		{
			if (!Enabled)
				return false;
			
			Enabled = false;
			return true;
		}
		
		public Vector3 GetTargetLocalPosition()
		{
			var localPosition = Target.localPosition;
			localPosition.y += TargetYOffset;
			
			return localPosition;
		}
		
		public Vector3 GetTargetPosition()
		{
			return Target.position + Target.up * TargetYOffset;
		}
	}
}