using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Structs;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Enums;
using UnityEngine;
using Tuple = Code.Tools.Tuple;

namespace Code.Frameworks.Animation
{
	public class HAnimation : BaseAnimation
	{
		/// <summary>
		/// Type of this H animation
		/// Affects what part of the UI it shows up on and the arousal change rate
		/// </summary>
		[field: SerializeField]
		public EHAnimationType Type { get; set; }
		
		/// <summary>
		/// Climax types supported by this H animation
		/// </summary>
		[field: SerializeField]
		public ESupportedClimaxTypesFlags SupportedClimaxTypes { get; set; }
		
		/// <summary>
		/// Should this animation raise arousal of the active character (eg. male)
		/// </summary>
		[field: SerializeField]
		public bool ArouseActive { get; set; }
		
		/// <summary>
		/// Should this animation raise arousal of the passive character (eg. female)
		/// </summary>
		[field: SerializeField]
		public bool ArousePassive { get; set; }
		
		/// <summary>
		/// Multiply arousal increase by this value
		/// Some poses might want to increase arousal more than other even if they're of the same type
		/// </summary>
		[field: SerializeField]
		public float ArousalMultiplier { get; set; }
		
		/// <summary>
		/// Offset the default vertical position of the camera
		/// Standing poses and lying down poses might have different defaults
		/// </summary>
		[field: SerializeField]
		public float VerticalCameraOffset { get; set; }
		
		/// <summary>
		/// Idle animations per-container
		/// eg:
		/// [0] = container 0 - male
		/// [1] = container 1 - female
		/// </summary>
		[field: SerializeField]
		public AnimationClip[] IdleClips { get; set; }
		
		/// <summary>
		/// Non-Climax animations per-container
		/// eg:
		/// [0] = container 0 - male
		/// [1] = container 1 - female
		/// </summary>
		[field: SerializeField]
		public SClimaxAnimation[] NonClimaxClips { get; set; }

		/// <summary>
		/// Climax animations per-type and per-container
		/// eg:
		/// [0] = outside + (container 0 - male, container 1 - female)
		/// [1] = inside + (container 0 - male, container 1 - female)
		/// </summary>
		[field: SerializeField]
		public Tuple.SerializableTuple<EClimaxType, SClimaxAnimation[]>[] ClimaxClips { get; set; }

		/// <summary>
		/// Returns climax animations for specified climax type
		/// Array indexing matches clip container setup
		/// eg:
		/// if first container is male, the first index returned will be the climax animation for male
		/// </summary>
		public SClimaxAnimation[] GetClimaxClips(EClimaxType climaxType)
		{
			if (ClimaxClips == null)
				return null;
			
			for (var i = 0; i < ClimaxClips.Length; i++)
			{
				var tuple = ClimaxClips[i];
				if (tuple.Item1 != climaxType)
					continue;
				
				return tuple.Item2;
			}

			return null;
		}
	}
}