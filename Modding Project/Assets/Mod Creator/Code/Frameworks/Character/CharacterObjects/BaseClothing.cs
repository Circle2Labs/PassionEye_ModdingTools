using System;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Managers;
using Code.Tools;
using Railgun.AssetPipeline.Attributes;
using UnityEngine;

namespace Code.Frameworks.Character.CharacterObjects
{
	[Serializable]
	public class BaseClothing : MonoBehaviour, IClothing
	{
        protected const bool __metadata = true;
        
        [APLIgnore]
        public bool IsSingular => false;
        
        [field: SerializeField]
        public ECharacterObjectType ObjectType { get; set; }
		
        [field: SerializeField]
		public EClothingType ClothingType { get; set; }

		[field: SerializeField]
		public Transform[] ClothingStates { get; set; }

		[field: SerializeField]
		public EClothingState ClothingState { get; set; }

		[field: SerializeField]
		public bool Reparentable { get; set; }

		/// <summary>
		/// Default parent assigned when the object is simulated using `AttachOnly` mode.
		/// </summary>
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
		public Simulation Simulation { get; set; }
		
		[field: SerializeField]
		public SBlendshapeOffset[] BlendshapeOffsets { get; set; }
		
		[field: SerializeField]
		public float ClippingDistance { get; set; }

		[field: SerializeField]
		public byte[] BVHData { get; set; }
		
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

		public void SetupBoneMap(BaseBaseMesh baseMesh)
		{
			
		}
		
		public virtual void Remove(Character chara)
		{

		}
		
		public void SetState(EClothingState state)
		{

		}

		public GameObject GetStateObject(EClothingState state)
		{
			if (ClothingStates == null)
				return null;

			var index = (int)state;
			if (index >= ClothingStates.Length || ClothingStates[index] == null)
				return null;
            
			return ClothingStates[index].gameObject;
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

		public void AddSimulationData()
		{
			
		}
	}
}