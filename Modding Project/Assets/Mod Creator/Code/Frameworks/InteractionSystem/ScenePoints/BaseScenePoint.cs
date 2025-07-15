using Code.Frameworks.InteractionSystem.ScenePoints.Enums;
using Code.Frameworks.InteractionSystem.ScenePoints.Interfaces;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.ScenePoints
{
	public class BaseScenePoint : MonoBehaviour, IScenePoint
	{
		[field: SerializeField]
		public virtual EUsageOptionFlags UsageOptionFlags { get; set; }
	}
}