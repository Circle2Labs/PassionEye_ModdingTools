using System;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Structs
{
	[Serializable]
	public struct SAxis
	{
		[SerializeField]
		public bool Enabled;
		
		[SerializeField]
		public bool Inverted;
	}
}