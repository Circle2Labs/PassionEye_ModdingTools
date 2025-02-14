using System;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Interfaces;
using Code.Frameworks.Animation.Structs;
using UnityEngine;

namespace Code.Frameworks.Animation
{
    [Serializable]
	public class BaseAnimation : MonoBehaviour, IAnimation
	{
		protected const bool __metadata = true;

		[field: SerializeField]
		public Sprite Icon { get; set; }

		[field: SerializeField]
		public string Name { get; set; }
		
		[field: SerializeField]
		public string[] Tags { get; set; }

		[field: SerializeField]
		public string Description { get; set; }
		
		[field: SerializeField]
		public bool IsNSFW { get; set; }

		[field: SerializeField]
		public EAnimationUsageFlags UsageFlags { get; set; }

		#region CrossFade
		
		[field: SerializeField]
		public float FadeDuration { get; set; }

		#endregion
		
		[field: SerializeField]
		public SClipContainer[] ClipContainers { get; set; }
	}
}