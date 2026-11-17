using System;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Character.Enums;
using RootMotion.FinalIK;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
	[Serializable]
	public struct SStateIKDefinition
	{
		[SerializeField]
		public FullBodyBipedEffector Effector;
		
		[SerializeField]
		public FullBodyBipedChain Chain;

		[SerializeField]
		public bool IsChain;

		[SerializeField]
		public EIKTargetObject TargetObject;

		[SerializeField]
		public EIKTargetType TargetType;

		[SerializeField]
		public HumanBodyBones TargetBone;
		
		[SerializeField]
		public EAdditionalHumanBodyBones AdditionalTargetBone;
		
		[SerializeField]
		public AnimationCurve PositionCurve;
		
		[SerializeField]
		public AnimationCurve RotationCurve;
		
		public SStateIKDefinition Copy()
		{
			var definition = new SStateIKDefinition();
			definition.Effector = Effector;
			definition.Chain = Chain;
			definition.IsChain = IsChain;
			definition.TargetObject = TargetObject;
			definition.TargetType = TargetType;
			definition.TargetBone = TargetBone;
			definition.AdditionalTargetBone = AdditionalTargetBone;
			definition.PositionCurve = new AnimationCurve();
			definition.PositionCurve.CopyFrom(PositionCurve);
			definition.RotationCurve = new AnimationCurve();
			definition.RotationCurve.CopyFrom(RotationCurve);
			return definition;
		}

		public static SStateIKDefinition[] CopyArray(SStateIKDefinition[] array)
		{
			var copy = new SStateIKDefinition[array.Length];
			
			for (var i = 0; i < array.Length; i++)
				copy[i] = array[i].Copy();

			return copy;
		}
	}
}