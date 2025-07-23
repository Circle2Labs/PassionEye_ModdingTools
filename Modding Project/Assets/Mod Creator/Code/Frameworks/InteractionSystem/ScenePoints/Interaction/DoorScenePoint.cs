using Code.Components;
using Code.Frameworks.InteractionSystem.Database.Enums;
using Code.Frameworks.InteractionSystem.Database.Interfaces;
using Code.Frameworks.InteractionSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.ScenePoints.Interaction
{
	public class DoorScenePoint : BaseInteractionScenePoint
	{
		public override EInteractionIdentifier InteractionIdentifier => EInteractionIdentifier.Door;

		public override object[] Parameters => new object[] { Door };

		[SerializeField]
		public DoorController Door;
	}
}