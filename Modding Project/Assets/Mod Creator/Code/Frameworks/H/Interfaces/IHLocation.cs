using Code.Frameworks.H.Enums;
using Code.Frameworks.H.Structs;
using Code.Tools;
using UnityEngine;

namespace Code.Frameworks.H.Interfaces
{
	public interface IHLocation
	{
		public string Name { get; set; }
		public ELocationType Type { get; set; }
		// Only HAnimation with these tags will show up
		public string[] ExclusiveTags { get; set; }
		// If a tag exists as item1, it will run item3 animation on item2 animator
		public Tuple.SerializableTuple<string, Animator, AnimationClip>[] ObjectAnimations { get; set; }
	}
}