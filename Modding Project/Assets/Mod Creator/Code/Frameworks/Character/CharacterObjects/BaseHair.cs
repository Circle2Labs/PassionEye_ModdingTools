using System;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Railgun.AssetPipeline.Attributes;
using UnityEngine;

namespace Code.Frameworks.Character.CharacterObjects
{
	[Serializable]
	public class BaseHair : MonoBehaviour, IHair
	{
        protected const bool __metadata = true;
        
        [APLIgnore]
        public bool IsSingular => false;
        
        [field: SerializeField]
        public ECharacterObjectType ObjectType { get; set; }

        [field: SerializeField]
		public EHairType HairType { get; set; }

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
		public ESupportedGendersFlags SupportedGendersFlags { get; set; }

		[field: SerializeField]
		public Renderer[] Renderers { get; set; }

		[field: NonSerialized] 
		[APLIgnore]
		public Character Character { get; private set; }

		public virtual void Assign(Character chara)
		{

		}

		public virtual void Remove(Character chara)
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
	}
}