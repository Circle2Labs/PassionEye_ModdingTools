using Code.Frameworks.InteractionSystem.Context;
using Code.Frameworks.InteractionSystem.Database.Enums;
using Code.Frameworks.InteractionSystem.Database.Interfaces;
using Code.Frameworks.InteractionSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.ScenePoints.Interfaces
{
	public interface IInteractionScenePoint
	{
		/// <summary>
		/// Identifier of the interaction belonging to this scenepoint
		/// If the interaction does not allow execution when busy and is busy at the time, this is not shown in the action wheel
		/// </summary>
		public EInteractionIdentifier InteractionIdentifier { get; set; }

		public int InstanceId { get; }

		/// <summary>
		/// Parameters sent to the Setup method of the interaction
		/// </summary>
		public object[] Parameters { get; set; }

		/// <summary>
		/// Allow providing a custom name for the interaction
		/// </summary>
		public string OverrideName { get; set; }
		/// <summary>
		/// Allow providing a custom description for the interaction
		/// </summary>
		public string OverrideDescription { get; set; }

		/// <summary>
		/// Anchor to help position the primary indicator UI
		/// </summary>
		public Transform PrimaryIndicatorAnchor { get; set; }
		
		/// <summary>
		/// Anchor to help position the interaction user
		/// </summary>
		public Transform PositioningAnchor { get; set; }
		
		/// <summary>
		/// Override the default action category with a custom one
		/// </summary>
		public ActionCategory GetActionCategory(ActionCategory defaultActionCategory, IScriptableInteraction scriptableInteraction, SRunParameters runparameters);
	}
}