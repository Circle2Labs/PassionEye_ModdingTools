#if UNITY_EDITOR
using System;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Structs;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.ModdedScenes.Flags;
using Code.Frameworks.Studio.Enums;
using Code.Tools;
using UnityEngine;
using Tuple = Code.Tools.Tuple;

namespace Code.EditorScripts.ModCreator
{
    [Serializable]
	public class Template
	{
		public ETemplateType TemplateType;

		[SerializeField]
		private bool advanced;
		public bool Advanced
		{
			get => advanced;
			set
			{
				var last = advanced;
				advanced = value;
				
				if (last == advanced)
					return;
				
				if (advanced)
				{
					Type = "CustomType";
					Usings = Array.Empty<string>();
					Source = $@"[Serializable]
public class {Type} : {GetTemplateClass(this)}
{{
{GetTemplateCode(this)}
}}";
				}
				else
				{
					Type = GetTemplateClass(this);
					Usings = Array.Empty<string>();
					Source = "";
				}
			}
		}
		
		public Sprite Icon;
		
		public string Name = "New Item";
		public string Description = "";
		
		public string[] Tags = Array.Empty<string>();

		public bool IsNSFW = true;

		// Advanced class type
		public string Type;

		// Advanced class usings
		public string[] Usings = Array.Empty<string>();
		
		#region Character Object

		[SerializeField]
		private ECharacterObjectType characterObjectType;
		public ECharacterObjectType CharacterObjectType
		{
			get => characterObjectType;
			set
			{
				var last = characterObjectType;
				
				characterObjectType = value;

				if (last == characterObjectType)
					return;
				
				if (!advanced)
					Type = GetTemplateClass(this);
			}
		}
		
		// Hair
		public EHairType HairType;
		
		// Clothing
		public EClothingType ClothingType;
		
		// Accessory
		public EAccessoryType AccessoryType;
		
		// Texture Type
		public ETextureType TextureType;
		
		// Texture Overlay Target
		public EOverlayTarget OverlayTarget;
		
		// Texture Overlay Mode
		public EOverlayMode OverlayMode;

		// Texture Overlay Color
		public Color OverlayColor = Color.white;
		
		// Texture is overlay
		public bool IsOverlay;
		
		// Texture
		public Texture2D Texture;

		// Default parent
		public string DefaultParent;

		// Should the object be reparentable
		public bool Reparentable = true;
		
		// Default parent index
		public int DefaultParentIdx;
		
		// Clothing states map
		public Transform[] ClothingStates;

		// Offsets used in blendshapes when making clothing
		public SBlendshapeOffset[] BlendshapeOffsets;
		
		// Compatible base meshes for character objects
		public SCompatibleBaseMesh[] CompatibleBaseMeshes;

		#region Base Mesh

		public Avatar Avatar;
		public ESupportedGendersFlags SupportedGendersFlags;
		public List<SBlendshapePreset> BlendshapePresets;
		public Transform POV;
		public Transform FaceTransform;
		public List<SBlendShape> Blendshapes;
		public List<Tools.Tuple.SerializableTuple<Transform, string>> AccessoryParents;
		public List<Tools.Tuple.SerializableTuple<ETextureType, Renderer, int>> TextureMaterialMap;
		public List<SBodyPart> BodyParts;
		public List<SkinnedMeshRenderer> BlendshapeRenderers;
		public List<Collider> SFWColliders;
		public SEyeData EyeData;
		public SMouthData MouthData;
		public Transform Cock;
		public Transform BodyRootBone;
		public Transform HeadRootBone;
		public Transform PrivatesRootBone;
		public List<Transform> Eyes;
		public List<Transform> Breasts;
		public List<Transform> Buttocks;
		public bool EyeControl = true;
		public bool ExpressionControl = true;
		public bool PoseControl = true;

		#endregion
		
		// Advanced class source
		public string Source = "";

		// Clipping fix
		public bool UseClippingFix = true;
		public float ClippingDistance = 0.001f;
		
		// Physics
		public Simulation Simulation;
		
		#endregion

		#region Studio Object

		// Studio Object
		public EStudioObjectType StudioObjectType;

		// FK
		public SFKData FKData;

		#endregion

		#region Modded Scene

		// Modded scene usage flags
		public EModdedSceneUsageFlags ModdedSceneUsageFlags;

		// Used in modded scenes 16:9 bg for the map
		public Sprite LargeBackground;

		#endregion

		#region Animation

		// Animation usage flags
		public EAnimationUsageFlags AnimationUsageFlags;

		// Animation clips
		public SClipContainer[] AnimationClipContainers = Array.Empty<SClipContainer>();

		// Animation crossfade duration
		public float AnimationFadeDuration = 0.25f;
		
		#endregion
		
		public Template()
		{
			Type = GetTemplateClass(this);
			SetupStates();
		}
		
		public Template Copy()
		{
			var copiedSimulation = Simulation.Copy();
			
			string[] copiedTags = null;
			if (Tags != null)
			{
				copiedTags = new string[Tags.Length];
				Array.Copy(Tags, copiedTags, Tags.Length);
			}

			string[] copiedIncludes = null;
			if (Usings != null)
			{
				copiedIncludes = new string[Usings.Length];
				Array.Copy(Usings, copiedIncludes, Usings.Length);
			}
			
			Transform[] copiedStates = null;
			if (ClothingStates != null)
			{
				copiedStates = new Transform[ClothingStates.Length];
				Array.Copy(ClothingStates, copiedStates, ClothingStates.Length);
			}

			SCompatibleBaseMesh[] copiedCompatibleBaseMeshes = null;
			if (CompatibleBaseMeshes != null)
			{
				copiedCompatibleBaseMeshes = new SCompatibleBaseMesh[CompatibleBaseMeshes.Length];
				Array.Copy(CompatibleBaseMeshes, copiedCompatibleBaseMeshes, CompatibleBaseMeshes.Length);
			}
			
			SClipContainer[] copiedClipContainers = null;
			if (AnimationClipContainers != null)
			{
				copiedClipContainers = new SClipContainer[AnimationClipContainers.Length];
				for (var i = 0; i < AnimationClipContainers.Length; i++)
					copiedClipContainers[i] = AnimationClipContainers[i].Copy();
			}
			
			SBlendshapeOffset[] copiedBlendshapeOffsets = null;
			if (BlendshapeOffsets != null)
			{
				copiedBlendshapeOffsets = new SBlendshapeOffset[BlendshapeOffsets.Length];
				Array.Copy(BlendshapeOffsets, copiedBlendshapeOffsets, BlendshapeOffsets.Length);
			}

			List<SBlendshapePreset> copiedBlendshapePresets = null;
			if (BlendshapePresets != null)
			{
				copiedBlendshapePresets = new List<SBlendshapePreset>();
				for (var i = 0; i < BlendshapePresets.Count; i++)
					copiedBlendshapePresets.Add(BlendshapePresets[i]);
			}

			List<Tuple.SerializableTuple<Transform, string>> copiedAccessoryParents = null;
			if (AccessoryParents != null)
			{
				copiedAccessoryParents = new List<Tuple.SerializableTuple<Transform, string>>();
				for (var i = 0; i < AccessoryParents.Count; i++)
					copiedAccessoryParents.Add(AccessoryParents[i]);
			}

			List<Tuple.SerializableTuple<ETextureType, Renderer, int>> copiedTextureMaterialMap = null;
			if (TextureMaterialMap != null)
			{
				copiedTextureMaterialMap = new List<Tuple.SerializableTuple<ETextureType, Renderer, int>>();
				for (var i = 0; i < TextureMaterialMap.Count; i++)
					copiedTextureMaterialMap.Add(TextureMaterialMap[i]);
			}

			List<SBlendShape> copiedBlendshapes = null;
			if (Blendshapes != null)
			{
				copiedBlendshapes = new List<SBlendShape>();
				for (var i = 0; i < Blendshapes.Count; i++)
					copiedBlendshapes.Add(Blendshapes[i]);
			}

			List<SBodyPart> copiedBodyParts = null;
			if (BodyParts != null)
			{
				copiedBodyParts = new List<SBodyPart>();
				for (var i = 0; i < BodyParts.Count; i++)
					copiedBodyParts.Add(BodyParts[i]);
			}

			List<SkinnedMeshRenderer> copiedBlendshapeRenderers = null;
			if (BlendshapeRenderers != null)
			{
				copiedBlendshapeRenderers = new List<SkinnedMeshRenderer>();
				for (var i = 0; i < BlendshapeRenderers.Count; i++)
					copiedBlendshapeRenderers.Add(BlendshapeRenderers[i]);
			}

			List<Collider> copiedSFWColliders = null;
			if (SFWColliders != null)
			{
				copiedSFWColliders = new List<Collider>();
				for (var i = 0; i < SFWColliders.Count; i++)
					copiedSFWColliders.Add(SFWColliders[i]);
			}

			List<Transform> copiedEyes = null;
			if (Eyes != null)
			{
				copiedEyes = new List<Transform>();
				for (var i = 0; i < Eyes.Count; i++)
					copiedEyes.Add(Eyes[i]);
			}

			List<Transform> copiedBreasts = null;
			if (Breasts != null)
			{
				copiedBreasts = new List<Transform>();
				for (var i = 0; i < Breasts.Count; i++)
					copiedBreasts.Add(Breasts[i]);
			}

			List<Transform> copiedButtocks = null;
			if (Buttocks != null)
			{
				copiedButtocks = new List<Transform>();
				for (var i = 0; i < Buttocks.Count; i++)
					copiedButtocks.Add(Buttocks[i]);
			}
			
			var template = new Template
			{
				// Base
				TemplateType = TemplateType,
				characterObjectType = CharacterObjectType,
				Icon = Icon,
				Name = Name,
				Description = Description,
				Tags = copiedTags,
				IsNSFW = IsNSFW,
				Source = Source,
				Type = Type,
				Advanced = Advanced,
				Usings = copiedIncludes,

				// Hair
				HairType = HairType,
				
				// Clothing
				ClothingType = ClothingType,
				ClothingStates = copiedStates,
				ClippingDistance = ClippingDistance,
				UseClippingFix = UseClippingFix,
				BlendshapeOffsets = copiedBlendshapeOffsets,

				// Studio
				StudioObjectType = StudioObjectType,
				
				// Accessory
				AccessoryType = AccessoryType,
				DefaultParent = DefaultParent,
				DefaultParentIdx = DefaultParentIdx,

				// Modded Scene
				ModdedSceneUsageFlags = ModdedSceneUsageFlags,
				LargeBackground = LargeBackground,

				// Texture
				TextureType = TextureType,
				Texture = Texture,
				OverlayTarget = OverlayTarget,
				OverlayMode = OverlayMode,
				IsOverlay = IsOverlay,
				OverlayColor = OverlayColor,

				// Animation
				AnimationUsageFlags = AnimationUsageFlags,
				AnimationClipContainers = copiedClipContainers,
				AnimationFadeDuration = AnimationFadeDuration,

				// Base Mesh
				Avatar = Avatar,
				SupportedGendersFlags = SupportedGendersFlags,
				BlendshapePresets = copiedBlendshapePresets,
				AccessoryParents = copiedAccessoryParents,
				TextureMaterialMap = copiedTextureMaterialMap,
				Blendshapes = copiedBlendshapes,
				BodyParts = copiedBodyParts,
				BlendshapeRenderers = copiedBlendshapeRenderers,
				SFWColliders = copiedSFWColliders,
				POV = POV,
				FaceTransform = FaceTransform,
				EyeData = EyeData,
				MouthData = MouthData,
				Cock = Cock,
				BodyRootBone = BodyRootBone,
				HeadRootBone = HeadRootBone,
				PrivatesRootBone = PrivatesRootBone,
				Eyes = copiedEyes,
				Breasts = copiedBreasts,
				Buttocks = copiedButtocks,
				EyeControl = EyeControl,
				ExpressionControl = ExpressionControl,
				PoseControl = PoseControl,
				
				// Shared
				Simulation = copiedSimulation,
				FKData = FKData,
				CompatibleBaseMeshes = copiedCompatibleBaseMeshes
			};

			return template;
		}

		public void SetupStates()
		{
			ClothingStates ??= new Transform[Enum.GetNames(typeof(EClothingState)).Length];

			for (var i = 0; i < ClothingStates.Length; i++)
			{
				if (ClothingStates[i] != null)
					continue;

				ClothingStates[i] = null;
			}
		}
		
		public static string GetTemplateClass(Template template)
		{
			if (template.TemplateType == ETemplateType.CharacterObject)
			{
				switch (template.CharacterObjectType)
				{
					case ECharacterObjectType.Accessory:
						return "BaseAccessory";
					case ECharacterObjectType.Clothing:
						return "BaseClothing";
					case ECharacterObjectType.Hair:
						return "BaseHair";
					case ECharacterObjectType.Texture:
						return "BaseTexture";
					case ECharacterObjectType.BaseMesh:
						return "BaseBaseMesh";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else if (template.TemplateType == ETemplateType.StudioObject) 
				return "BaseStudioObject";

			return "";
		}

		public static string GetTemplateCode(Template template)
		{
			if (template.TemplateType == ETemplateType.CharacterObject)
			{
				return $@"
	public override void Assign(Character character)
	{{
		base.Assign(character);
	}}

	public override void Remove(Character character)
	{{
		base.Remove(character);
	}}
";
			}
			
			if (template.TemplateType == ETemplateType.StudioObject)
			{
				return $@"
	public override void Awake()
	{{
		base.Awake();
	}}
";
			}

			return "";
		}
	}
	
	public enum ETemplateType
	{
		CharacterObject,
		StudioObject,
		ModdedScene,
		Animation
	}
}
#endif