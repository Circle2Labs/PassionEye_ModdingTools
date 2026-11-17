using System;
using Code.Tools;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
	[Serializable]
	public struct SClipDefinition
	{
		[SerializeField]
		public AnimationClip AnimationClip;

		[SerializeField]
		public SCockTargetDefinition CockTargetDefinition;
		
		[SerializeField]
		public SStateIKDefinition[] IKDefinitions;

		public STransitionDefinition ToTransitionDefinition(float fadeDuration)
		{
			return new STransitionDefinition
			{ 
				Transition = AnimationClip.ToClipTransition(fadeDuration),
				CockTargetDefinitions = new [] {CockTargetDefinition},
				IKContainers = new [] {new SStateIKDefinitionContainer {Definitions = IKDefinitions}}
			};
		}
	}
}