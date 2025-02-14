using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Structs;
using UnityEngine;

namespace Code.Frameworks.Animation.Interfaces
{
    public interface IAnimation
	{
		public Sprite Icon { get; set; }

		public string Name { get; set; }

		public string[] Tags { get; set; }

		public string Description { get; set; }
		
		public bool IsNSFW { get; set; }
		
		public EAnimationUsageFlags UsageFlags { get; }

		#region CrossFade
		
		public float FadeDuration { get; }
		
		#endregion

		public SClipContainer[] ClipContainers { get; }
	}
}