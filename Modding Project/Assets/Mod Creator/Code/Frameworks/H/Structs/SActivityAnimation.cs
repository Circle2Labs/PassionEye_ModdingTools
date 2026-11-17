using System;
using System.Collections.Generic;
using Animancer;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Structs;
using Code.Frameworks.H.Enums;
using UnityEngine;

namespace Code.Frameworks.H.Structs
{
	public struct SActivityAnimation
	{
		public Guid Id;
		public string Name;
		public string Description;
		public Sprite Icon;
		public EHAnimationType Type;
		public string[] ExclusiveTags;
		public string ObjectAnimationTag;
		public bool IsAffectingActiveArousal;
		public bool IsAffectingPassiveArousal;
		public float ArousalModifier;
		public Vector3 CameraPositionOffset;
		public Vector3 CameraAnglesOffset;
		public float CameraDistance;
		public EClimaxType[] ClimaxTypes;

		public bool[] RaycastDown;
		public bool[] LookAtPartner;

		public STransitionDefinition[] IdleDefinitions;
		public STransitionDefinition[] ActivityDefinitions;

		public Dictionary<EClimaxType, STransitionDefinition[]> ClimaxIdleDefinitions;          //index (value) = participant index
		public Dictionary<EClimaxType, STransitionDefinition[]> ClimaxDefinitions;          //index (value) = participant index
		public Dictionary<EClimaxType, STransitionDefinition[]> ClimaxTransitionDefinitions;          //index (value) = participant index

		public STransitionDefinition[] NonClimaxIdleDefinitions;
		public STransitionDefinition[] NonClimaxDefinitions;
		public STransitionDefinition[] NonClimaxTransitionDefinitions;
	}
}