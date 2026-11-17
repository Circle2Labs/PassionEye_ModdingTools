using System;
using Animancer;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
	[Serializable]
	public struct STransitionDefinition
	{
		[SerializeField]
		public ITransition Transition;

		[SerializeField]
		public SCockTargetDefinition[] CockTargetDefinitions;

		[SerializeField]
		public SStateIKDefinitionContainer[] IKContainers;
	}
}