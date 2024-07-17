using System;
using UnityEngine;
using Tuple = Code.Tools.Tuple;

namespace Code.Frameworks.ForwardKinematics.Structs
{
	[Serializable]
	public struct SFKTransform
	{
		public string Name;
		
		public Transform Transform;

		public Quaternion RestingAngle;

		public Transform MirrorTransform;

		public Tuple.SerializableTuple<bool, bool, bool> MirrorInvertAxis;

		public float MovableTargetScale;
		
		public static SFKTransform Default()
		{
			return new SFKTransform
			{
				Name = "New Transform",
				RestingAngle = new Quaternion(),
				MirrorTransform = null,
				MirrorInvertAxis = new Tuple.SerializableTuple<bool, bool, bool>(true, false, false),
				MovableTargetScale = 1f
			};
		}
	}
}