using System;
using Code.Frameworks.ModdedScenes.Flags;
using Railgun.ModEngine.Types;
using UnityEngine;

namespace Code.Frameworks.ModdedScenes
{
	[Serializable]
	public class ModdedScene : MonoBehaviour, IHasMetadata
	{
		public GameObjectMetadata Metadata { get; set; }
		
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
		public EModdedSceneUsageFlags UsageFlags { get; set; }
		
		[field: SerializeField]
		public Sprite LargeBackground { get; set; }
	}
}  