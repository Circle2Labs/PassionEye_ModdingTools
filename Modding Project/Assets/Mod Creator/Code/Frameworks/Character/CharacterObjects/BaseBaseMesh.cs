using System;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ForwardKinematics;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Managers;
using Code.Tools;
using Railgun.AssetPipeline.Attributes;
using UnityEngine;

namespace Code.Frameworks.Character.CharacterObjects
{
	[Serializable]
	public class BaseBaseMesh : MonoBehaviour, IBaseMesh
	{
		protected const bool __metadata = true;

		[APLIgnore]
		public bool IsSingular => true;
		
		[field: SerializeField]
		public ECharacterObjectType ObjectType { get; set; }

		[field: SerializeField]
		public SAvatarData AvatarData { get; set; }

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
		
		/// <summary>
		/// Not supported for this type of object.
		/// </summary>
		[field: SerializeField]
		public bool Reparentable { get; set; }

		/// <summary>
		/// Not supported for this type of object.
		/// </summary>
		[field: SerializeField]
		public string DefaultParent { get; set; }

		[field: SerializeField] 
		public Simulation Simulation { get; set; }
		
		[field: SerializeField]
		public ESupportedGendersFlags SupportedGendersFlags { get; set; }

		[field: SerializeField]
		public SBlendshapePreset[] BlendshapePresets { get; set; }
		
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
		
		public int GetLOD(SkinnedMeshRenderer rend)
		{
			return 0;
		}
		
		public void SetupBoneMap(BaseBaseMesh baseMesh)
		{
			
		}

		public void AddSimulationData()
		{
			
		}
	}
}