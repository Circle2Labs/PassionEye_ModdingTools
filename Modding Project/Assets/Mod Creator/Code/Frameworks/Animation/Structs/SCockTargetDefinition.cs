using System;
using Code.Frameworks.Animation.Enums;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
	[Serializable]
	public struct SCockTargetDefinition
	{
		[SerializeField]
		public ECockTarget CockTarget;
		
		[SerializeField]
		public AnimationCurve Curve;
	}
}