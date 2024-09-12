#if UNITY_EDITOR
using System;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.ModdedScenes.Flags;
using Code.Frameworks.Studio.Enums;
using UnityEngine;

namespace Code.EditorScripts.ModCreator
{
    [Serializable]
	public class Template
	{
		public ETemplateType TemplateType;
		
		[SerializeField]
		private ECharacterObjectType characterObjectType;
		public ECharacterObjectType CharacterObjectType
		{
			get => characterObjectType;
			set
			{
				var last = characterObjectType;
				
				// todo: remove once basemesh stuff is set up
				if (value is ECharacterObjectType.BaseMesh)
					characterObjectType = ECharacterObjectType.Clothing;
				else
					characterObjectType = value;

				if (last == characterObjectType)
					return;
				
				if (!advanced)
					Type = GetTemplateClass(this);
			}
		}

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
		
		// Hair
		public EHairType HairType;
		
		// Clothing
		public EClothingType ClothingType;
		
		// Studio Object
		public EStudioObjectType StudioObjectType;
		
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

		// Character object allowed gender flags
		public ESupportedGendersFlags SupportedGendersFlags;
		
		// Modded scene usage flags
		public EModdedSceneUsageFlags ModdedSceneUsageFlags;

		// Clothing states map
		public Transform[] ClothingStates;

		// Physics
		public Simulation Simulation;
		
		// FK
		public SFKData FKData;

		// Offsets used in blendshapes when making clothing
		public SBlendshapeOffset[] BlendshapeOffsets;
		
		// Advanced class source
		public string Source = "";

		// Advanced class type
		public string Type;

		// Advanced class usings
		public string[] Usings = Array.Empty<string>();

		public Template()
		{
			Type = GetTemplateClass(this);
			SetupStates();
		}
		
		public Template Copy()
		{
			var copiedTags = new string[Tags.Length];
			Array.Copy(Tags, copiedTags, Tags.Length);
			
			var copiedIncludes = new string[Usings.Length];
			Array.Copy(Usings, copiedIncludes, Usings.Length);
			
			var copiedStates = new Transform[ClothingStates.Length];
			Array.Copy(ClothingStates, copiedStates, ClothingStates.Length);

			var copiedSimulation = Simulation.Copy();
			
			var copiedBlendshapeOffsets = new SBlendshapeOffset[BlendshapeOffsets.Length];
			Array.Copy(BlendshapeOffsets, copiedBlendshapeOffsets, BlendshapeOffsets.Length);
			
			var template = new Template
			{
				TemplateType = TemplateType,
				characterObjectType = CharacterObjectType,
				Icon = Icon,
				Name = Name,
				Description = Description,
				Tags = copiedTags,
				IsNSFW = IsNSFW,
				SupportedGendersFlags = SupportedGendersFlags,
				HairType = HairType,
				ClothingType = ClothingType,
				StudioObjectType = StudioObjectType,
				AccessoryType = AccessoryType,
				DefaultParent = DefaultParent,
				DefaultParentIdx = DefaultParentIdx,
				ModdedSceneUsageFlags = ModdedSceneUsageFlags,
				ClothingStates = copiedStates,
				Source = Source,
				Type = Type,
				Advanced = Advanced,
				Usings = copiedIncludes,
				Simulation = copiedSimulation,
				TextureType = TextureType,
				Texture = Texture,
				OverlayTarget = OverlayTarget,
				OverlayMode = OverlayMode,
				IsOverlay = IsOverlay,
				OverlayColor = OverlayColor,
				BlendshapeOffsets = copiedBlendshapeOffsets,
				FKData = FKData
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
		ModdedScene
	}
}
#endif