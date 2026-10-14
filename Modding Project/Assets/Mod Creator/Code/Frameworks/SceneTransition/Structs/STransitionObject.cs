using System;
using UnityEngine;

namespace Code.Frameworks.SceneTransition.Structs
{
	[Serializable]
	public struct STransitionObject
	{
		[SerializeField]
		public GameObject Object;

		[SerializeField]
		public Vector3 Position;
		
		[SerializeField]
		public Vector3 Angles;

		[SerializeField]
		public Vector3 LinearVelocity;
		
		[SerializeField]
		public Vector3 AngularVelocity;
	}
}