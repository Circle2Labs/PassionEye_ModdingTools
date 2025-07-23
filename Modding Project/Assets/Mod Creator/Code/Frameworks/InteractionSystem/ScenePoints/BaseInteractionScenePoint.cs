using Code.Frameworks.InteractionSystem.Context;
using Code.Frameworks.InteractionSystem.Database.Enums;
using Code.Frameworks.InteractionSystem.Database.Interfaces;
using Code.Frameworks.InteractionSystem.ScenePoints.Enums;
using Code.Frameworks.InteractionSystem.ScenePoints.Interfaces;
using Code.Frameworks.InteractionSystem.Structs;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.ScenePoints
{
    public class BaseInteractionScenePoint : MonoBehaviour, IInteractionScenePoint
	{
		[field: SerializeField]
		public EInstantiationFlags InstantiationFlags { get; set; }

		[field: SerializeField]
		public virtual EInteractionIdentifier InteractionIdentifier { get; set; }

		public int InstanceId => GetInstanceID();

        public virtual object[] Parameters { get; set; }
		
		[field: SerializeField]
		public string OverrideName { get; set; }
		[field: SerializeField]
		public string OverrideDescription { get; set; }

		public virtual ActionCategory GetActionCategory(ActionCategory defaultActionCategory, IScriptableInteraction scriptableInteraction, SRunParameters runparameters)
		{
			return defaultActionCategory;
		}
	}
}