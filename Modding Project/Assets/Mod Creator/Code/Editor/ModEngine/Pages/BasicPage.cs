using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.ModdedScenes.Flags;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.Studio.Enums;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ClippingFix.Enums;
using Code.Frameworks.RayTracing;
using UnityEngine.Rendering;
using Ray = Code.Frameworks.RayTracing.Ray;
using Timer = System.Timers.Timer;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator 
	{
		[SerializeField]
		public bool[] BasicFolds = new bool[3];
		
		private static GUIStyle guiStyle;
		public static GUIStyle GUIStyle
		{
			get
			{
				if (guiStyle != null)
					return guiStyle;

				guiStyle = new GUIStyle();
				return guiStyle;
			}
		}
		
		private static Texture2D texture;
		public static Texture2D Texture
		{
			get
			{
				if (texture != null)
					return texture;

				texture = new Texture2D(1, 1);
				return texture;
			}
		}

		// todo: figure out a better solution. Maybe allow the user to pick for which base mesh they are making the item and then populate this list accordingly.
		private readonly string[] defaultParents =
		{
			// enpi
			//"DEF_hips.001","DEF_buttocks.L.001","DEF_buttocks.L.001_end","DEF_buttocks.R.001","DEF_buttocks.R.001_end","DEF_hips.002","DEF_hips.002_end","DEF_hips.L.001","DEF_thigh.L.001","DEF_thigh.L.002","DEF_shin.L.001","DEF_feet.L.001","DEF_feet.L.003","DEF_feet.L.003_end","DEF_Toe_master.L.001","DEF_index_toe.L.002","DEF_index_toe.L.003","DEF_index_toe.L.003_end","DEF_middle_toe.L.002","DEF_middle_toe.L.003","DEF_middle_toe.L.003_end","DEF_pinky_toe.L.002","DEF_pinky_toe.L.002_end","DEF_ring_toe.L.002","DEF_ring_toe.L.003","DEF_ring_toe.L.003_end","DEF_thumb_toe.L.002","DEF_thumb_toe.L.002_end","DEF_hips.R.001","DEF_thigh.R.001","DEF_thigh.R.002","DEF_shin.R.001","DEF_feet.R.001","DEF_feet.R.003","DEF_feet.R.003_end","DEF_Toe_master.R.001","DEF_index_toe.R.002","DEF_index_toe.R.003","DEF_index_toe.R.003_end","DEF_middle_toe.R.002","DEF_middle_toe.R.003","DEF_middle_toe.R.003_end","DEF_pinky_toe.R.002","DEF_pinky_toe.R.002_end","DEF_ring_toe.R.002","DEF_ring_toe.R.003","DEF_ring_toe.R.003_end","DEF_thumb_toe.R.002","DEF_thumb_toe.R.002_end","DEF_penis.001","DEF_penis.002","DEF_penis.003","DEF_penis.003_end","DEF_spine.001","DEF_spine.002","DEF_spine.003","DEF_breast.L.001","DEF_breast.L.002","DEF_breast.L.003","DEF_breast.L.003_end","DEF_breast.R.001","DEF_breast.R.002","DEF_breast.R.003","DEF_breast.R.003_end","DEF_neck.001","DEF_head.001","DEF_Ear_L.001","DEF_Ear_L.002","DEF_Ear_L.003","DEF_Ear_L.004","DEF_Ear_L.005","DEF_Ear_L.005_end","DEF_Ear_R.001","DEF_Ear_R.002","DEF_Ear_R.003","DEF_Ear_R.004","DEF_Ear_R.005","DEF_Ear_R.005_end","DEF_eye_L.001","DEF_eye_L.001_end","DEF_eye_R.001","DEF_eye_R.001_end","DEF_Jaw.001","DEF_Jaw_L.002","DEF_Jaw_L.003","DEF_Jaw_L.004","DEF_Jaw_L.004_end","DEF_Jaw_R.002","DEF_Jaw_R.003","DEF_Jaw_R.004","DEF_Jaw_R.004_end","DEF_LipMouthBottom.001","DEF_LipMouthBottom.001_end","DEF_tongue.001","DEF_tongue.002","DEF_tongue.003","DEF_tongue.003_end","DEF_Nose.001","DEF_Nose.001_end","DEF_Scalp_L.001","DEF_Scalp_L.002","DEF_Scalp_L.003","DEF_Scalp_L.004","DEF_Scalp_L.004_end","DEF_Scalp_R.001","DEF_Scalp_R.002","DEF_Scalp_R.003","DEF_Scalp_R.004","DEF_Scalp_R.004_end","DEF_UpperCheek_L.001","DEF_UpperCheek_L.002","DEF_UpperCheek_L.003","DEF_UpperCheek_L.003_end","DEF_UpperCheek_R.001","DEF_UpperCheek_R.002","DEF_UpperCheek_R.003","DEF_UpperCheek_R.003_end","DEF_LipMouthUpper.001","DEF_LipMouthUpper.001_end","DEF_shoulder.L.001","DEF_biceps.L.001","DEF_biceps.L.002","DEF_forearm.L.001","DEF_hand.L.001","DEF_index.L.002","DEF_index.L.003","DEF_index.L.004","DEF_index.L.004_end","DEF_middle.L.002","DEF_middle.L.003","DEF_middle.L.004","DEF_middle.L.004_end","DEF_pinky.L.002","DEF_pinky.L.003","DEF_pinky.L.004","DEF_pinky.L.004_end","DEF_ring.L.002","DEF_ring.L.003","DEF_ring.L.004","DEF_ring.L.004_end","DEF_thumb.L.001","DEF_thumb.L.002","DEF_thumb.L.003","DEF_thumb.L.003_end","DEF_shoulder.R.001","DEF_biceps.R.001","DEF_biceps.R.002","DEF_forearm.R.001","DEF_hand.R.001","DEF_index.R.002","DEF_index.R.003","DEF_index.R.004","DEF_index.R.004_end","DEF_middle.R.002","DEF_middle.R.003","DEF_middle.R.004","DEF_middle.R.004_end","DEF_pinky.R.002","DEF_pinky.R.003","DEF_pinky.R.004","DEF_pinky.R.004_end","DEF_ring.R.002","DEF_ring.R.003","DEF_ring.R.004","DEF_ring.R.004_end","DEF_thumb.R.001","DEF_thumb.R.002","DEF_thumb.R.003","DEF_thumb.R.003_end"
			// rappy
			"Armature","Hips","Buttocks.L.001","Buttocks.R.001","Penis.000","Penis.001","Penis.002","Spine","Chest","UpperChest","Breast.L.001","Breast.L","Breast.R.001","Breast.R","Neck","Head","Cheak.L","Cheak_shape.L","Cheak.R","Cheak_shape.R","Chin.L","Chin.R","Ctrl.lip.corner.L","Ctrl.lip.corner.R","Ctrl.lip.lower","lip.lower.L","lip.lower.R","Ctrl.lip.lower.L","lip.lower.L.001","Ctrl.lip.lower.R","lip.lower.R.001","Ctrl.lip.upper","lip.upper.L","lip.upper.R","Ctrl.lip.upper.L","lip.upper.L.001","Ctrl.lip.upper.R","lip.upper.R.001","ctrl.tongue.000","ctrl.tongue.001","ctrl.tongue.002","ear rings.L","ear rings.R","Eye.L","Eye.R","Jaw_shape.L","Jaw.L","Jaw_shape.R","Jaw.R","teeth.lower","teeth.upper","tongue.000","tongue.001","tongue.002","Shoulder.L","UpperArm.L","LowerArm.L","Hand.L","palm.01.L","f_index.01.L","f_index.02.L","f_index.03.L","thumb.01.L","thumb.02.L","thumb.03.L","palm.02.L","f_middle.01.L","f_middle.02.L","f_middle.03.L","palm.03.L","f_ring.01.L","f_ring.02.L","f_ring.03.L","palm.04.L","f_pinky.01.L","f_pinky.02.L","f_pinky.03.L","Shoulder.R","UpperArm.R","LowerArm.R","Hand.R","palm.01.R","f_index.01.R","f_index.02.R","f_index.03.R","thumb.01.R","thumb.02.R","thumb.03.R","palm.02.R","f_middle.01.R","f_middle.02.R","f_middle.03.R","palm.03.R","f_ring.01.R","f_ring.02.R","f_ring.03.R","palm.04.R","f_pinky.01.R","f_pinky.02.R","f_pinky.03.R","UpperLeg.L","LowerLeg.L","Foot.L","Toes.L","Toes.Index.L.001","Toes.Index.L.002","Toes.Middle.L.001","Toes.Middle.L.002","Toes.Pinky.L.001","Toes.Ring.L.001","Toes.Ring.L.002","Toes.Thumb.L.001","Toes.Thumb.L.002","UpperLeg.R","LowerLeg.R","Foot.R","Toes.R","Toes.Index.R.001","Toes.Index.R.002","Toes.Middle.R.001","Toes.Middle.R.002","Toes.Pinky.R.001","Toes.Ring.R.001","Toes.Ring.R.002","Toes.Thumb.R.001","Toes.Thumb.R.002","Waist.L","Waist.R"
		};

		private readonly string[] bodyBlendshapes =
		{
			"Breast Size", "Breast Depth", "Breast Shape", "Nipple Depth", "Nipple Size", "Areola Size", "Areola Depth", "Neck Width", "Neck Thickness", "Upper Torso Width", "Upper Torso Thickness", "Lower Torso Width", "Lower Torso Thickness", "Torso Position", "Torso Thickness", "Waist Width", "Waist Thickness", "Hip Width", "Hip Thickness", "Butt Size", "Butt Angle", "Butt Spacing", "Shoulder Width", "Shpulder Thickness", "Upper Arm Width", "Upper Arm Thickness", "Elbow Width", "Elbow Thickness", "Forearm Width", "Forearm Thickness", "Wrist Scale", "Hand Scale", "Upper Thigh Width", "Upper Thigh Thickness", "Lower Thigh Width", "Lower Thigh Thickness", "Knee Width", "Knee Thickness", "Calves Vertical Position", "Calves Definition", "Ankle Width", "Ankle Thickness", "Labia 1 Length", "Labia 1 Width", "Labia 2 Size", "Labia 2 Thickness", "Labia 1 Opening", "Labia 1 Spread", "Labia 2 Opening", "Labia 2 Spread", "Clitoris Size", "Vaginal Hole Size"
		};
		
		private EPreset selectedPhysicsPreset;
		
		private Vector2 basicScrollPosition;

		private bool blendshapeOffsetsFold;
		private bool physicsSimulationFold;
		private bool forwardKinematicsFold;
		
		private Template copyBuffer;

		private Tuple<Transform, SkinnedMeshRenderer[]> previewingClothingClone;
		private Template previewingTemplate;
		private EClothingState previewingState;
		private Dictionary<string, Transform> previewingBodyBones;
		private Timer previewTimer;

		private readonly SynchronizationContext context = SynchronizationContext.Current;

		private GameObject previewingBody;
		public GameObject PreviewingBody
		{
			get
			{
				if (previewingBody != null)
					return previewingBody;

				previewingBodyMaterial = null;
				
				previewingBody = Instantiate(Resources.Load<GameObject>("ClippingFixPreviewBody"));
				previewingBodyBones = new Dictionary<string,Transform>();
				
				var renderer = PreviewingBody.transform.Find("LOD0").GetComponent<SkinnedMeshRenderer>();
				var bones = renderer.bones;

				foreach (var bone in bones)
					previewingBodyBones.Add(bone.name, bone);
				
				return previewingBody;
			}
		}

		private Material previewingBodyMaterial;
		public Material PreviewingBodyMaterial
		{
			get
			{
				if (previewingBodyMaterial != null)
					return previewingBodyMaterial;
				
				// This is not using sharedMaterial because it would modify the material of the file that modders use for reference
				previewingBodyMaterial = PreviewingBody.transform.Find("LOD0").GetComponent<Renderer>().material;
				return PreviewingBodyMaterial;
			}
		}
		
		private CustomRenderTexture previewingClippingFixTexture;
		public CustomRenderTexture PreviewingClippingFixTexture
		{
			get
			{
				if (previewingClippingFixTexture != null)
					return previewingClippingFixTexture;
				
				var resolution = BodyRaysResolution;

				previewingClippingFixTexture =
					new CustomRenderTexture((int)resolution, (int)resolution, RenderTextureFormat.R8);
				previewingClippingFixTexture.enableRandomWrite = true;
				previewingClippingFixTexture.doubleBuffered = true;
				previewingClippingFixTexture.material = CoreUtils.CreateEngineMaterial(Shader.Find("White"));
				previewingClippingFixTexture.Create();
				previewingClippingFixTexture.Initialize();
				previewingClippingFixTexture.Update();
				
				// set the texture to the material
				PreviewingBodyMaterial.SetTexture("_AlphaMap", previewingClippingFixTexture);
				
				return previewingClippingFixTexture;
			}
		}

		private bool hidePreviewRenderers;
		public bool HidePreviewRenderers
		{
			get => hidePreviewRenderers;
			set
			{
				var previousValue = hidePreviewRenderers;
				hidePreviewRenderers = value;
				
				if (previewingTemplate == null || previousValue == hidePreviewRenderers)
					return;
				
				previewingClothingClone.Item1.gameObject.SetActive(!value);
			}
		}

		private ERaysResolution bodyRaysResolution;

		public ERaysResolution BodyRaysResolution
		{
			get
			{
				switch ((int)bodyRaysResolution)
				{
					case 0:
						return ERaysResolution.Low;
					case 1:
						return ERaysResolution.Medium;
					case 2:
						return ERaysResolution.High;
					case 3:
						return ERaysResolution.VeryHigh;
				}

				return ERaysResolution.Low;
			}
		}

		// todo: handle multiple base meshes
		private Dictionary<ERaysResolution, Ray[]> bodyRays;
		public Dictionary<ERaysResolution, Ray[]> BodyRays
		{
			get
			{
				if (bodyRays != null)
					return bodyRays;

				string raysPath;
				
				if (IsStandalone)
					raysPath = "Assets/Mod Creator/ClippingFixRays/pr.PR-builtin.BaseMeshFemale-";
				else
					raysPath = "Assets/GameAssets/Addressables/ClippingFixRays/pr.PR-builtin.BaseMeshFemale-";
				
				var dict = new Dictionary<ERaysResolution, Ray[]>();
				
				var resolutions = Enum.GetValues(typeof(ERaysResolution));
				for (var i = 0; i < resolutions.Length; i++)
				{
					var resolution = (ERaysResolution)resolutions.GetValue(i);
					
					var bytes = File.ReadAllBytes($"{raysPath}{resolution.ToString()}.bytes");
					var rays = bytes.ToRaysPtr();

					dict[resolution] = rays;
				}

				bodyRays = dict;
				return bodyRays;
			}
		}
		
		// actual properties for baking
		
		private BVHNodeGPU[] bvhNodes;
		private Triangle[] triangles;
		
		// compute shaders
		private ComputeShader clippingFixShader;
		private int clippingFixKernel;

		private ComputeShader dilaterShader;
		private int dilaterKernel;
		
		private ComputeBuffer raysBuffer;

		private CustomRenderTexture bakerTempRT;
		
		public void DrawBasic()
		{
			if (Templates == null || Templates.Count == 0)
				return;
			
			var template = Templates[CurrentTemplate];
			
			basicScrollPosition = GUILayout.BeginScrollView(basicScrollPosition);
			
			if (template.TemplateType == ETemplateType.CharacterObject)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.CharacterObjectType = (ECharacterObjectType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_TYPE")}*", template.CharacterObjectType, "MODCREATOR_BASIC_TYPE_");
				template.SupportedGendersFlags = (ESupportedGendersFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_GENDERS")}*", template.SupportedGendersFlags);

				switch (template.CharacterObjectType)
				{
					case ECharacterObjectType.Hair:
						template.HairType = (EHairType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_HAIRSLOT")}*", template.HairType, "MODCREATOR_BASIC_HAIRSLOT_");
						
                        GUILayout.Space(2);

						// todo: hide until you can actually reparent hair
						/*if(template.Simulation.BoneAssignmentMode == EBoneAssignmentMode.AttachOnly)
						{
                            template.Reparentable = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_REPARENTABLE")}*", template.Reparentable);
                            if (template.Reparentable)
                            {
                                template.DefaultParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultParentIdx, defaultParents);
                                template.DefaultParent = defaultParents[template.DefaultParentIdx];
                            }
                        }*/
						template.Reparentable = false;

                        GUILayout.Space(5);

						physicsSimulation(template);
						break;
					case ECharacterObjectType.Clothing:
						template.ClothingType = (EClothingType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_CLOTHSLOT")}*", template.ClothingType, "MODCREATOR_BASIC_CLOTHSLOT_");

						GUILayout.Space(10);
						
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSTATE"), EditorStyles.boldLabel);
						
						for (var i = 0; i < template.ClothingStates.Length; i++)
						{
							var stateEnum = (EClothingState)i;
							if (stateEnum == EClothingState.Off)
								continue;

							var state = $"{GetLocalizedString($"MODCREATOR_BASIC_CLOTHSTATE_{stateEnum.ToString().ToUpper()}")} state{(stateEnum == EClothingState.Full ? "*" : "")}";
							var obj = (Transform)EditorGUILayout.ObjectField(state, template.ClothingStates[i], typeof(Transform), true);
							
							template.ClothingStates[i] = obj;
						}

						GUILayout.Space(10);

						clippingFix(template);
						
						if (!IsStandalone)
						{
							GUILayout.Space(5);

							blendshapeOffsets(template);
						}

                        GUILayout.Space(2);

						// todo: hide until you can actually reparent and offset clothing
                        /*if (template.Simulation.BoneAssignmentMode == EBoneAssignmentMode.AttachOnly)
                        {
                            template.Reparentable = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_REPARENTABLE")}*", template.Reparentable);
                            if (template.Reparentable)
                            {
                                template.DefaultParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultParentIdx, defaultParents);
                                template.DefaultParent = defaultParents[template.DefaultParentIdx];
                            }
                        }*/
						template.Reparentable = false;

                        GUILayout.Space(5);
						
						physicsSimulation(template);
						break;
					case ECharacterObjectType.Accessory:
						template.AccessoryType = (EAccessoryType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_ACCSLOT")}*", template.AccessoryType, "MODCREATOR_BASIC_ACCSLOT_");
						
						GUILayout.Space(2);

						template.Reparentable = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_REPARENTABLE")}*", template.Reparentable);
						if (template.Reparentable)
						{
							template.DefaultParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultParentIdx, defaultParents);
							template.DefaultParent = defaultParents[template.DefaultParentIdx];
						}
						
						GUILayout.Space(5);
						
						physicsSimulation(template);

						if (template.Simulation.BoneAssignmentMode == EBoneAssignmentMode.Full)
							template.Reparentable = false;
						break;
					case ECharacterObjectType.Texture:
						template.TextureType = (ETextureType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_TEXTYPE")}*", template.TextureType, "MODCREATOR_BASIC_TEXTYPE_");

						if (template.TextureType is ETextureType.BodyOverlay or ETextureType.Nipples)
						{
							template.OverlayTarget = EOverlayTarget.Body;
							template.OverlayMode = EOverlayMode.FullTexture;
							template.IsOverlay = true;
						}
						else if (template.TextureType is ETextureType.FaceOverlay or ETextureType.Lips or ETextureType.Blush)
						{
							template.OverlayTarget = EOverlayTarget.Face;
							template.OverlayMode = EOverlayMode.FullTexture;
							template.IsOverlay = true;
						}
						else
						{
							template.IsOverlay = false;
						}

						if (template.IsOverlay)
							template.OverlayColor = EditorGUILayout.ColorField($"{GetLocalizedString("MODCREATOR_BASIC_OVERLAYCOLOR")}*", template.OverlayColor);
						
						GUILayout.BeginHorizontal();
						template.Texture = (Texture2D)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_TEX")}*", template.Texture, typeof(Texture2D), false, GUILayout.Width(128 + 150), GUILayout.Height(128));
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				
				GUILayout.Space(10);
			}
			else if (template.TemplateType == ETemplateType.StudioObject)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.StudioObjectType = (EStudioObjectType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_STUDIOSLOT")}*", template.StudioObjectType, "MODCREATOR_BASIC_STUDIOSLOT_");
				
				GUILayout.Space(5);
						
				forwardKinematics(template);
				
				GUILayout.Space(10);
			}
			else if (template.TemplateType == ETemplateType.ModdedScene)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.ModdedSceneUsageFlags = (EModdedSceneUsageFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_USAGE")}*", template.ModdedSceneUsageFlags);
				// todo: figure out how to localize this
				GUILayout.Space(10);
			}

			EditorGUILayout.LabelField(
				template.TemplateType == ETemplateType.ModdedScene
					? GetLocalizedString("MODCREATOR_BASIC_SCENECONF")
					: GetLocalizedString("MODCREATOR_BASIC_ITEMCONF"), EditorStyles.boldLabel);

			template.Tags ??= Array.Empty<string>();
			
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginVertical();
			
			template.Name = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_NAME")}*", template.Name);
			template.Description = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_DESC"), template.Description);
			
			GUILayout.EndVertical();

			EditorGUIUtility.labelWidth = 35;
			template.Icon = (UnityEngine.Sprite)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_ICON")}*", template.Icon, typeof(UnityEngine.Sprite), true, GUILayout.Width(95));
			EditorGUIUtility.labelWidth = 0;
			
			GUILayout.EndHorizontal();
			
			GUILayout.Space(10);
			
			template.Tags = verticalList(template.Tags, GetLocalizedString("MODCREATOR_BASIC_TAGS"));
			
			GUILayout.Space(10);

			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWCAT"), EditorStyles.boldLabel);

			// custom base meshes are bound to have some sort of compatibility issues, for safety stick to the default ones marked as sfw
			if (template.TemplateType is ETemplateType.CharacterObject && template.CharacterObjectType is ECharacterObjectType.BaseMesh)
			{
				template.IsNSFW = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_ISNSFW")}*", true);
			}
			else
			{
				template.IsNSFW = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_ISNSFW")}*", template.IsNSFW);
			}
			
			GUILayout.Space(5);
			
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWWARN1"));
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWWARN2"));
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWWARN3"));

			GUILayout.EndScrollView();
	
			GUILayout.FlexibleSpace();
			
			DrawAdvancedToggle(template);
			
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_COPY")))
				copyBuffer = template.Copy();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_PASTE")))
			{
				if (template.TemplateType != copyBuffer.TemplateType)
					return;
				
				Templates[CurrentTemplate] = copyBuffer.Copy();
			}

			GUILayout.EndHorizontal();
		}
		
		public GUIStyle GetBackgroundStyle(Color color)
		{
			Texture.SetPixel(0, 0, color);
			Texture.Apply();
			
			GUIStyle.normal.background = Texture;
			return GUIStyle;
		}

		private void blendshapeOffsets(Template template)
		{
			GUILayout.Space(5);

			template.BlendshapeOffsets ??= Array.Empty<SBlendshapeOffset>();
			var tempList = template.BlendshapeOffsets.ToList();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			blendshapeOffsetsFold = EditorGUILayout.Foldout(blendshapeOffsetsFold, GetLocalizedString("MODCREATOR_BASIC_BLENDOFF_TITLE"), true);
			style.fontStyle = previousStyle;

			if (!blendshapeOffsetsFold) 
				return;
			
			foreach (var blendshape in bodyBlendshapes)
			{
				var contains = false;
				foreach (var offset in tempList)
				{
					if (offset.Name != blendshape) 
						continue;
					
					contains = true;
					break;
				}

				if (!contains)
					tempList.Add(new SBlendshapeOffset(blendshape, 0f));
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.Label($"List{itemsCount(bodyBlendshapes.Length)}");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_RESET"), GUILayout.Width(50)))
			{
				tempList.Clear();
				
				foreach (var blendshape in bodyBlendshapes)
					tempList.Add(new SBlendshapeOffset(blendshape, 0f));
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.3f, 0.3f, 0.3f)));

			for (var i = 0; i < tempList.Count; i++)
			{
				var offset = tempList[i];
				tempList[i] = new SBlendshapeOffset(offset.Name, EditorGUILayout.Slider(offset.Name, offset.Offset, -100f, 100f));
			}

			GUILayout.EndVertical();
			
			template.BlendshapeOffsets = tempList.ToArray();
		}
		
		private void physicsSimulation(Template template)
		{
			// todo: localize this stuff
			GUILayout.Space(5);

			var simulation = template.Simulation;
			var simulationDataTempArr = template.Simulation.SimulationData ?? new SimulationData[0];
            var simulationDataTempList = simulationDataTempArr.ToList();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			physicsSimulationFold = EditorGUILayout.Foldout(physicsSimulationFold, GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_TITLE"), true);
			style.fontStyle = previousStyle;

			if (physicsSimulationFold)
			{
                GUILayout.BeginVertical();
                simulation.BoneAssignmentMode = (EBoneAssignmentMode)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BONEALIGNMENTMODE"), simulation.BoneAssignmentMode, "MODCREATOR_BASIC_CLOTHSIM_BONEALIGNMENTMODE_");
				GUILayout.EndVertical();
			}

            if (physicsSimulationFold)
            {
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Entries{itemsCount(simulationDataTempList.Count)}");
                GUILayout.FlexibleSpace();
                
                EditorGUIUtility.labelWidth = 55;
                selectedPhysicsPreset = (EPreset)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_PRESET"), selectedPhysicsPreset, "MODCREATOR_BASIC_CLOTHSIM_PRESET_", GUILayout.Width(200));
                EditorGUIUtility.labelWidth = 0;

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				{
					SimulationData sData = SimulationData.Create(selectedPhysicsPreset);
					sData.Preset = selectedPhysicsPreset;
					simulationDataTempList.Add(sData);
                }
                    
                if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
                    simulationDataTempList.Clear();
                GUILayout.EndHorizontal();

            }

            if (physicsSimulationFold)
			{
				GUILayout.Space(2);
				
				for (var i = 0; i < simulationDataTempList.Count; i++)
				{
					GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.3f, 0.3f, 0.3f)));
					
					var simulationData = simulationDataTempList[i];

					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_GENERAL"), EditorStyles.boldLabel);

                    simulationData.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_NAME"), simulationData.Name);
					
					GUILayout.BeginHorizontal();
                    simulationData.Enabled = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ENABLED"), simulationData.Enabled);
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
                        simulationDataTempList.RemoveAt(i);
						break;
					}
					GUILayout.EndHorizontal();

                    simulationData.AnimationPoseRatio = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANIMPOSERATIO"), simulationData.AnimationPoseRatio, 0f, 1f);
                    
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_COLLISION"), EditorStyles.boldLabel);

                    simulationData.CollisionMode = (ECollisionMode)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_COLLISIONMODE"), simulationData.CollisionMode, "MODCREATOR_BASIC_CLOTHSIM_COLLISIONMODE_");
                    simulationData.Radius = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RADIUS"), simulationData.Radius, 0f, 1f);
                    simulationData.RadiusCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RADIUSCURVE"), simulationData.RadiusCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.Friction = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_FRICTION"), simulationData.Friction, 0f, 1f);

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ADVCOLLISION"), EditorStyles.boldLabel);

                    simulationData.MaxDistanceRadius = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_MAXDISTRADIUS"), simulationData.MaxDistanceRadius);
                    simulationData.MaxDistanceRadiusCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_MAXDISTRADIUSCURVE"), simulationData.MaxDistanceRadiusCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.BackstopNormalAlignment = dragTransformNameSingle(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPNORMALALIGN"), simulationData.BackstopNormalAlignment, Prefabs[Templates.IndexOf(template)]);
                    simulationData.BackstopDistance = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPDIST"), simulationData.BackstopDistance);
                    simulationData.BackstopDistanceCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPDISTCURVE"), simulationData.BackstopDistanceCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.BackstopRadius = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPRADIUS"), simulationData.BackstopRadius);
                    simulationData.BackstopStiffness = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPSTIFFNESS"), simulationData.BackstopStiffness);

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_FORCE"), EditorStyles.boldLabel);

                    simulationData.Gravity = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_GRAVITY"), simulationData.Gravity, 0f, 10f);
                    simulationData.Damping = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_DAMPING"), simulationData.Damping, 0f, 1f);
                    simulationData.DampingCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_DAMPINGCURVE"), simulationData.DampingCurve, Color.green, new Rect(0, 0, 1, 1));

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLE"), EditorStyles.boldLabel);

                    simulationData.Stiffness = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_STIFFNESS"), simulationData.Stiffness, 0f, 1f);
                    simulationData.StiffnessCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_STIFFNESSCURVE"), simulationData.StiffnessCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.VelocityAttenuation = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_VELATTEN"), simulationData.VelocityAttenuation, 0f, 1f);
                    simulationData.AngleLimit = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLIMIT"), simulationData.AngleLimit, 0f, 180f);
                    simulationData.AngleLimitCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLIMITCURVE"), simulationData.AngleLimitCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.AngleLimitStiffness = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLIMITSTIFFNESS"), simulationData.AngleLimitStiffness, 0f, 1f);

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SHAPE"), EditorStyles.boldLabel);

                    simulationData.Rigidness = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RIGIDNESS"), simulationData.Rigidness, 0f, 1f);
                    simulationData.RigidnessCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RIGIDCURVE"), simulationData.RigidnessCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.Tether = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_TETHER"), simulationData.Tether, 0f, 1f);
					
					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_INERTIA"), EditorStyles.boldLabel);

                    simulationData.WorldInertia = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_WORLDINERT"), simulationData.WorldInertia, 0f, 1f);
                    simulationData.LocalInertia = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_LOCALINERT"), simulationData.LocalInertia, 0f, 1f);
					
					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SIMULATION"), EditorStyles.boldLabel);

                    simulationData.SimulationType = (ESimulationType)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SIMULATIONTYPE"), simulationData.SimulationType, "MODCREATOR_BASIC_CLOTHSIM_SIMULATIONTYPE_");

					if (simulationData.SimulationType is ESimulationType.BoneCloth or ESimulationType.MeshCloth)
					{
                        simulationData.SkinningBones ??= Array.Empty<string>();

						GUILayout.Space(2);
						var skinningBonesList = simulationData.SkinningBones.ToList();
						verticalDragTransformNameList(skinningBonesList, GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SKINNINGBONES"), Prefabs[Templates.IndexOf(template)]);
						GUILayout.Space(8);

                        simulationData.SkinningBones = skinningBonesList.ToArray();
					}
					
					if (simulationData.SimulationType is ESimulationType.BoneCloth or ESimulationType.BoneSpring)
					{
                        simulationData.RootBones ??= Array.Empty<string>();

						GUILayout.Space(2);
						var rootBonesList = simulationData.RootBones.ToList();
						verticalDragTransformNameList(rootBonesList, GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ROOTBONES"), Prefabs[Templates.IndexOf(template)]);
						GUILayout.Space(2);

                        simulationData.RootBones = rootBonesList.ToArray();
					}
					
					if (simulationData.SimulationType is ESimulationType.MeshCloth)
					{
                        simulationData.Reduction = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_REDUCTION"), simulationData.Reduction, 0f, 0.2f);
                        simulationData.PaintMap = (Texture2D)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_PAINTMAP"), simulationData.PaintMap, typeof(Texture2D), false);
					}

					if (simulationData.SimulationType == ESimulationType.BoneCloth)
                        simulationData.ConnectionMode = (EConnectionMode)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_CONNECTIONMODE"), simulationData.ConnectionMode, "MODCREATOR_BASIC_CLOTHSIM_CONNECTIONMODE_");

					if (simulationData.SimulationType is ESimulationType.BoneSpring)
					{
                        simulationData.SpringStrength = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGSTR"), simulationData.SpringStrength, 0f, 0.2f);
                        simulationData.SpringDistance = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGDIST"), simulationData.SpringDistance, 0f, 0.5f);
                        simulationData.SpringNormalDistanceRatio = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGNORMALDISTRATIO"), simulationData.SpringNormalDistanceRatio, 0f, 1f);
                        simulationData.SpringNoise = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGNOISE"), simulationData.SpringNoise, 0f, 1f);
					}
					
					GUILayout.EndVertical();
					
					if (i != simulationDataTempList.Count - 1)
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    simulationDataTempList[i] = simulationData;
				}
			}

            simulation.SimulationData = simulationDataTempList.ToArray();
            template.Simulation = simulation;
		}
		
		private void forwardKinematics(Template template)
		{
			GUILayout.Space(5);

			var fkData = template.FKData;
			fkData.Groups ??= Array.Empty<SFKGroup>();
			
			var tempList = fkData.Groups.ToList();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			forwardKinematicsFold = EditorGUILayout.Foldout(forwardKinematicsFold, GetLocalizedString("MODCREATOR_BASIC_FK_TITLE"), true);
			style.fontStyle = previousStyle;

			if (forwardKinematicsFold)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"List{itemsCount(tempList.Count)}");
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
					tempList.Add(SFKGroup.Default());
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
					tempList.Clear();
				GUILayout.EndHorizontal();
			}
			
			if (forwardKinematicsFold)
			{
				GUILayout.Space(2);
				
				for (var i = 0; i < tempList.Count; i++)
				{
					GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.3f, 0.3f, 0.3f)));
					
					var fkGroup = tempList[i];

					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_GENERAL"), EditorStyles.boldLabel);
					
					GUILayout.BeginHorizontal();
					fkGroup.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_FK_GROUPNAME"), fkGroup.Name);
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						tempList.RemoveAt(i);
						break;
					}
					GUILayout.EndHorizontal();

					if (template.TemplateType == ETemplateType.CharacterObject && template.CharacterObjectType == ECharacterObjectType.BaseMesh)
					{
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_CHARASETTINGS"), EditorStyles.boldLabel);
						fkGroup.SupportedGenders = (ESupportedGendersFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_GENDERS")}*", fkGroup.SupportedGenders);
						fkGroup.FutaExclusive = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_FUTAEXCLUSIVE"), fkGroup.FutaExclusive);
					}

					fkGroup.Transforms ??= Array.Empty<SFKTransform>();

					GUILayout.Space(2);
					var transformsList = fkGroup.Transforms.ToList();

					GUILayout.BeginHorizontal();
					GUILayout.Label($"List{itemsCount(transformsList.Count)}");
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
						transformsList.Add(SFKTransform.Default());
					if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
						transformsList.Clear();
					GUILayout.EndHorizontal();

					var previousIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 1;
					
					for (var k = 0; k < transformsList.Count; k++)
					{
						GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.35f, 0.35f, 0.35f)));
						
						var fkTransform = transformsList[k];
						
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_GENERAL"), EditorStyles.boldLabel);
					
						GUILayout.BeginHorizontal();
						fkTransform.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_FK_TRANSFORMNAME"), fkTransform.Name);
						if (GUILayout.Button("-", GUILayout.Width(25)))
						{
							transformsList.RemoveAt(k);
							break;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						
						fkTransform.Transform = (Transform)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_FK_TRANSFORM"), fkTransform.Transform, typeof(Transform), true);

						GUI.enabled = fkTransform.Transform != null;
						
						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_FK_READRESTINGANG"), GUILayout.Width(175)))
							fkTransform.RestingAngle = fkTransform.Transform.localRotation;
						
						GUI.enabled = true;
						
						GUILayout.EndHorizontal();
						
						var angVec = new Vector4
						{
							x = fkTransform.RestingAngle.x,
							y = fkTransform.RestingAngle.y,
							z = fkTransform.RestingAngle.z,
							w = fkTransform.RestingAngle.w
						};
						
						angVec = EditorGUILayout.Vector4Field(GetLocalizedString("MODCREATOR_BASIC_FK_RESTINGANGLE"), angVec);
						fkTransform.RestingAngle = new Quaternion(angVec.x, angVec.y, angVec.z, angVec.w);

						fkTransform.MovableTargetScale = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_FK_MOVABLETARGSCA"), fkTransform.MovableTargetScale, 0.1f, 2f);
						
						GUILayout.Space(10);
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORING"), EditorStyles.boldLabel);

						fkTransform.MirrorTransform = (Transform)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORTRANSFORM"), fkTransform.MirrorTransform, typeof(Transform), true);
						
						GUILayout.BeginHorizontal();
						fkTransform.MirrorInvertAxis.Item1 = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORX"), fkTransform.MirrorInvertAxis.Item1);
						fkTransform.MirrorInvertAxis.Item2 = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORY"), fkTransform.MirrorInvertAxis.Item2);
						fkTransform.MirrorInvertAxis.Item3 = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORZ"), fkTransform.MirrorInvertAxis.Item3);
						GUILayout.EndHorizontal();
						
						GUILayout.EndVertical();
					
						if (k != transformsList.Count - 1)
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						
						transformsList[k] = fkTransform;
					}
					
					EditorGUI.indentLevel = previousIndent;

					GUILayout.Space(8);

					fkGroup.Transforms = transformsList.ToArray();
					
					GUILayout.EndVertical();
					
					if (i != tempList.Count - 1)
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					
					tempList[i] = fkGroup;
				}
			}

			fkData.Groups = tempList.ToArray();
			template.FKData = fkData;
		}

		private void clippingFix(Template template)
		{
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLIPPINGFIX"), EditorStyles.boldLabel);

			var previousClippingFix = template.UseClippingFix;
			var previousClippingDistance = template.ClippingDistance;
			var previousBodyRaysResolution = BodyRaysResolution;
			
			template.UseClippingFix = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_USECLIPPINGFIX"), template.UseClippingFix);
			EditorGUIUtility.fieldWidth = 60;
			template.ClippingDistance = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLIPPINGDISTANCE"), template.ClippingDistance, 0.00001f, 0.01f);
			EditorGUIUtility.fieldWidth = 0;
			
			GUILayout.BeginHorizontal();
			HidePreviewRenderers = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_HIDEPREVIEWRENDERERS"), HidePreviewRenderers);
			bodyRaysResolution = (ERaysResolution)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_RAYSRESOLUTION")}*", bodyRaysResolution, "MODCREATOR_BASIC_RAYSRESOLUTION_");
			GUILayout.EndHorizontal();
			
			GUILayout.Space(2);
			
			GUILayout.BeginHorizontal();
			for (var i = 0; i < template.ClothingStates.Length; i++)
			{
				var stateEnum = (EClothingState)i;
				if (stateEnum == EClothingState.Off)
					continue;

				var previewingThis = template == previewingTemplate && stateEnum == previewingState;

				GUI.enabled = template.ClothingStates[i] != null;
				var startPreviewState = GUILayout.Button(GetLocalizedString($"MODCREATOR_BASIC_PREVIEW_STATE_{stateEnum.ToString().ToUpper()}") + (previewingThis ? "*" : ""));
				GUI.enabled = true;

				if (!startPreviewState)
					continue;

				var clothingTransform = ((GameObject)Prefabs[Templates.IndexOf(template)]).transform;
				startPreview(template, stateEnum, clothingTransform);
			}

			GUILayout.Space(5);

			GUI.enabled = previewingTemplate != null;
			var stopPreviewState = GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_PREVIEW_STOP"), GUILayout.Width(125));
			GUI.enabled = true;

			if (stopPreviewState)
				stopPreview();
			
			GUILayout.EndHorizontal();

			if (template.UseClippingFix != previousClippingFix || template.ClippingDistance != previousClippingDistance || BodyRaysResolution != previousBodyRaysResolution)
				refreshPreview(previousClippingDistance, previousBodyRaysResolution);
		}
		
		private string dragTransformNameSingle(string label, string transformName, Object parentObject)
		{
			if (parentObject == null || parentObject is not GameObject parent)
				return "";
			
			var trs = parent.GetComponentsInChildren<Transform>();
			Transform tr = null;

			foreach (var child in trs)
			{
				if (child.name != transformName)
					continue;

				tr = child;
				break;
			}
			
			var objField = (Transform)EditorGUILayout.ObjectField(label, tr, typeof(Transform), true);
			return objField != null ? objField.name : "";
		}
		
		private void verticalDragTransformNameList(List<string> list, string labelTitle, Object parentObject)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add("");
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			if (parentObject == null || parentObject is not GameObject parent)
				return;

			var trs = parent.GetComponentsInChildren<Transform>();
			
			for (var i = 0; i < list.Count; i++)
			{
				Transform tr = null;

				foreach (var child in trs)
				{
					if (child.name != list[i])
						continue;

					tr = child;
					break;
				}
				
				GUILayout.BeginHorizontal();
				
				var objField = (Transform)EditorGUILayout.ObjectField("", tr, typeof(Transform), true);
				if (objField != null)
					list[i] = objField.name;
				else
					list[i] = "";
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}

		private void focusSelectedObject()
		{
			var sceneView = SceneView.lastActiveSceneView;
				
			var focusedField = sceneView.GetType().GetField("m_WasFocused", BindingFlags.NonPublic | BindingFlags.Instance);
			focusedField.SetValue(sceneView, false);
			
			sceneView.FrameSelected(false, true);
		}

		private void startPreview(Template template, EClothingState state, Transform clothingTransform)
		{
			Debug.Log("Entering clothing preview mode");

			// hide all states except the one we need so we can grab the appropriate renderers when cloning
			for (var i = 0; i < template.ClothingStates.Length; i++)
				if (template.ClothingStates[i] != null)
					template.ClothingStates[i].gameObject.SetActive(state == (EClothingState)i);
			
			// get a copy of the clothing so it can be used as a preview
			var clone = Instantiate(clothingTransform);
			
			// get only renderers on active objects (the correct state)
			var renderers = clone.GetComponentsInChildren<SkinnedMeshRenderer>(false);

			// re-enable all states again
			foreach (var stateObject in template.ClothingStates)
				if (stateObject != null)
					stateObject.gameObject.SetActive(true);
			
			previewingClothingClone = new Tuple<Transform, SkinnedMeshRenderer[]>(clone, renderers);
			previewingTemplate = template;
			previewingState = state;
			
			// hide all original objects
			foreach (var prefab in Prefabs)
			{
				if (prefab == null || prefab is not GameObject gameObject)
					continue;
				
				gameObject.SetActive(false);
			}
			
			PreviewingBodyMaterial.SetTexture("_AlphaMap", null);
			PreviewingBody.SetActive(true);

			/*// remap preview clothing bones to body bones (todo: handle lods)
			foreach (var skinnedMeshRenderer in previewingClothingClone.Item2)
			{
				var currentBones = skinnedMeshRenderer.bones;
				var newBones = new Transform[currentBones.Length];
				for (var i = 0; i < currentBones.Length; i++)
				{
					var bone = currentBones[i].gameObject;
					previewingBodyBones.TryGetValue(bone.name, out newBones[i]);

					if (newBones[i] == null)
						newBones[i] = bone.transform;
				}
				skinnedMeshRenderer.bones = newBones;
			}
			
			// bake preview clothing meshes to adjust to new bones
			foreach (var skinnedMeshRenderer in previewingClothingClone.Item2)
			{
				var transform = skinnedMeshRenderer.transform;
				
				var previousPosition = transform.localPosition;
				var previousAngles = transform.localEulerAngles;
				
				transform.localPosition = Vector3.zero;
				transform.localEulerAngles = Vector3.zero;

				var newMesh = new Mesh();
				skinnedMeshRenderer.BakeMesh(newMesh, true);
				skinnedMeshRenderer.sharedMesh = newMesh;

				transform.localPosition = previousPosition;
				transform.localEulerAngles = previousAngles;
			}*/
			
			Selection.activeGameObject = clone.gameObject;
			focusSelectedObject();
			
			HidePreviewRenderers = !HidePreviewRenderers;
			HidePreviewRenderers = !HidePreviewRenderers;
			
			clippingFixShader = Resources.Load<ComputeShader>("Compute/TransparencyBaker");
			clippingFixKernel = clippingFixShader.FindKernel("CastRaysBVH");

			dilaterShader = Resources.Load<ComputeShader>("Compute/DilateTexture");
			dilaterKernel = dilaterShader.FindKernel("DilateTexture");
			
			bvhNodes = null;
			triangles = null;
			
			// reset textures
			PreviewingClippingFixTexture?.Release();
			previewingClippingFixTexture = null;
			
			bakerTempRT?.Release();
			bakerTempRT = null;
			
			refreshPreview(-1, BodyRaysResolution);
		}

		// PreviewingBodyMaterial - material of the test body
		// PreviewingBody - gameobject of the test body
		// previewingState - clothing state being previewed
		// previewingClothingRenderers - all renderers that belong to the clothing state being previewed
		// BodyRaysResolution - selected resolution of body rays (make sure to use the property, not the field)
		// BodyRays - dictionary of resolution->ray[] (make sure to use the property, not the field)

		private void refreshPreview(float previousClippingDistance, ERaysResolution previousBodyRaysResolution)
		{
			if (previewingTemplate == null)
				return;

			if (!HidePreviewRenderers)
			{
				previewingClothingClone.Item1.gameObject.SetActive(false);

				if (previewTimer == null)
				{
					previewTimer = new Timer();
					previewTimer.Interval = 500;
					previewTimer.AutoReset = false;
					previewTimer.Enabled = true;
					previewTimer.Elapsed += delegate { context.Send(delegate { timerElapsed(); }, null); };
				}

				previewTimer.Stop();
				previewTimer.Start();
			}
			
			// PreviewingBodyMaterial - material of the test body
			// PreviewingBody - gameobject of the test body
			// previewingState - clothing state being previewed
			// previewingClothingRenderers - all renderers that belong to the clothing state being previewed
			
			// if it's the first time we're baking, we need to build the BVH
			if (bvhNodes == null || triangles == null)
			{
				Raycaster raycaster = new();

				foreach (var renderer in previewingClothingClone.Item2)
					raycaster.AddMesh(renderer.sharedMesh, renderer.transform);

				raycaster.BuildBVH();

				(bvhNodes, triangles) = raycaster.BvhRoot.ToGPU();
			}
			
			// if resolution is changed, we need to update the rays buffer and the textures
			if(BodyRaysResolution != previousBodyRaysResolution || raysBuffer == null)
			{
				// the getter will automatically create the texture when needed if it doesn't exist

				PreviewingClippingFixTexture?.Release();
				previewingClippingFixTexture = null;
				
				bakerTempRT?.Release();
				bakerTempRT = null;
				
				var rays = BodyRays[BodyRaysResolution];
				
				raysBuffer = new ComputeBuffer(rays.Length, Ray.Size);
				raysBuffer.SetData(rays);
				
				clippingFixShader.SetBuffer(clippingFixKernel, "Rays", raysBuffer);
			}
			
			var bvhNodesBuffer = new ComputeBuffer(bvhNodes.Length, BVHNodeGPU.Size);
			bvhNodesBuffer.SetData(bvhNodes);
			
			var trianglesBuffer = new ComputeBuffer(triangles.Length, Triangle.Size);
			trianglesBuffer.SetData(triangles);
			
			// we only have one bvh root, so the only offset is 0
			var bvhNodesOffsets = new ComputeBuffer(1, sizeof(int));
			bvhNodesOffsets.SetData(new[] { 0 });
			
			// same as above, we only have one clothing piece so one distance
			var raycastDistancesBuffer = new ComputeBuffer(1, sizeof(float));
			raycastDistancesBuffer.SetData(new[] { previewingTemplate.ClippingDistance });

			if (bakerTempRT == null)
			{
				bakerTempRT = new CustomRenderTexture((int)BodyRaysResolution, (int)BodyRaysResolution, RenderTextureFormat.R8);
				bakerTempRT.doubleBuffered = true;
				bakerTempRT.enableRandomWrite = true;
				bakerTempRT.material = new Material(Shader.Find("White"));
				bakerTempRT.Create();
				bakerTempRT.Initialize();
				bakerTempRT.Update();
			}

			clippingFixShader.SetTexture(clippingFixKernel, "Hitmap", bakerTempRT);
			clippingFixShader.SetBuffer(clippingFixKernel, "Rays", raysBuffer);
			clippingFixShader.SetBuffer(clippingFixKernel, "BVHNodes", bvhNodesBuffer);
			clippingFixShader.SetBuffer(clippingFixKernel, "Triangles", trianglesBuffer);
			clippingFixShader.SetBuffer(clippingFixKernel, "NodesOffsets", bvhNodesOffsets);
			clippingFixShader.SetBuffer(clippingFixKernel, "RaycastDistances", raycastDistancesBuffer);
            
			var fence = Graphics.CreateAsyncGraphicsFence();
			clippingFixShader.Dispatch(clippingFixKernel, bakerTempRT.width / 32, bakerTempRT.height / 32, 1);
			Graphics.WaitOnAsyncGraphicsFence(fence);

			// then dilate the texture
			// TODO: set the dilation radius as a parameter
			dilaterShader.SetInt("kernelSize", 6);
			dilaterShader.SetTexture(dilaterKernel, "original", bakerTempRT);
			dilaterShader.SetTexture(dilaterKernel, "result", PreviewingClippingFixTexture);
            
			fence = Graphics.CreateAsyncGraphicsFence();
			dilaterShader.Dispatch(dilaterKernel, PreviewingClippingFixTexture.width / 32, PreviewingClippingFixTexture.height / 32, 1);
			Graphics.WaitOnAsyncGraphicsFence(fence);
			
			// cleanup
			bvhNodesBuffer.Release();
			trianglesBuffer.Release();
			bvhNodesOffsets.Release();
			raycastDistancesBuffer.Release();
			
			SceneView.RepaintAll();
		}
		
		private void stopPreview()
		{
			if (previewingTemplate == null)
				return;

			Debug.Log("Exiting clothing preview mode");

			foreach (var prefab in Prefabs)
			{
				if (prefab == null || prefab is not GameObject gameObject)
					continue;
				
				gameObject.SetActive(true);
			}

			previewTimer?.Stop();
			
			bvhNodes = null;
			triangles = null;
			
			raysBuffer?.Release();
			raysBuffer = null;
			
			bakerTempRT?.Release();
			bakerTempRT = null;

			previewingClippingFixTexture?.Release();
			previewingClippingFixTexture = null;

			DestroyImmediate(previewingClothingClone.Item1.gameObject);
			PreviewingBody.SetActive(false);

			Selection.activeGameObject = null;
			focusSelectedObject();

			previewingTemplate = null;
		}

		private void timerElapsed()
		{
			if (HidePreviewRenderers)
				return;

			previewingClothingClone.Item1.gameObject.SetActive(true);
		}
	}
}
