//#define VERBOSE
#define MODCREATOR_STANDALONE

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Code.Components;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.CharacterObjects;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.ModdedScenes;
using Code.Frameworks.ModdedScenes.Flags;
using Code.Frameworks.Studio.Enums;
using Code.Frameworks.Studio.Interfaces;
using Code.Frameworks.Studio.StudioObjects;
using Code.Managers;
using Code.Tools;
using Microsoft.CSharp;
using Packages.SFB;
using Railgun.AssetPipeline.Enums;
using Railgun.AssetPipeline.Interfaces;
using Railgun.AssetPipeline.Models;
using Railgun.AssetPipeline.Services;
using Railgun.AssetPipeline.Types;
using Railgun.ModEngine.Providers;
using Railgun.ModEngine.Services;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using Tuple = Code.Tools.Tuple;
using Vector4 = UnityEngine.Vector4;

namespace Code.Editor.ModEngine
{
    public class ModCreator : EditorWindow
	{
		private int currentTab = 1;
		public int CurrentTab
		{
			get => currentTab;
			set
			{
				var oldValue = currentTab;
				currentTab = value;
				
				if (oldValue != currentTab)
				{
					EditorGUI.FocusTextInControl(null);

					if (currentTab == 0)
					{
						SetupPresets();
					}
				}
			}
		}
		 
		private int currentTemplate = -1;
		public int CurrentTemplate
		{
			get => currentTemplate;
			set
			{
				currentTemplate = value;
			}
		}

#if MODCREATOR_STANDALONE
		public const string ModCreatorPath = "Assets/Mod Creator/";
#else
		public const string ModCreatorPath = "Assets/";
#endif

		public const string Version = "v0.1.4.0";
		
		[SerializeField]
		public Manifest Manifest = new () {Name = "", Author = "", Version = "1.0.0"};
		
		[SerializeField]
		public List<Object> Prefabs = new ();
		
		[SerializeField]
		public List<Template> Templates = new ();

		[SerializeField]
		public List<string> Presets = new ();

		[SerializeField]
		public Tuple.SerializableTuple<string, byte[]> Assembly;

		[SerializeField]
		public string SavePresetName = "Preset Name";

		[SerializeField]
		public bool[] BasicFolds = new bool[3];

		[SerializeField]
		public bool DebugMode;

		[SerializeField]
		public int Language;

		[SerializeField] 
		public string[] AvailableLanguages = {"English"};

		[SerializeField]
		public ECompression DebugCompressionType = ECompression.None;
		
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
		
		private UnityEngine.Vector2 presetsScrollPosition;
		private UnityEngine.Vector2 builderScrollPosition;
		private UnityEngine.Vector2 basicScrollPosition;
		private UnityEngine.Vector2 advancedScrollPosition;

		private bool physicsSimulationFold;
		private bool forwardKinematicsFold;
		
		private Template copyBuffer;
		private Assembly tempAssembly;
		private EPreset selectedPreset;

		private Regex lodRegex = new ("^LOD\\d$");
		private const string basicFilter = @"(^[0-9])|([^a-zA-Z0-9_])";
		private const string uniqueIDFormat = "_#";

		// todo: figure out a better solution. Maybe allow the user to pick for which base mesh they are making the item and then populate this list accordingly.
		private readonly string[] defaultParents =
		{
			// enpi
			//"DEF_hips.001","DEF_buttocks.L.001","DEF_buttocks.L.001_end","DEF_buttocks.R.001","DEF_buttocks.R.001_end","DEF_hips.002","DEF_hips.002_end","DEF_hips.L.001","DEF_thigh.L.001","DEF_thigh.L.002","DEF_shin.L.001","DEF_feet.L.001","DEF_feet.L.003","DEF_feet.L.003_end","DEF_Toe_master.L.001","DEF_index_toe.L.002","DEF_index_toe.L.003","DEF_index_toe.L.003_end","DEF_middle_toe.L.002","DEF_middle_toe.L.003","DEF_middle_toe.L.003_end","DEF_pinky_toe.L.002","DEF_pinky_toe.L.002_end","DEF_ring_toe.L.002","DEF_ring_toe.L.003","DEF_ring_toe.L.003_end","DEF_thumb_toe.L.002","DEF_thumb_toe.L.002_end","DEF_hips.R.001","DEF_thigh.R.001","DEF_thigh.R.002","DEF_shin.R.001","DEF_feet.R.001","DEF_feet.R.003","DEF_feet.R.003_end","DEF_Toe_master.R.001","DEF_index_toe.R.002","DEF_index_toe.R.003","DEF_index_toe.R.003_end","DEF_middle_toe.R.002","DEF_middle_toe.R.003","DEF_middle_toe.R.003_end","DEF_pinky_toe.R.002","DEF_pinky_toe.R.002_end","DEF_ring_toe.R.002","DEF_ring_toe.R.003","DEF_ring_toe.R.003_end","DEF_thumb_toe.R.002","DEF_thumb_toe.R.002_end","DEF_penis.001","DEF_penis.002","DEF_penis.003","DEF_penis.003_end","DEF_spine.001","DEF_spine.002","DEF_spine.003","DEF_breast.L.001","DEF_breast.L.002","DEF_breast.L.003","DEF_breast.L.003_end","DEF_breast.R.001","DEF_breast.R.002","DEF_breast.R.003","DEF_breast.R.003_end","DEF_neck.001","DEF_head.001","DEF_Ear_L.001","DEF_Ear_L.002","DEF_Ear_L.003","DEF_Ear_L.004","DEF_Ear_L.005","DEF_Ear_L.005_end","DEF_Ear_R.001","DEF_Ear_R.002","DEF_Ear_R.003","DEF_Ear_R.004","DEF_Ear_R.005","DEF_Ear_R.005_end","DEF_eye_L.001","DEF_eye_L.001_end","DEF_eye_R.001","DEF_eye_R.001_end","DEF_Jaw.001","DEF_Jaw_L.002","DEF_Jaw_L.003","DEF_Jaw_L.004","DEF_Jaw_L.004_end","DEF_Jaw_R.002","DEF_Jaw_R.003","DEF_Jaw_R.004","DEF_Jaw_R.004_end","DEF_LipMouthBottom.001","DEF_LipMouthBottom.001_end","DEF_tongue.001","DEF_tongue.002","DEF_tongue.003","DEF_tongue.003_end","DEF_Nose.001","DEF_Nose.001_end","DEF_Scalp_L.001","DEF_Scalp_L.002","DEF_Scalp_L.003","DEF_Scalp_L.004","DEF_Scalp_L.004_end","DEF_Scalp_R.001","DEF_Scalp_R.002","DEF_Scalp_R.003","DEF_Scalp_R.004","DEF_Scalp_R.004_end","DEF_UpperCheek_L.001","DEF_UpperCheek_L.002","DEF_UpperCheek_L.003","DEF_UpperCheek_L.003_end","DEF_UpperCheek_R.001","DEF_UpperCheek_R.002","DEF_UpperCheek_R.003","DEF_UpperCheek_R.003_end","DEF_LipMouthUpper.001","DEF_LipMouthUpper.001_end","DEF_shoulder.L.001","DEF_biceps.L.001","DEF_biceps.L.002","DEF_forearm.L.001","DEF_hand.L.001","DEF_index.L.002","DEF_index.L.003","DEF_index.L.004","DEF_index.L.004_end","DEF_middle.L.002","DEF_middle.L.003","DEF_middle.L.004","DEF_middle.L.004_end","DEF_pinky.L.002","DEF_pinky.L.003","DEF_pinky.L.004","DEF_pinky.L.004_end","DEF_ring.L.002","DEF_ring.L.003","DEF_ring.L.004","DEF_ring.L.004_end","DEF_thumb.L.001","DEF_thumb.L.002","DEF_thumb.L.003","DEF_thumb.L.003_end","DEF_shoulder.R.001","DEF_biceps.R.001","DEF_biceps.R.002","DEF_forearm.R.001","DEF_hand.R.001","DEF_index.R.002","DEF_index.R.003","DEF_index.R.004","DEF_index.R.004_end","DEF_middle.R.002","DEF_middle.R.003","DEF_middle.R.004","DEF_middle.R.004_end","DEF_pinky.R.002","DEF_pinky.R.003","DEF_pinky.R.004","DEF_pinky.R.004_end","DEF_ring.R.002","DEF_ring.R.003","DEF_ring.R.004","DEF_ring.R.004_end","DEF_thumb.R.001","DEF_thumb.R.002","DEF_thumb.R.003","DEF_thumb.R.003_end"
			// rappy
			"Armature","Hips","Buttocks.L.001","Buttocks.R.001","Penis.000","Penis.001","Penis.002","Spine","Chest","UpperChest","Breast.L.001","Breast.L","Breast.R.001","Breast.R","Neck","Head","Cheak.L","Cheak_shape.L","Cheak.R","Cheak_shape.R","Chin.L","Chin.R","Ctrl.lip.corner.L","Ctrl.lip.corner.R","Ctrl.lip.lower","lip.lower.L","lip.lower.R","Ctrl.lip.lower.L","lip.lower.L.001","Ctrl.lip.lower.R","lip.lower.R.001","Ctrl.lip.upper","lip.upper.L","lip.upper.R","Ctrl.lip.upper.L","lip.upper.L.001","Ctrl.lip.upper.R","lip.upper.R.001","ctrl.tongue.000","ctrl.tongue.001","ctrl.tongue.002","ear rings.L","ear rings.R","Eye.L","Eye.R","Jaw_shape.L","Jaw.L","Jaw_shape.R","Jaw.R","teeth.lower","teeth.upper","tongue.000","tongue.001","tongue.002","Face Helper","Shoulder.L","UpperArm.L","LowerArm.L","Hand.L","palm.01.L","f_index.01.L","f_index.02.L","f_index.03.L","thumb.01.L","thumb.02.L","thumb.03.L","palm.02.L","f_middle.01.L","f_middle.02.L","f_middle.03.L","palm.03.L","f_ring.01.L","f_ring.02.L","f_ring.03.L","palm.04.L","f_pinky.01.L","f_pinky.02.L","f_pinky.03.L","Shoulder.R","UpperArm.R","LowerArm.R","Hand.R","palm.01.R","f_index.01.R","f_index.02.R","f_index.03.R","thumb.01.R","thumb.02.R","thumb.03.R","palm.02.R","f_middle.01.R","f_middle.02.R","f_middle.03.R","palm.03.R","f_ring.01.R","f_ring.02.R","f_ring.03.R","palm.04.R","f_pinky.01.R","f_pinky.02.R","f_pinky.03.R","UpperLeg.L","LowerLeg.L","Foot.L","Toes.L","Toes.Index.L.001","Toes.Index.L.002","Toes.Middle.L.001","Toes.Middle.L.002","Toes.Pinky.L.001","Toes.Ring.L.001","Toes.Ring.L.002","Toes.Thumb.L.001","Toes.Thumb.L.002","UpperLeg.L.C","UpperLeg.R","LowerLeg.R","Foot.R","Toes.R","Toes.Index.R.001","Toes.Index.R.002","Toes.Middle.R.001","Toes.Middle.R.002","Toes.Pinky.R.001","Toes.Ring.R.001","Toes.Ring.R.002","Toes.Thumb.R.001","Toes.Thumb.R.002","UpperLeg.R.C","Waist.L","Waist.R","SFWCollider"
		};

		[MenuItem("Mod Engine/Mod Creator")]
		public static void Initialize()
		{
            var window = GetWindow(typeof(ModCreator), true, $"Mod Creator {Version}");
			window.minSize = new UnityEngine.Vector2(700, 500);
			window.Show();

			LocalizationManager.Instance.LoadAllLanguages();

			var modCreator = (ModCreator)window;
			modCreator.AvailableLanguages = LocalizationManager.Instance.GetAvailableLanguages().ToArray();
			modCreator.SetLanguage("English");
		}

		public void OnDestroy()
		{
			SavePreset("_previous_state_", true);
		}

		public void OnGUI()
		{
			if (Templates.Count == 0 || CurrentTemplate == -1)
			{
				CurrentTab = GUILayout.Toolbar(CurrentTab, new[]
				{
					GetLocalizedString("MODCREATOR_TABS_PRESETLANGUAGE"),
					GetLocalizedString("MODCREATOR_TABS_BUILDER")
				});
			}
			else if (CurrentTemplate != -1 && Templates.Count > CurrentTemplate)
			{
				CurrentTab = GUILayout.Toolbar(CurrentTab, Templates[CurrentTemplate].Advanced
					? new[]
					{
						GetLocalizedString("MODCREATOR_TABS_PRESETLANGUAGE"),
						GetLocalizedString("MODCREATOR_TABS_BUILDER"),
						GetLocalizedString("MODCREATOR_TABS_BASIC"),
						GetLocalizedString("MODCREATOR_TABS_ADVANCED")
					}
					: new[]
					{
						GetLocalizedString("MODCREATOR_TABS_PRESETLANGUAGE"),
						GetLocalizedString("MODCREATOR_TABS_BUILDER"),
						GetLocalizedString("MODCREATOR_TABS_BASIC")
					});
			}

			switch (CurrentTab)
			{
				case 0:
				{
					GUILayout.Space(2);
					
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_LANGUAGES"), EditorStyles.boldLabel);

					var prevLanguage = Language;
					Language = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_CURRENTLANG")}*", Language, AvailableLanguages);

					if (prevLanguage != Language)
						SetLanguage(AvailableLanguages[Language]);
					
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_SAVE"), EditorStyles.boldLabel);
					
					GUILayout.BeginHorizontal();
					
					SavePresetName = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_NAME")}*", SavePresetName);
					
					if (GUILayout.Button(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_SAVEPRESET"), GUILayout.Width(125)))
						SavePreset(SavePresetName);
					
					GUILayout.EndHorizontal();
					
					// todo: figure out how to deal with this
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_RUNTIMEDATA"));
					
					GUILayout.Space(10);
					
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_LOAD"), EditorStyles.boldLabel);
					
					presetsScrollPosition = GUILayout.BeginScrollView(presetsScrollPosition);

					if (Presets.Count > 0)
					{
						GUILayout.BeginVertical();
						verticalPresetList(Presets);
						GUILayout.EndVertical();
					}
					else
					{
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_NOPRESETS"), EditorStyles.boldLabel);
					}

					GUILayout.EndScrollView();
					
					break;
				}
				case 1:
				{
					builderScrollPosition = GUILayout.BeginScrollView(builderScrollPosition);

					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_MODINFO"), EditorStyles.boldLabel);
					Manifest.Name = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BUILDER_NAME")}*", Manifest.Name);
					if (Manifest.Name != null)
						Manifest.Name = Regex.Replace(Manifest.Name, basicFilter, "");
					
					Manifest.Description = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BUILDER_DESC"), Manifest.Description);

					Manifest.Author = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BUILDER_AUTHOR")}*", Manifest.Author);
					if (Manifest.Author != null)
						Manifest.Author = Regex.Replace(Manifest.Author, basicFilter, "");
					
					Manifest.Version = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BUILDER_VERSION")}*", Manifest.Version);

					GUILayout.Space(10);

					{
						var list = Manifest.CompatibleVersions != null ? Manifest.CompatibleVersions.ToList() : new List<string>();
						verticalList(list, GetLocalizedString("MODCREATOR_BUILDER_COMPATVER"));
						Manifest.CompatibleVersions = list.ToArray();
					}
					
					GUILayout.Space(10);

					{
						var list = Manifest.Dependencies != null ? Manifest.Dependencies.ToList() : new List<string>();
						verticalList(list, GetLocalizedString("MODCREATOR_BUILDER_DEPS"));
						Manifest.Dependencies = list.ToArray();
					}
					
					GUILayout.Space(10);
					
					BasicFolds[0] = EditorGUILayout.Foldout(BasicFolds[0], GetLocalizedString("MODCREATOR_BUILDER_CHARAOBJ"), true);
					if (BasicFolds[0])
					{
						GUILayout.BeginVertical();
						verticalList(Prefabs, ETemplateType.CharacterObject,GetLocalizedString("MODCREATOR_BUILDER_LIST"));
						GUILayout.EndVertical();
					}
					
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					
					BasicFolds[1] = EditorGUILayout.Foldout(BasicFolds[1], GetLocalizedString("MODCREATOR_BUILDER_STUDIOOBJ"), true);
					if (BasicFolds[1])
					{
						GUILayout.BeginVertical();
						verticalList(Prefabs, ETemplateType.StudioObject, GetLocalizedString("MODCREATOR_BUILDER_LIST"));
						GUILayout.EndVertical();

					}
					
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					
					BasicFolds[2] = EditorGUILayout.Foldout(BasicFolds[2], GetLocalizedString("MODCREATOR_BUILDER_MODSCENE"), true);
					if (BasicFolds[2])
					{
						GUILayout.BeginVertical();
						verticalList(Prefabs, ETemplateType.ModdedScene, GetLocalizedString("MODCREATOR_BUILDER_LIST"));
						GUILayout.EndVertical();
					}
					
					GUILayout.Space(10);
					
					if (!string.IsNullOrEmpty(Assembly.Item1))
					{
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_ASMINFO"), EditorStyles.boldLabel);
						
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_NAME"), Assembly.Item1, "TextField");
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_SIZE"), Assembly.Item2.Length / 1024 + " kB", "TextField");
					}

					GUILayout.EndScrollView();
					
					GUILayout.FlexibleSpace();

					DebugMode = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BUILDER_DEBUG"), DebugMode);

					if (DebugMode)
					{
						GUILayout.BeginHorizontal();

						DebugCompressionType = (ECompression)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BUILDER_COMPTYPE"), DebugCompressionType, "MODCREATOR_BUILDER_COMPTYPE_", GUILayout.Width(275));
                        
						var actionText = DebugCompressionType == ECompression.None ? GetLocalizedString("MODCREATOR_BUILDER_DECOMPRESS") : $"{GetLocalizedString("MODCREATOR_BUILDER_COMPRESS")} ({(DebugCompressionType == ECompression.NullValue ? GetLocalizedString("MODCREATOR_BUILDER_DEFAULT") : DebugCompressionType)})";
						if (GUILayout.Button(actionText, GUILayout.Width(175)))
						{
							StandaloneFileBrowser.OpenFilePanelAsync(GetLocalizedString("MODCREATOR_BUILDER_LOAD"), "", "rgm", false,delegate(string[] files)
							{
								if (files.Length == 0 || files[0] == "")
									return;
                                
								var affService = Railgun.AssetPipeline.Providers.ServiceProvider.Obtain<IAFFService>();
								if (affService.TryCompressModFile(files[0], out var decompressedPath, DebugCompressionType))
									Debug.Log($"Sucessfully {(DebugCompressionType == ECompression.None ? "Decompress" : "Compress")}ed mod to {decompressedPath}");
								else
									Debug.LogError($"Failed to {(DebugCompressionType == ECompression.None ? "Decompress" : "Compress")} mod {files[0]}");
							});
						}

						GUILayout.FlexibleSpace();

						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_LOAD"), GUILayout.Width(125)))
							Load();
						
						GUILayout.EndHorizontal();
					}
					else
					{
						GUILayout.BeginHorizontal();

						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_ASSIGN"), GUILayout.Width(175)))
							AssignComponents();

						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_BUILD"), GUILayout.Width(125)))
							Build();
                        
						GUILayout.FlexibleSpace();

						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_RESET"), GUILayout.Width(75)))
						{
							Manifest = new Manifest { Name = "", Author = "", Version = "1.0.0" };

							Prefabs.Clear();
							Templates.Clear();

							Assembly = new Tuple.SerializableTuple<string, byte[]>(null, null);

							copyBuffer = null;
							tempAssembly = null;
						}

						GUILayout.EndHorizontal();
					}

					break;
				}
				case 2:
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
								else if (template.TextureType is ETextureType.FaceOverlay or ETextureType.Lips)
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
					
					if (template.TemplateType is not ETemplateType.ModdedScene)
						template.Advanced = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_ADVEDIT"), template.Advanced);
					
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

					break;
				}
				case 3:
				{
					if (Templates == null || Templates.Count == 0)
						return;
					
					var template = Templates[CurrentTemplate];

					GUILayout.Space(2);
					
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_ADV_CODEREL"), EditorStyles.boldLabel);
					template.Type = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_ADV_CUSTOMTYPE")}*", template.Type);
					template.Type = Regex.Replace(template.Type, basicFilter, "");
					
					GUILayout.Space(10);

					advancedScrollPosition = GUILayout.BeginScrollView(advancedScrollPosition);
					
					template.Usings = verticalList(template.Usings, GetLocalizedString("MODCREATOR_ADV_USINGS"));
					
					GUILayout.Space(10);
					
					EditorGUILayout.LabelField($"{GetLocalizedString("MODCREATOR_ADV_SOURCE")}*", EditorStyles.boldLabel);
					template.Source = EditorGUILayout.TextArea(template.Source, GUILayout.ExpandHeight(true));

					GUILayout.EndScrollView();
					
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_ADV_WARN1"));
					EditorGUILayout.LabelField(template.TemplateType == ETemplateType.CharacterObject
						? GetLocalizedString("MODCREATOR_ADV_WARN2")
						: GetLocalizedString("MODCREATOR_ADV_WARN3"));
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_ADV_WARN4"));
					
					GUILayout.BeginHorizontal();

					if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADV_IMPORT")))
					{
						StandaloneFileBrowser.OpenFilePanelAsync(GetLocalizedString("MODCREATOR_ADV_IMPORTSRC"), "", "cs", false, delegate(string[] files)
						{
							if (files.Length == 0 || files[0] == "")
								return;

							if (FileTools.ReadAllTextSafe(files[0], out var source))
							{
								template.Source = source;
								
								Debug.Log("Source Code imported from " + files[0]);

								var from = template.Source.IndexOf(" class ", StringComparison.InvariantCulture);
								var to = template.Source.IndexOf(" : Base", StringComparison.InvariantCulture);

								if (from == -1 || to == -1)
								{
									Debug.LogError("No type found in imported file");
									return;
								}

								from += 7;

								template.Type = template.Source.Substring(from, to - from);
							}
							else
							{
								Debug.LogError($"Failed to read source code from {files[0]}");
							}
						});
					}

					if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADV_EXPORT")))
					{
						StandaloneFileBrowser.SaveFilePanelAsync(GetLocalizedString("MODCREATOR_ADV_EXPORTSRC"), "", template.Type + ".cs", "cs", delegate(string file)
						{
							if (string.IsNullOrEmpty(file))
								return;

							if (FileTools.WriteAllTextSafe(file, template.Source))
								Debug.Log("Source Code exported to " + file);
							else
								Debug.LogError("Failed writing Source Code to " + file);
						});
					}

					GUILayout.EndHorizontal();

					break;
				}
			}
		}

		public void Load()
		{
			StandaloneFileBrowser.OpenFilePanelAsync("Load Mod", "", "rgm", false, delegate(string[] files)
			{
				if (files.Length == 0 || files[0] == "")
					return;

				var modLoader = ServiceProvider.Obtain<IModLoader>();
                var mod = modLoader.LoadMod(files[0]).Result;
				if (mod == null)
				{
					Debug.LogError("Failed loading mod " + files[0]);
					return;
				}
				
				Templates.Clear();
				Prefabs.Clear();
				
				if (mod.TryGetManifest(out Manifest man))
					Manifest = man;

				var adv = false;
				
				foreach (var tuple in mod.GetModObjects())
				{
					var go = Railgun.ModEngine.Conversion.Converter.From(tuple.Item2).To<GameObject>().Convert();
					
					foreach (var component in tuple.Item2.Components)
					{
						var type = Type.GetType($"{component.Type.Item1}, {component.Type.Item2}");
						if (type == null || (!type.GetInterfaces().Contains(typeof(IStudioObject)) && !type.GetInterfaces().Contains(typeof(ICharacterObject)) && type != typeof(ModdedScene)))
							continue;

						if (type.Assembly != typeof(BaseClothing).Assembly)
							adv = true;

						var template = new Template();

						var comp = go.GetComponent(type);
						if (comp is IStudioObject studioObject)
						{
							template.TemplateType = ETemplateType.StudioObject;
							template.StudioObjectType = studioObject.StudioObjectType;
							template.Name = studioObject.Name;
							template.Tags = studioObject.Tags;
							template.Icon = studioObject.Icon;
							template.Description = studioObject.Description;
							template.IsNSFW = studioObject.IsNSFW;
							template.FKData = studioObject.FKData;
						}
						else if (comp is ICharacterObject characterObject)
						{
							template.TemplateType = ETemplateType.CharacterObject;
							template.Name = characterObject.Name;
							template.Tags = characterObject.Tags;
							template.Icon = characterObject.Icon;
							template.Description = characterObject.Description;
							template.IsNSFW = characterObject.IsNSFW;
							template.SupportedGendersFlags = characterObject.SupportedGendersFlags;
							template.Simulation = characterObject.Simulation;
							
							switch (comp)
							{
								case IClothing cloth:
									template.ClothingType = cloth.ClothingType;
									template.CharacterObjectType = ECharacterObjectType.Clothing;
									template.ClothingStates = new Transform[Enum.GetNames(typeof(EClothingState)).Length];
					
									for (var k = 0; k < cloth.ClothingStates.Length; k++)
										template.ClothingStates[k] = cloth.ClothingStates[k];
									break;
								case IHair hair:
									template.HairType = hair.HairType;
									template.CharacterObjectType = ECharacterObjectType.Hair;
									break;
								case IAccessory acc:
									template.AccessoryType = acc.AccessoryType;
									template.CharacterObjectType = ECharacterObjectType.Accessory;
									template.DefaultParent = acc.DefaultParent;
									template.Reparentable = acc.Reparentable;

									for (var i = 0; i < defaultParents.Length; i++)
									{
										if (defaultParents[i] != acc.DefaultParent) 
											continue;
										
										template.DefaultParentIdx = i;
										break;
									}
									
									break;
								case ITexture tex:
									template.TextureType = tex.TextureType;
									template.Texture = tex.Texture;
									template.IsOverlay = tex.IsOverlay;
									template.OverlayTarget = tex.OverlayTarget;
									template.OverlayMode = tex.OverlayMode;
									template.OverlayColor = tex.OverlayColor;
									break;
							}
						}
						else if (comp is ModdedScene moddedScene)
						{
							template.TemplateType = ETemplateType.ModdedScene;
							template.Name = moddedScene.Name;
							template.Tags = moddedScene.Tags;
							template.Icon = moddedScene.Icon;
							template.Description = moddedScene.Description;
							template.IsNSFW = moddedScene.IsNSFW;
							template.ModdedSceneUsageFlags = moddedScene.UsageFlags;

							var parentObject = new GameObject(template.Name);
							go.transform.SetParent(parentObject.transform);
							
							removeUniqueID(go);
						}

						template.Tags ??= Array.Empty<string>();
						
						Templates.Add(template);
						Prefabs.Add(comp.GetType() == typeof(ModdedScene) ? null : go);
						
						break;
					}
				}

				removeUniqueIDs(Prefabs);

				FixAccessoryParents();
				
				for (var i = 0; i < BasicFolds.Length; i++)
					BasicFolds[i] = false;

				foreach (var template in Templates.Where(template => !BasicFolds[(int)template.TemplateType]))
					BasicFolds[(int)template.TemplateType] = true;

				Debug.Log("Loaded mod " + files[0]);

				if (adv)
				{
					// todo: retrieving type, includes, source code from Advanced mods
					Debug.LogWarning("Advanced tab of components that use it were not autofilled due to current limitations.");
				}

				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			});
		}
		
		public void Build()
		{
			if (!CanBuild())
			{
				Debug.LogError("Failed build checks");
				return;
			}
			
			AssignRenderers();
			
			var buildList = new List<Object>();
			foreach (var obj in Prefabs)
				buildList.AddUnique(obj);

			var currentScene = SceneManager.GetActiveScene();
			var scenes = new List<Scene>();
			
			for (var i = 0; i < buildList.Count; i++)
			{
				if (buildList[i] is not SceneAsset sceneAsset)
					continue;
				
				var path = AssetDatabase.GetAssetOrScenePath(sceneAsset);
				var moddedScene = currentScene;
					
				if (currentScene.path != path)
				{
					moddedScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
					scenes.Add(moddedScene);
				}

				var gameObject = moddedScene.GetRootGameObjects()[0];
				buildList[i] = gameObject;
			}
			
			Railgun.ModEngine.Conversion.Converter.OnContentDescriptorCreate += OnContentDescriptorCreate;
			
			addUniqueIDs(buildList);
			var mod = ServiceProvider.Obtain<IModService>().BuildMod(Manifest, buildList, !string.IsNullOrEmpty(Assembly.Item1) && Assembly.Item2 != null ? new Tuple<string, byte[]>(Assembly.Item1, Assembly.Item2) : null);
			removeUniqueIDs(buildList);

			foreach (var scene in scenes)
				EditorSceneManager.CloseScene(scene, true);

			SceneManager.SetActiveScene(currentScene);
			
			Railgun.ModEngine.Conversion.Converter.OnContentDescriptorCreate -= OnContentDescriptorCreate;
			
			if (mod == null)
			{
				Debug.LogError("Failed building Mod");
				return;
			}
			
            Debug.Log($"Built mod {mod} with {mod.ModObjectsCount()} root mod objects and {mod.AssetsCount()} loose assets.");
#if VERBOSE
			foreach(var tuple in mod.GetModObjects())
			{
                logBuildModObject(tuple.Item2);
            }
#endif
			
            StandaloneFileBrowser.SaveFilePanelAsync("Save Mod", "", $"pr.{Manifest.Author}.{Manifest.Name}.rgm", "rgm", delegate(string file)
			{
				if (string.IsNullOrEmpty(file))
					return;

				if (File.Exists(file))
				{
					Debug.Log($"Deleting existing file {file}");
					File.Delete(file);
				}
				
				try
				{
					if (ServiceProvider.Obtain<IModWriter>().WriteMod(file, mod))
						Debug.Log("Saved mod " + file);
					else
						Debug.LogError("Failed saving mod " + file);
				}
				catch (Exception e)
				{
					Debug.LogError("Failed saving mod " + file + "\n" + e);
				}
			});
		}
		
		public void AssignComponents()
		{
			if (!CanAssignComponents())
			{
				Debug.LogError("Failed component checks");
				return;
			}
			
			var asmName = $"pr.{Manifest.Author}.{Manifest.Name}.dll";
			var code = GetCompleteSource();

			// build assembly if there's any custom components
			if (code != "")
			{
				var parameters = new CompilerParameters
				{
					GenerateExecutable = false,
					GenerateInMemory = true,
					OutputAssembly = asmName,
				};
			
				var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assemblyName in typeof(BaseClothing).Assembly.GetReferencedAssemblies())
				{
					var assembly = allAssemblies.SingleOrDefault(asm => asm.FullName == assemblyName.FullName);
					if (assembly == null || assembly.IsDynamic || assembly.FullName.Contains("mscorlib.dll"))
						continue;

					parameters.ReferencedAssemblies.Add(assembly.Location);
				}

				parameters.ReferencedAssemblies.Add(typeof(BaseClothing).Assembly.Location);
			
				var provider = new CSharpCodeProvider();
				var result = provider.CompileAssemblyFromSource(parameters, code);
			
				result.Errors.Cast<CompilerError>().ToList().ForEach(error => Debug.LogError(error.ErrorText));
				if (result.Errors.Count != 0) 
					return;
				
				tempAssembly = result.CompiledAssembly;
				
				if (!File.Exists(asmName))
				{
					Debug.LogError("Generated assembly not found");
					return;
				}

				if (FileTools.ReadAllBytesSafe(asmName, out var bytes))
				{
					Assembly = new Tuple.SerializableTuple<string, byte[]>(asmName, bytes);
				}
				else
				{
					Debug.LogError($"Failed to read generated assembly {asmName}");
					return;
				}
			}

			// assign components to gameobjects
			for (var i = 0; i < Prefabs.Count; i++)
			{
				GameObject gameObject = null;

				var template = Templates[i];
				if (template.TemplateType != ETemplateType.ModdedScene)
					gameObject = (GameObject)Prefabs[i];
                
				if (template.TemplateType == ETemplateType.CharacterObject)
				{
					var existing = gameObject.GetComponents<ICharacterObject>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate((Component)existing[k]);
					
					Component comp;
					if (template.Advanced)
					{
						var type = tempAssembly.GetExportedTypes().SingleOrDefault(t => t.Name == template.Type);
						if (type == null)
						{
							Debug.LogError($"Type not found in generated assembly for {template.TemplateType.ToString()} {template.Name}");
							continue;
						}
						
						comp = gameObject.AddComponent(type);
					}
					else
					{
						switch (template.CharacterObjectType)
						{
							case ECharacterObjectType.Clothing:
								comp = gameObject.AddComponent<BaseClothing>();
								break;
							case ECharacterObjectType.Accessory:
								comp = gameObject.AddComponent<BaseAccessory>();
								break;
							case ECharacterObjectType.Hair:
								comp = gameObject.AddComponent<BaseHair>();
								break;
							case ECharacterObjectType.Texture:
								comp = gameObject.AddComponent<BaseTexture>();
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}

					if (comp is ICharacterObject charaObject)
					{
						charaObject.Name = template.Name;
						charaObject.Description = template.Description;
						charaObject.IsNSFW = template.IsNSFW;
						charaObject.SupportedGendersFlags = template.SupportedGendersFlags;
						charaObject.Icon = template.Icon;
						charaObject.Tags = template.Tags;
						charaObject.Simulation = template.Simulation;
					}
					
					switch (comp)
					{
						case IClothing cloth:
							cloth.ClothingType = template.ClothingType;
							cloth.ObjectType = ECharacterObjectType.Clothing;
							cloth.ClothingStates = new Transform[Enum.GetNames(typeof(EClothingState)).Length];

							for (var k = 0; k < template.ClothingStates.Length; k++)
								cloth.ClothingStates[k] = template.ClothingStates[k];
							break;
						case IHair hair:
							hair.HairType = template.HairType;
							hair.ObjectType = ECharacterObjectType.Hair;
							break;
						case IAccessory acc:
							acc.AccessoryType = template.AccessoryType;
							acc.ObjectType = ECharacterObjectType.Accessory;
							acc.DefaultParent = template.DefaultParent;
							acc.Reparentable = template.Reparentable;
							break;
						case ITexture tex:
							tex.TextureType = template.TextureType;
							tex.ObjectType = ECharacterObjectType.Texture;
							tex.Texture = template.Texture;
							tex.IsOverlay = template.IsOverlay;
							tex.OverlayTarget = template.OverlayTarget;
							tex.OverlayMode = template.OverlayMode;
							tex.OverlayColor = template.OverlayColor;
							break;
					}
				}
				else if (template.TemplateType == ETemplateType.StudioObject)
				{
					var existing = gameObject.GetComponents<IStudioObject>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate((Component)existing[k]);

					Component comp;
					if (template.Advanced)
					{
						var type = tempAssembly.GetExportedTypes().SingleOrDefault(t => t.Name == template.Type);
						if (type == null)
						{
							Debug.LogError($"Type not found in generated assembly for {template.TemplateType.ToString()} {template.Name}");
							continue;
						}
						
						comp = gameObject.AddComponent(type);
					}
					else
					{
						comp = gameObject.AddComponent<BaseStudioObject>();
					}
					
					if (comp is IStudioObject studioObject)
					{
						studioObject.StudioObjectType = template.StudioObjectType;
						studioObject.Name = template.Name;
						studioObject.Description = template.Description;
						studioObject.IsNSFW = template.IsNSFW;
						studioObject.Icon = template.Icon;
						studioObject.Tags = template.Tags;
						studioObject.FKData = template.FKData;
					}
				}
				else if (template.TemplateType == ETemplateType.ModdedScene)
				{
					var path = AssetDatabase.GetAssetOrScenePath((SceneAsset)Prefabs[i]);
					var currentScene = SceneManager.GetActiveScene();
					
					var loaded = currentScene.path == path;
					var moddedScene = loaded ? currentScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

					gameObject = moddedScene.GetRootGameObjects()[0];

					var existing = gameObject.GetComponents<ModdedScene>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate(existing[k]);

					var moddedSceneComp = gameObject.AddComponent<ModdedScene>();
					moddedSceneComp.Name = template.Name;
					moddedSceneComp.Description = template.Description;
					moddedSceneComp.IsNSFW = template.IsNSFW;
					moddedSceneComp.Icon = template.Icon;
					moddedSceneComp.Tags = template.Tags;
					moddedSceneComp.UsageFlags = template.ModdedSceneUsageFlags;

					EditorSceneManager.SaveScene(moddedScene);
					
					if (!loaded)
					{
						EditorSceneManager.CloseScene(moddedScene, true);
						SceneManager.SetActiveScene(currentScene);
					}
				}

				if (template.TemplateType == ETemplateType.StudioObject)
				{
					var hasLODs = gameObject.transform.Find("LOD0") != null;
					if (!hasLODs) 
						continue;
					
					var lods = new List<LOD>();

					for (var k = 0; k < gameObject.transform.childCount; k++)
					{
						var child = gameObject.transform.GetChild(k);
						if (!lodRegex.IsMatch(child.name))
							continue;

						lods.Add(new LOD(1f / (lods.Count + 2), child.GetComponentsInChildren<Renderer>()));
					}

					if (lods.Count == 0)
						continue;
					
					if (lods.Count is > 0 and < 3)
					{
						Debug.LogError($"There must be either none, or at least 3 LODs for {template.TemplateType.ToString()} {template.Name}");
						continue;
					}
					
					var lodGroup = gameObject.GetComponent<LODGroup>();
					if (lodGroup == null)
						lodGroup = gameObject.AddComponent<LODGroup>();

					lodGroup.SetLODs(lods.ToArray());
					
					var lodHolder = gameObject.GetComponent<LODHolder>();
					if (lodHolder == null)
						lodHolder = gameObject.AddComponent<LODHolder>();
					
					lodHolder.LODGroup = lodGroup;

					Debug.Log($"Created LOD Group and assigned {lods.Count} valid LODs for {template.TemplateType.ToString()} {template.Name}");
				}
				else if (template.TemplateType == ETemplateType.CharacterObject)
				{
					var targets = new List<Transform>();

					if (template.CharacterObjectType is ECharacterObjectType.Clothing)
					{
						foreach (var clothingState in template.ClothingStates)
							targets.Add(clothingState);
					}
					else
					{
						targets.Add(gameObject.transform);
					}

					foreach (var target in targets)
					{
						if (target == null)
							continue;
						
						var lods = new List<LOD>();

						for (var k = 0; k < target.childCount; k++)
						{
							var child = target.GetChild(k);
							if (!lodRegex.IsMatch(child.name))
								continue;

							lods.Add(new LOD(1f / (lods.Count + 2), child.GetComponentsInChildren<Renderer>()));
						}
					
						if (lods.Count == 0)
							continue;
						
						if (lods.Count is > 0 and < 3)
						{
							Debug.LogError($"There must be either none, or at least 3 LODs for {template.TemplateType.ToString()} {template.Name}");
							continue;
						}
						
						var stateGameObject = target.gameObject;

						var lodGroup = stateGameObject.GetComponent<LODGroup>();
						if (lodGroup == null)
							lodGroup = stateGameObject.AddComponent<LODGroup>();
						
						lodGroup.SetLODs(lods.ToArray());
					
						var lodHolder = stateGameObject.GetComponent<LODHolder>();
						if (lodHolder == null)
							lodHolder = stateGameObject.AddComponent<LODHolder>();
					
						lodHolder.LODGroup = lodGroup;
						
						Debug.Log($"Created LOD Group and assigned {lods.Count} valid LODs for {template.TemplateType.ToString()} {template.Name} target {target.name}");
					}
				}
			}

			// cache and import assembly if there's any custom components
			if (code != "")
			{
				Directory.CreateDirectory(Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Cache"));
			
				var asmPath = Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Cache", asmName);
				var codePath = Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Cache", asmName + ".cs.source");
				
				if (File.Exists(asmPath))
					File.Delete(asmPath);

				if (File.Exists(codePath))
					File.Delete(codePath);

				if (FileTools.MoveSafe(asmName, asmPath))
					Debug.Log($"Cached assembly to {asmPath}");
				else
					Debug.LogError($"Failed caching assembly to {asmPath}");
				
				if (FileTools.WriteAllTextSafe(codePath, code))
					Debug.Log($"Cached code to {codePath}");
				else
					Debug.LogError($"Failed caching code to {codePath}");
				
				AssetDatabase.ImportAsset(asmPath, ImportAssetOptions.ForceSynchronousImport);
			
				var import = AssetImporter.GetAtPath(asmPath) as PluginImporter;
				import.SetCompatibleWithAnyPlatform(false);
				import.SetCompatibleWithEditor(true);
				import.SaveAndReimport();
			}
			
			Debug.Log("Assigned components");
		}

		public void AssignRenderers()
		{
			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				
				var prefab = Prefabs[i];
				if (prefab == null)
				{
					Debug.LogWarning($"Failed assigning renderers for {template.Name} because it has no object assigned");
					continue;
				}

				switch (template.TemplateType)
				{
					case ETemplateType.CharacterObject:
					{
						var characterObject = ((GameObject)prefab).GetComponent<ICharacterObject>();
						characterObject.Renderers = characterObject.GetGameObject().GetComponentsInChildren<Renderer>();
						break;
					}
					case ETemplateType.StudioObject:
					{
						var studioObject = ((GameObject)prefab).GetComponent<IStudioObject>();
						studioObject.Renderers = studioObject.GetGameObject().GetComponentsInChildren<Renderer>();
						break;
					}
				}
			}
		}
		
		public string GetCompleteSource()
		{
			var custom = Templates.Any(template => template.Advanced);
			if (!custom)
				return "";

			var source = $@"using System;
using Code.Frameworks.Character;
using Code.Frameworks.Character.CharacterObjects;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;

using Code.Frameworks.ModdedScenes;
using Code.Frameworks.ModdedScenes.Flags;

using Code.Frameworks.PhysicsSimulation;

using Code.Frameworks.ForwardKinematics;
using Code.Frameworks.ForwardKinematics.Interfaces;
using Code.Frameworks.ForwardKinematics.Structs;

using Code.Frameworks.Studio;
using Code.Frameworks.Studio.StudioObjects;
using Code.Frameworks.Studio.Enums;
using Code.Frameworks.Studio.Interfaces;

using Code.Interfaces;

{GetCompleteUsings()}
namespace {Manifest.Author}.{Manifest.Name} 
{{
";

			source = Templates.Where(template => template.Advanced && template.Type != "").Aggregate(source, (current, template) => current + template.Source + "\n");
			source += "}";
			
			return source;
		}

		public string GetCompleteUsings()
		{
			var usings = new List<string>();
			usings = Templates.Where(template => template.Advanced).Aggregate(usings, (current, template) => current.Union(template.Usings).ToList());

			var builder = new StringBuilder();
			
			foreach (var usingStr in usings)
				builder.AppendLine($"using {usingStr};");

			return builder.ToString();
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
                selectedPreset = (EPreset)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_PRESET"), selectedPreset, "MODCREATOR_BASIC_CLOTHSIM_PRESET_", GUILayout.Width(200));
                EditorGUIUtility.labelWidth = 0;

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				{
					SimulationData sData = SimulationData.Create(selectedPreset);
					sData.Preset = selectedPreset;
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
					
					// todo: clothSim.Preset
					
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
		{ // todo: localize this stuff
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
		
		#region presets

		public void SavePreset(string presetName, bool overwrite = false)
		{
			if (string.IsNullOrEmpty(presetName))
			{
				Debug.LogError("Preset name not set");
				return;
			}

			if (presetName.Contains("/") || presetName.Contains("\\"))
			{
				Debug.LogError("Preset name can not contain slashes");
				return;
			}
			
			var root = new GameObject("Preset");

			var preset = root.AddComponent<ModCreatorPreset>();
			ToPreset(preset);

			foreach (var obj in preset.Prefabs)
			{
				if (obj == null || obj is not GameObject go) 
					continue;
				
				go.transform.SetParent(root.transform);
			}

			var targetPath = Path.Combine(ModCreatorPath, $"Code/Editor/ModEngine/Presets/{presetName}.prefab");
			if (overwrite && File.Exists(targetPath))
			{
				Debug.Log($"Removing preset {presetName}");
				File.Delete(targetPath);
			}
			
			var path = AssetDatabase.GenerateUniqueAssetPath(targetPath);
			
			PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
			
			foreach (var obj in preset.Prefabs)
			{
				if (obj == null || obj is not GameObject go) 
					continue;
				
				go.transform.SetParent(null);
			}
			 
			DestroyImmediate(root.gameObject);
			
			var file = new FileInfo(path);
			Debug.Log($"Saved preset {file.Name[..^file.Extension.Length]}");
			
			SetupPresets();
		}

		public void LoadPreset(string presetName)
		{
			if (!File.Exists(Path.Combine(ModCreatorPath, $"Code/Editor/ModEngine/Presets/{presetName}.prefab")))
			{
				Debug.LogError("Preset not found");
				return;
			}
			
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(ModCreatorPath, $"Code/Editor/ModEngine/Presets/{presetName}.prefab"));

			var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			if (root == null)
			{
				Debug.LogError("Failed to load preset");
				return;
			}

			PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

			var preset = root.GetComponent<ModCreatorPreset>();
			FromPreset(preset);

			foreach (var obj in preset.Prefabs)
			{
				if (obj == null || obj is not GameObject go) 
					continue;
				
				go.transform.SetParent(null);
			}

			DestroyImmediate(root);

			for (var i = 0; i < BasicFolds.Length; i++)
				BasicFolds[i] = false;

			foreach (var template in Templates.Where(template => !BasicFolds[(int)template.TemplateType]))
				BasicFolds[(int)template.TemplateType] = true;

			Debug.Log($"Loaded preset {presetName}");
		}

		public void SetupPresets()
		{
			Presets.Clear();

			Directory.CreateDirectory(Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Presets"));

			foreach (var file in Directory.GetFiles(Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Presets"), "*.prefab"))
			{
				var info = new FileInfo(file);
				Presets.Add(info.Name[..^info.Extension.Length]);
			}
		}
		
		public void ToPreset(ModCreatorPreset preset)
		{
			preset.Name = Manifest.Name;
			preset.ModId = Manifest.ModId;
			preset.Description = Manifest.Description;
			preset.Version = Manifest.Version;
			preset.Author = Manifest.Author;
			preset.CompatibleVersions = Manifest.CompatibleVersions;
			preset.Dependencies = Manifest.Dependencies;

			preset.Prefabs = Prefabs;
			preset.Templates = Templates;
		}

		public void FromPreset(ModCreatorPreset preset)
		{
			Manifest.Name = preset.Name;
			Manifest.ModId = preset.ModId;
			Manifest.Description = preset.Description;
			Manifest.Version = preset.Version;
			Manifest.Author = preset.Author;
			Manifest.CompatibleVersions = preset.CompatibleVersions;
			Manifest.Dependencies = preset.Dependencies;

			Prefabs = preset.Prefabs;
			Templates = preset.Templates;
			
			FixAccessoryParents();
			FixNonexistentStates();
		}
		
		#endregion
		
		#region checks

		public bool CanBuild()
		{
			var pass = true;
			var checkAsm = true;

			if (string.IsNullOrEmpty(Manifest.Name))
			{
				pass = false;
				Debug.LogWarning("Mod name is not set");
			}
			
			if (string.IsNullOrEmpty(Manifest.Author))
			{
				pass = false;
				Debug.LogWarning("Mod author is not set");
			}
			
			if (string.IsNullOrEmpty(Manifest.Version))
			{
				pass = false;
				Debug.LogWarning("Mod version is not set");
			}
			
			if (Prefabs.Count == 0 || Templates.Count == 0)
			{
				pass = false;
				Debug.LogWarning("There are no objects or components set");
			}
			
			if (Prefabs.Count != Templates.Count)
			{
				pass = false;
				Debug.LogWarning($"Objects {Prefabs.Count} and Components {Templates.Count} count mismatch");
			}
			
			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				var prefab = Prefabs[i];
				
				if (checkAsm && template.Advanced && string.IsNullOrEmpty(Assembly.Item1))
				{
					checkAsm = false;
					pass = false;
					Debug.LogWarning("Mod assembly is not built");
				}

				if (prefab != null)
				{
					if (template.TemplateType == ETemplateType.ModdedScene)
					{
						var path = AssetDatabase.GetAssetOrScenePath((SceneAsset)Prefabs[i]);
						var currentScene = SceneManager.GetActiveScene();
					
						var loaded = currentScene.path == path;
						var moddedScene = loaded ? currentScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        
						if (template.TemplateType == ETemplateType.ModdedScene && moddedScene.GetRootGameObjects()[0].GetComponent<ModdedScene>() == null)
						{
							pass = false;
							Debug.LogWarning($"Component is not added for {template.Name}");
						}
					
						if (!loaded)
						{
							EditorSceneManager.CloseScene(moddedScene, true);
							SceneManager.SetActiveScene(currentScene);
						}
						
						continue;
					}
					
					var gameObject = (GameObject)prefab;
					
					if ((template.TemplateType == ETemplateType.CharacterObject && gameObject.GetComponent<ICharacterObject>() == null) || (template.TemplateType == ETemplateType.StudioObject && gameObject.GetComponent<IStudioObject>() == null))
					{
						pass = false;
						Debug.LogWarning($"Component is not added for {template.Name}");
					}

					if (template.TemplateType == ETemplateType.CharacterObject && template.CharacterObjectType == ECharacterObjectType.Clothing)
					{
						var clothingObject = gameObject.GetComponent<IClothing>();
						if (clothingObject != null)
						{
							var fullState = clothingObject.GetStateObject(EClothingState.Full);
							if (fullState == null)
							{
								pass = false;
								Debug.LogWarning($"Full state missing for {template.Name}");
							}
							else if (fullState.GetComponentInChildren<SkinnedMeshRenderer>() == null)
							{
								pass = false;
								Debug.LogWarning($"Skinned mesh renderer missing for full state of {template.Name}. Make sure your mesh has an Armature");
							}

							var halfState = clothingObject.GetStateObject(EClothingState.Half);
							if (halfState == null)
							{
								Debug.LogWarning($"Optional half state missing for {template.Name}");
							}
							else if (halfState.GetComponentInChildren<SkinnedMeshRenderer>() == null)
							{
								pass = false;
								Debug.LogWarning($"Skinned mesh renderer missing for half state of {template.Name}. Make sure your mesh has an Armature");
							}

							if (fullState == halfState && fullState != null)
							{
								pass = false;
								Debug.LogWarning($"Full and half states of {template.Name} are the same object. Make sure they are separate objects or unassign the half state");
							}
						}
					}

					if (template.TemplateType == ETemplateType.CharacterObject && template.CharacterObjectType == ECharacterObjectType.Texture)
					{
						var texture = gameObject.GetComponent<ITexture>();
						if (texture.Texture == null)
						{
							pass = false;
							Debug.LogWarning($"Texture missing for {template.Name}");
						}

						var components = gameObject.GetComponents<Component>().ToList();
						components.Remove(gameObject.transform);
						components.Remove((Component)texture);

						if (gameObject.transform.childCount == 0 && components.Count == 0) 
							continue;
						
						pass = false;
						Debug.LogWarning($"Object for {template.Name} is not empty. Texture mods must have empty GameObjects assigned with no meshes");
					}

					var anySkinnedRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
					if (anySkinnedRenderers != null && anySkinnedRenderers.Length != 0)
					{
						var armature = gameObject.transform.Find("Armature");
						if (armature == null)
						{
							pass = false;
							Debug.LogWarning($"Armature for {template.Name} was not found. Skinned meshes must have an armature at the root of the object, with the name of \"Armature\"");
						}
					}
				}
				else
				{
					pass = false;
					Debug.LogWarning($"Object is not set for {template.Name}");
				}
			}

			for (var i = 0; i < Prefabs.Count; i++)
			{
				for (var k = 0; k < Prefabs.Count; k++)
				{
					if (i == k || Prefabs[i] == null || Prefabs[k] == null)
						continue;

					if (Prefabs[i] != Prefabs[k]) 
						continue;
					
					pass = false;
					Debug.LogWarning($"Items {Templates[i].Name} and {Templates[k].Name} share the same object. This is not allowed");
				}
			}
			
			return pass;
		}

		public bool CanAssignComponents()
		{
			var pass = true;

			if (Prefabs.Count == 0 || Templates.Count == 0)
			{
				pass = false;
				Debug.LogWarning("There are no objects or components set");
			}
			
			if (Prefabs.Count != Templates.Count)
			{
				pass = false;
				Debug.LogWarning($"Objects {Prefabs.Count} and Components {Templates.Count} count mismatch");
			}

			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				
				if (Prefabs[i] == null)
				{
					pass = false;
					Debug.LogWarning($"Object is not set for {template.Name}");
				}
				
				if (string.IsNullOrEmpty(template.Name))
				{
					pass = false;
					Debug.LogWarning($"Name is not set for {template.Name}");
				}
				
				if (template.Icon == null)
				{
					pass = false;
					Debug.LogWarning($"Icon is not set for {template.Name}");
				}
				
				if (template.TemplateType == ETemplateType.ModdedScene && Prefabs[i] != null)
				{
					if (template.ModdedSceneUsageFlags == 0)
						Debug.LogWarning($"Modded Scene usage is empty for {template.Name}");

					var path = AssetDatabase.GetAssetOrScenePath((SceneAsset)Prefabs[i]);
					var currentScene = SceneManager.GetActiveScene();
					
					var loaded = currentScene.path == path;
					var moddedScene = loaded ? currentScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

					var objects = moddedScene.GetRootGameObjects();
					switch (objects.Length)
					{
						case 0:
						{
							pass = false;
							Debug.LogWarning($"Root object 'ModdedScene' not found for {template.Name}");
							break;
						}
						case 1:
						{
							if (objects[0].name != "ModdedScene")
							{
								pass = false;
								Debug.LogWarning($"Root object 'ModdedScene' not found for {template.Name}");
							}
							break;
						}
						case > 1:
						{
							pass = false;
							Debug.LogWarning($"Multiple root objects found for {template.Name}");
							break;
						}
					}

					if (!loaded)
					{
						EditorSceneManager.CloseScene(moddedScene, true);
						SceneManager.SetActiveScene(currentScene);
					}
				}

				if (template.TemplateType == ETemplateType.CharacterObject && template.SupportedGendersFlags == ESupportedGendersFlags.None)
					Debug.LogWarning($"Supported genders is None for {template.Name}. This might cause the item to not show up");

				if (template.FKData.Groups != null)
				{
					foreach (var group in template.FKData.Groups)
					{
						if (group.Transforms == null || group.Transforms.Length == 0)
						{
							pass = false;
							Debug.LogWarning($"FK Group {group.Name} for {template.Name} does not have any transforms");
							
							continue;
						}
						
						foreach (var transform in group.Transforms)
						{
							if (transform.Transform != null) 
								continue;
							
							pass = false;
							Debug.LogWarning($"FK Group {group.Name} transform {transform.Name} for {template.Name} is null");
							
							break;
						}
					}
				}
                
				if (template.Advanced)
				{
					if (string.IsNullOrEmpty(template.Type))
					{
						pass = false;
						Debug.LogWarning($"Custom Type is not set for {template.Name}");
					}

					if (Templates.Any(innerTemplate => innerTemplate != template && innerTemplate.Advanced && innerTemplate.Type == template.Type))
					{
						pass = false;
						Debug.LogWarning($"Custom Type is not unique for {template.Name}");
					}
					
					if (string.IsNullOrEmpty(template.Source))
					{
						pass = false;
						Debug.LogWarning($"Source Code is not set for for {template.Name}");
					}
				}
			}

			for (var i = 0; i < Prefabs.Count; i++)
			{
				for (var k = 0; k < Prefabs.Count; k++)
				{
					if (i == k || Prefabs[i] == null || Prefabs[k] == null)
						continue;

					if (Prefabs[i] != Prefabs[k]) 
						continue;
					
					pass = false;
					Debug.LogWarning($"Items {Templates[i].Name} and {Templates[k].Name} share the same object. This is not allowed");
				}
			}

			return pass;
		}

		#endregion
		
		#region debug

		private void logBuildModObject(ModObject modObject)
		{
            Debug.Log($"-----------BEGIN MODOBJECT {modObject.Name}------------");
            Debug.Log($"{modObject.Name} contains {modObject.Children.Count} children and {modObject.Components.Count} components.");

			if(modObject.Components.Count > 0)
			{
                Debug.Log($"-----------BEGIN COMPONENTS {modObject.Name}------------");
                foreach (var comp in modObject.Components)
                {
                    Debug.Log($"{comp.Type.Item1} contains {comp.ChildrenLinks.Count} links to other components, {comp.TypeLinks.Count} links to custom types, {comp.AssetLinks.Count} links to assets and {comp.ArrayLinks.Count} links to arrays.");

					if(comp.TypeLinks.Count > 0)
					{
                        Debug.Log($"-----------BEGIN DUMP TYPELINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.TypeLinks)
                        {
                            if (kv.Value is IAssetPipelineType arr)
                            {
                                Debug.Log($"Key {kv.Key}, Value {arr.GetType()}");
                            }
                        }
                        Debug.Log($"-----------END DUMP TYPELINKS {modObject.Name}------------");
                    }

                    if (comp.AssetLinks.Count > 0)
                    {
                        Debug.Log($"-----------BEGIN DUMP ASSETLINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.AssetLinks)
                        {
                            if (kv.Value is Asset arr)
                            {
                                Debug.Log($"Key {kv.Key}, Value {arr.GetType()}");
                            }
                        }
                        Debug.Log($"-----------END DUMP ASSETLINKS {modObject.Name}------------");
                    }

                    if (comp.ChildrenLinks.Count > 0)
                    {
                        Debug.Log($"-----------BEGIN DUMP CHILDRENLINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.ChildrenLinks)
                        {
                            if (kv.Value is Array arr)
                            {
                                Debug.Log($"Key {kv.Key}, Value {arr.GetValue(0)} {arr.GetValue(1)}");
                            }
                        }
                        Debug.Log($"-----------END DUMP CHILDRENLINKS {modObject.Name}------------");
                    }

                    if (comp.ArrayLinks.Count > 0)
                    {
                        Debug.Log($"-----------BEGIN DUMP ARRAYLINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.ArrayLinks)
                        {
							if(kv.Value is Array arr)
							{
                                Debug.Log($"Key {kv.Key}, Value {arr.GetValue(0)} {arr.GetValue(1)}");
                            }
                        }
                        Debug.Log($"-----------END DUMP ARRAYLINKS {modObject.Name}------------");
                    }
                }
                Debug.Log($"-----------END COMPONENTS {modObject.Name}------------");
            }
            
			if(modObject.Children.Count > 0)
			{
                Debug.Log($"-----------BEGIN CHILDREN OF {modObject.Name}------------");
                foreach (var child in modObject.Children)
                {
                    logBuildModObject(child);
                }
                Debug.Log($"-----------END CHILDREN OF {modObject.Name}------------");
            }
            Debug.Log($"-----------END MODOBJECT {modObject.Name}------------");
        }

		#endregion
		
		#region unique indexing

		private void addUniqueIDs(List<Object> objects)
		{
			var id = 0;
			foreach (var go in objects)
			{
				foreach (var transform in ((GameObject)go).GetComponentsInChildren<Transform>(true))
				{
					transform.name = transform.name + uniqueIDFormat + id;
					id++;
				}

				id = 0;
			}
		}

		private void removeUniqueIDs(List<Object> objects)
		{
			foreach (var go in objects.Where(go => go != null))
			{
				removeUniqueID(go);
			}
		}
		
		private void removeUniqueID(Object obj)
		{
			foreach (var transform in ((GameObject)obj).GetComponentsInChildren<Transform>(true))
			{
				var index = transform.name.LastIndexOf(uniqueIDFormat, StringComparison.InvariantCultureIgnoreCase);
				if (index == -1)
					continue;

				transform.name = transform.name[..index];
			}
		}

		#endregion
		
		#region ui additions

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
		
		private void verticalList(List<string> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add("");
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				list[i] = EditorGUILayout.TextField("", list[i]);
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				GUILayout.EndHorizontal();
			}
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
        
		private void verticalPresetList(List<string> list)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (Presets[i] == null)
				{
					SetupPresets();
					return;
				}
				
				GUILayout.BeginHorizontal();
				
				if (GUILayout.Button(Presets[i]))
					LoadPreset(Presets[i]);

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_REPLACE"), GUILayout.Width(75)))
				{
					AssetDatabase.DeleteAsset(Path.Combine(ModCreatorPath, $"Code/Editor/ModEngine/Presets/{Presets[i]}.prefab"));
					SavePreset(Presets[i]);
					Debug.Log($"Replaced preset {Presets[i]}");
				}
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					AssetDatabase.DeleteAsset(Path.Combine(ModCreatorPath, $"Code/Editor/ModEngine/Presets/{Presets[i]}.prefab"));
					Debug.Log($"Removed preset {Presets[i]}");
					
					SetupPresets();
				}
				
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<Object> list, ETemplateType type, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(Templates.Count(t => t.TemplateType == type)));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
			{
				if (Templates.Count < 254)
				{
					list.Add(new Object());
					Templates.Add(new Template { TemplateType = type });

					CurrentTemplate = -1;
				}
				else
				{
					Debug.LogWarning("More than 254 modded objects in a single mod is not supported");
				}
			}
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
			{
				for (var i = Templates.Count - 1; i >= 0; i--)
				{
					if (Templates[i].TemplateType != type) 
						continue;
					
					list.RemoveAt(i);
					Templates.RemoveAt(i);
				}

				CurrentTemplate = -1;
			}
			GUILayout.EndHorizontal();

			var templates = Templates.Where(t => t.TemplateType == type).ToArray();
			
			for (var i = 0; i < templates.Length; i++)
			{
				var current = Templates.FindIndex(t => t == templates[i]);

				GUILayout.BeginHorizontal();
				
				if (type == ETemplateType.ModdedScene)
					list[current] = EditorGUILayout.ObjectField("", list[current], typeof(SceneAsset), false, GUILayout.Width(250));
				else
					list[current] = EditorGUILayout.ObjectField("", list[current], typeof(GameObject), true, GUILayout.Width(250));

				if (list[current] != null && type != ETemplateType.ModdedScene)
				{
					var go = (GameObject)list[current];
					if (go.scene != SceneManager.GetActiveScene())
						list[current] = null;
				}
				
				if (GUILayout.Button((CurrentTemplate == current ? "*" : "") + "" + Templates[current].Name))
					CurrentTemplate = current;
				
				if (GUILayout.Button("/\\", GUILayout.Width(25)))
				{
					if (i == 0)
						return;

					var target = Templates.FindIndex(t => t == templates[i - 1]);
					
					(Templates[current], Templates[target]) = (Templates[target], Templates[current]);
					(list[current], list[target]) = (list[target], list[current]);

					if (CurrentTemplate == current)
						CurrentTemplate = target;
					else if (CurrentTemplate == target)
						CurrentTemplate = current;
				}
				
				if (GUILayout.Button("\\/", GUILayout.Width(25)))
				{
					if (i == templates.Length - 1)
						return;
					
					var target = Templates.FindIndex(t => t == templates[i + 1]);
					
					(Templates[current], Templates[target]) = (Templates[target], Templates[current]);
					(list[current], list[target]) = (list[target], list[current]);
					
					if (CurrentTemplate == current)
						CurrentTemplate = target;
					else if (CurrentTemplate == target)
						CurrentTemplate = current;
				}
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					list.RemoveAt(i);
					Templates.Remove(templates[i]);
					
					CurrentTemplate = -1;
					return;
				}
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<Transform> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(null);
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				
				list[i] = (Transform)EditorGUILayout.ObjectField("", list[i], typeof(Transform), true);
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}

		private string[] verticalList(string[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				array = array.Append("").ToArray();
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				array = Array.Empty<string>();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				array[i] ??= "";
				
				GUILayout.BeginHorizontal();
				array[i] = EditorGUILayout.TextField("", array[i]);
				array[i] = array[i].Replace("\"", "");
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					array[i] = null;
					array = array.Where(s => s != null).ToArray();
				}
				GUILayout.EndHorizontal();
			}

			return array;
		}

		private string itemsCount(int count)
		{
			return $" ({count} item{(count == 1 ? "" : "s")})";
		}

		public GUIStyle GetBackgroundStyle(Color color)
		{
			Texture.SetPixel(0, 0, color);
			Texture.Apply();
			
			GUIStyle.normal.background = Texture;
			return GUIStyle;
		}
		
		#endregion

		public void SetLanguage(string language)
		{
			for (var i = 0; i < AvailableLanguages.Length; i++)
			{
				if (AvailableLanguages[i] != language) 
					continue;
				
				Language = i;
			}
		}

		public string GetLocalizedString(string key)
		{
			return LocalizationManager.Instance.GetLocalizedString(AvailableLanguages[Language], key).Value;
		}

		public Enum LocalizedEnumPopup(string label, Enum selected, string localizationRoot, params GUILayoutOption[] options)
		{
			var names = Enum.GetNames(selected.GetType());
			for (var i = 0; i < names.Length; i++)
				names[i] = GetLocalizedString(localizationRoot + names[i].ToUpper());
			
			var popup = EditorGUILayout.Popup(label, Convert.ToInt32(selected), names, options);
			return (Enum)Enum.ToObject(selected.GetType(), popup);
		}
		
		public void FixAccessoryParents()
		{
			foreach (var template in Templates.Where(template => template.TemplateType == ETemplateType.CharacterObject).Where(template => template.CharacterObjectType == ECharacterObjectType.Accessory))
			{
				for (var i = 0; i < defaultParents.Length; i++)
				{
					if (defaultParents[i] == template.DefaultParent)
					{
						template.DefaultParentIdx = i;
					}
				}
			}
		}

		public void FixNonexistentStates()
		{
			foreach (var template in Templates)
			{
				template.SetupStates();
			}
		}
		
		public ContentDescriptor OnContentDescriptorCreate(Component component, ContentDescriptor contentDescriptor)
		{
			switch (component)
			{
				case IStudioObject studioObject:
					contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("studioobjecttype", studioObject.StudioObjectType)).ToArray();
					contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", studioObject.IsNSFW)).ToArray();
					break;
				case ICharacterObject characterObject:
					switch (characterObject)
					{
						case IClothing clothing:
							contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("clothingtype", clothing.ClothingType)).ToArray();
							break;
						case IHair hair:
							contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("hairtype", hair.HairType)).ToArray();
							break;
						case IAccessory accessory:
							contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("accessorytype", accessory.AccessoryType)).ToArray();
							break;
						case ITexture texture:
							contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("texturetype", texture.TextureType)).ToArray();
							break;
					} 
					contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("supportedgenders", characterObject.SupportedGendersFlags)).ToArray();
					contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", characterObject.IsNSFW)).ToArray();
					break;
				case ModdedScene moddedScene:
					contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("moddedscene", moddedScene.UsageFlags)).ToArray();
					contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", moddedScene.IsNSFW)).ToArray();
					break;
			}
			
			return contentDescriptor;
		}
	}
}