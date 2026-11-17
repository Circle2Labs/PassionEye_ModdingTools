using System;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
	[Serializable]
	public struct SStateIKDefinitionContainer
	{
		[SerializeField]
		public SStateIKDefinition[] Definitions;
	}
}