using System;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Tools;
using Railgun.AssetPipeline.Attributes;
using UnityEngine;
using Code.Frameworks.Shared.Structs;
using Railgun.ModEngine.Types;

namespace Code.Frameworks.Character.CharacterObjects
{
	[Serializable]
	public class BaseTexture : MonoBehaviour, ITexture
	{
		public GameObjectMetadata Metadata { get; set; }

		[APLIgnore]
		public bool IsSingular => false;
		
		[field: SerializeField]
		public ECharacterObjectType ObjectType { get; set; }

		[field: SerializeField]
		public ETextureType TextureType { get; set; }
		
		[field: SerializeField]
		public EOverlayTarget OverlayTarget { get; set; }
		
		[field: SerializeField]
		public EOverlayMode OverlayMode { get; set; }
		
		[field: SerializeField]
		public Texture2D Texture { get; set; }
		
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
		public SVariant[] Variants { get; set; }

		[field: SerializeField]
		public int Variant { get; set; } = -1;

		/// <summary>
		/// Not supported for this type of object.
		/// </summary>
		public bool Reparentable { get; set; }

		/// <summary>
		/// Not supported for this type of object.
		/// </summary>
		[field: SerializeField]
		public string DefaultParent { get; set; }

		/// <summary>
		/// Not supported for this type of object.
		/// </summary>
		[field: SerializeField]
		public Simulation Simulation { get; set; }
		
		[field: SerializeField] 
		public Simulation[] Simulations { get; set; }
		
		[field: SerializeField]
		public SCompatibleBaseMesh[] CompatibleBaseMeshes { get; set; }

		[field: SerializeField]
		public Renderer[] Renderers { get; set; }
		
		[field: NonSerialized] 
		[APLIgnore]
		public Character Character { get; private set; }
		
		[field: SerializeField]
		[APLIgnore]
		public bool IsCustom { get; set; }
		
		[field: SerializeField]
		public bool IsOverlay { get; set; }
		
		[field: SerializeField]
		public Color OverlayColor { get; set; }
		
		public void SetVariant(int variant)
		{
			
		}
		
		public virtual void Assign(Character chara)
		{

		}

		public virtual void Remove(Character chara)
		{
			
		}
		
		public void UpdateTexture(bool updateOverlays = true)
		{
			
		}

		public void ApplyTexture(Texture2D texture)
		{
			
		}

		public void UpdateColor(bool reset = false)
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
		
		public void SetupBoneMap(IBaseMesh baseMesh)
		{
			
		}

		public void AddSimulationData()
		{
			
		}
	}
}