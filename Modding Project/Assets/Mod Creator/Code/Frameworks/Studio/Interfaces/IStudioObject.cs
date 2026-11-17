using Code.Frameworks.ForwardKinematics.Interfaces;
using Code.Frameworks.Shared.Interfaces;
using Code.Frameworks.Shared.Structs;
using Code.Frameworks.Studio.Enums;
using Code.Interfaces;
using Railgun.ModEngine.Types;
using UnityEngine;

namespace Code.Frameworks.Studio.Interfaces
{
	public interface IStudioObject : IMaterialCustomizable, IFKCompatible, IHasMetadata, IHasVariants
	{
		public EStudioObjectType StudioObjectType { get; set; }
		
		public Sprite Icon { get; set; }

		public string Name { get; set; }

		public string[] Tags { get; set; }

		public string Description { get; set; }
		
		public bool IsNSFW { get; set; }
		
		public GameObject GetGameObject();
	}
}