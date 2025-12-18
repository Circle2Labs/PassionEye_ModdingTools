using System;
using System.Collections.Generic;
using Animancer;
using Code.Components;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ClippingFix.Enums;
using Code.Frameworks.ForwardKinematics;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.RayTracing;
using Code.Managers;
using Code.Tools;
using Railgun.AssetPipeline.Attributes;
using Railgun.AssetPipeline.Models;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Material = UnityEngine.Material;
using Ray = Code.Frameworks.RayTracing.Ray;
using Shader = UnityEngine.Shader;
using Sprite = UnityEngine.Sprite;

namespace Code.Frameworks.Character.CharacterObjects
{
	[Serializable]
	public class BaseBaseMesh : MonoBehaviour, IBaseMesh
	{
		protected const bool __metadata = true;

		[APLIgnore]
		public bool IsSingular => true;
		
		#region ICharacterObject Data

		[field: SerializeField]
		public ECharacterObjectType ObjectType { get; set; }
		
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
		/// Specifies base meshes compatible with this base mesh
		/// To explain it better, an item made for a base mesh inside this array should also be (mostly or fully) compatible with this base mesh
		/// </summary>
		[field: SerializeField]
		public SCompatibleBaseMesh[] CompatibleBaseMeshes { get; set; }

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
		
		[field: NonSerialized][APLIgnore]
		public Character Character { get; private set; }
		
		#endregion

		#region IMaterialCustomizable Data

		[field: SerializeField]
		public Renderer[] Renderers { get; set; }

		#endregion

		#region Serialized Game Data

        [field: SerializeField]
        public SAvatarData AvatarData { get; set; }
		
        [field: SerializeField]
        public ESupportedGendersFlags SupportedGendersFlags { get; set; }
        
        [field: SerializeField]
        public SBlendshapePreset[] BlendshapePresets { get; set; }
        
        [field: SerializeField]
		public List<Tools.Tuple.SerializableTuple<Transform, string>> AccessoryParents { get; set; }
		[field: SerializeField]
		public List<Tools.Tuple.SerializableTuple<ETextureType, Renderer, int>> TextureMaterialMap { get; set; }
        
		[field: SerializeField]
		public List<SBlendShape> Blendshapes { get; set; }
		[field: SerializeField]
		public List<SBodyPart> BodyParts { get; set; }
		
		[field: SerializeField]
		public List<SkinnedMeshRenderer> BlendshapeRenderers { get; set; }
		
		[field: SerializeField]
		public List<Collider> SFWColliders { get; set; }
		
		[field: SerializeField]
		public Transform POV { get; set; }
		[field: SerializeField]
		public Transform FaceTransform { get; set; }

		[field: SerializeField]
		public SEyeData EyeData { get; set; }
		[field: SerializeField]
		public SMouthData MouthData { get; set; }
		
		[field: SerializeField]
		public Transform Cock { get; set; }
		[field: SerializeField]
		public Transform HideVag { get; set; }
		[field: SerializeField]
		public Transform BodyRootBone { get; set; }
		[field: SerializeField]
		public Transform HeadRootBone { get; set; }
		[field: SerializeField]
		public Transform PrivatesRootBone { get; set; }
		
		[field: SerializeField]
		public Transform MergedEyes { get; set; }
		[field: SerializeField]
		public bool InvertMergedEyes { get; set; }
		
		[field: SerializeField]
		public Transform[] Eyes { get; set; }
		[field: SerializeField]
		public Transform[] Breasts { get; set; }
        [field: SerializeField]
        public Transform[] Buttocks { get; set; }
        [field: SerializeField]
        public Transform[] Balls { get; set; }

        [field: SerializeField]
        public bool EyeControl { get; set; } = true;
        [field: SerializeField]
        public bool ExpressionControl { get; set; } = true;
        [field: SerializeField]
        public bool PoseControl { get; set; } = true;
		
        [field: SerializeField]
        public List<Tools.Tuple.SerializableTuple<ERaysResolution, byte>> RaysIDs { get; set; }
        
		#endregion
		
		#region Runtime Game Data

		[field: NonSerialized][APLIgnore]
		public Avatar Avatar { get; set; }

		#endregion
		
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
		
		public void SetupBoneMap(IBaseMesh baseMesh)
		{
			
		}

		public void AddSimulationData()
		{
			
		}
		
		#region Runtime Game Methods
		
        public (SkinnedMeshRenderer, int)? GetBodyRenderer()
        {
	        foreach (var tuple in TextureMaterialMap)
	        {
		        if (tuple.Item1 != ETextureType.BodySkin)
			        continue;

		        return ((SkinnedMeshRenderer)tuple.Item2, tuple.Item3);
	        }
			
	        return null;
        }
        
        #endregion
	}
}