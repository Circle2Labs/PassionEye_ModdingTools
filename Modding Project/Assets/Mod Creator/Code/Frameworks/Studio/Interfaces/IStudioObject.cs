using Code.Frameworks.Studio.Enums;
using Code.Interfaces;
using UnityEngine;

namespace Code.Frameworks.Studio.Interfaces
{
	public interface IStudioObject : IMaterialCustomizable
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