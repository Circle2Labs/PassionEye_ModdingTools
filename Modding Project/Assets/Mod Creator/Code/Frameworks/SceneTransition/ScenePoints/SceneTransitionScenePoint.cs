using System.Collections.Generic;
using Code.Frameworks.InteractionSystem;
using Code.Frameworks.InteractionSystem.Context;
using Code.Frameworks.InteractionSystem.Context.Actions;
using Code.Frameworks.InteractionSystem.Database.Enums;
using Code.Frameworks.InteractionSystem.Database.Interfaces;
using Code.Frameworks.InteractionSystem.ScenePoints;
using Code.Frameworks.InteractionSystem.Structs;
using Code.Frameworks.SceneTransition.Structs;
using UnityEngine;

namespace Code.Frameworks.SceneTransition.ScenePoints
{
	public class SceneTransitionScenePoint : BaseInteractionScenePoint
	{
		public override EInteractionIdentifier InteractionIdentifier => EInteractionIdentifier.SceneTransition; 

		public override object[] Parameters => new object[] { TransitionRoute, TransitionObjects, PlayerPosition, Quaternion.Euler(PlayerRotation) };

		[SerializeField]
		public STransitionRoute TransitionRoute;
		
		[SerializeField]
		public List<STransitionObject> TransitionObjects;
		
		[SerializeField]
		public Vector3 PlayerPosition;
		
		[SerializeField]
		public Vector3 PlayerRotation;

		public override ActionCategory GetActionCategory(ActionCategory defaultActionCategory, IScriptableInteraction scriptableInteraction, SRunParameters runparameters)
		{
			return null;
		}
	}
}