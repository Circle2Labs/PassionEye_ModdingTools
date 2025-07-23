using Code.Frameworks.InteractionSystem.Database.Enums;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.ScenePoints.Interaction
{
	public class LightSwitchScenePoint : BaseInteractionScenePoint
	{
		public override EInteractionIdentifier InteractionIdentifier => EInteractionIdentifier.LightSwitch; 

		public override object[] Parameters => Lights;

		[SerializeField]
		public Light[] Lights;
	}
}