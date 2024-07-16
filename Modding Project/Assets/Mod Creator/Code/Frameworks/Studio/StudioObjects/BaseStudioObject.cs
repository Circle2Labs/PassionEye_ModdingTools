using System;
using System.Collections.Generic;
using Code.Frameworks.ForwardKinematics;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.Studio.Enums;
using Code.Frameworks.Studio.Interfaces;
using Code.Tools;
using UnityEngine;

namespace Code.Frameworks.Studio.StudioObjects
{
	[Serializable]
	public class BaseStudioObject : MonoBehaviour, IStudioObject
	{
		protected const bool __metadata = true;
		
		[field: SerializeField]
		public EStudioObjectType StudioObjectType { get; set; }
		
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
		public SFKData FKData { get; set; }
		
		[field: SerializeField]
		public Renderer[] Renderers { get; set; }

		public GameObject GetGameObject()
		{
			return gameObject;
		}

		public List<LODGroup> GetLODGroups()
		{
			return null;
		}
		
		public int GetLOD(SkinnedMeshRenderer rend)
		{
			return 0;
		}
	}
}