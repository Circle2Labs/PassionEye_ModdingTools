using System;
using UnityEngine;

namespace Code.Frameworks.Animation.Structs
{
	[Serializable]
	public struct SClimaxAnimation
	{
		/// <summary>
		/// Transition which is optionally null
		/// </summary>
		[SerializeField]
		public SClipDefinition TransitionDefinition;

		/// <summary>
		/// Climax action
		/// If non-climax it is optional, otherwise required
		/// </summary>
		[SerializeField]
		public SClipDefinition ClimaxDefinition;

		/// <summary>
		/// Idle which is optionally null
		/// </summary>
		[SerializeField]
		public SClipDefinition IdleDefinition;
	}
}