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
		public AnimationClip Transition;

		/// <summary>
		/// Climax action
		/// If non-climax it is optional, otherwise required
		/// </summary>
		[SerializeField]
		public AnimationClip Climax;

		/// <summary>
		/// Idle which is optionally null
		/// </summary>
		[SerializeField]
		public AnimationClip Idle;
	}
}