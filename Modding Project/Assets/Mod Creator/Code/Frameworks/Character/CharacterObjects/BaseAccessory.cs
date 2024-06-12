using System;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.PhysicsSimulation;
using Code.Tools;
using Railgun.AssetPipeline.Attributes;
using UnityEngine;

namespace Code.Frameworks.Character.CharacterObjects
{
	[Serializable]
	public class BaseAccessory : MonoBehaviour, IAccessory
	{
		protected const bool __metadata = true;

		[APLIgnore]
		public bool IsSingular => false;
		
		[field: SerializeField]
		public ECharacterObjectType ObjectType { get; set; }

		[field: SerializeField]
		public EAccessoryType AccessoryType { get; set; }

		[field: SerializeField]
		public EAccessoryState AccessoryState { get; set; }

		[field: SerializeField]
		public EAccessoryVisibility AccessoryVisibility { get; private set; }

		[field: SerializeField]
		public string DefaultParent { get; set; }
		
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
		public Simulation[] Simulations { get; set; }
		
		[field: SerializeField]
		public ESupportedGendersFlags SupportedGendersFlags { get; set; }

		[field: SerializeField]
		public Renderer[] Renderers { get; set; }

		[field: NonSerialized] 
		[APLIgnore]
		public Character Character { get; private set; }

		[NonSerialized] 
		[APLIgnore]
		private IClothing boundClothing;

		public IClothing GetAccessoryBoundClothing()
		{
			return null;
		}
		
		public void SetAccessoryBoundClothing(IClothing clothing)
		{

		}

		public Transform GetAccessoryParent()
		{
			return null;
		}

		public void SetAccessoryParent(Transform parent)
		{

		}

		public void SetAccessoryVisibility(EAccessoryVisibility visibility)
		{

		}
		
		public virtual void Assign(Character chara)
		{

		}

		public virtual void Remove(Character chara)
		{

		}
		
		public void SetState(EAccessoryState state)
		{

		}
		
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