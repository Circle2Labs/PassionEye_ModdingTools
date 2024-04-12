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
using Object = UnityEngine.Object;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using Tuple = Code.Tools.Tuple;

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
		
		private UnityEngine.Vector2 presetsScrollPosition;
		private UnityEngine.Vector2 builderScrollPosition;
		private UnityEngine.Vector2 basicScrollPosition;
		private UnityEngine.Vector2 advancedScrollPosition;

		private Template copyBuffer;
		private Assembly tempAssembly;

		private Regex lodRegex = new ("^LOD\\d$");
		private const string basicFilter = @"(^[0-9])|([^a-zA-Z0-9_])";
		private const string uniqueIDFormat = "_#";

		// todo: figure out a better solution. Maybe allow the user to pick for which base mesh they are making the item and then populate this list accordingly.
		private readonly string[] defaultParents =
		{
			// enpi
			//"DEF_hips.001","DEF_buttocks.L.001","DEF_buttocks.L.001_end","DEF_buttocks.R.001","DEF_buttocks.R.001_end","DEF_hips.002","DEF_hips.002_end","DEF_hips.L.001","DEF_thigh.L.001","DEF_thigh.L.002","DEF_shin.L.001","DEF_feet.L.001","DEF_feet.L.003","DEF_feet.L.003_end","DEF_Toe_master.L.001","DEF_index_toe.L.002","DEF_index_toe.L.003","DEF_index_toe.L.003_end","DEF_middle_toe.L.002","DEF_middle_toe.L.003","DEF_middle_toe.L.003_end","DEF_pinky_toe.L.002","DEF_pinky_toe.L.002_end","DEF_ring_toe.L.002","DEF_ring_toe.L.003","DEF_ring_toe.L.003_end","DEF_thumb_toe.L.002","DEF_thumb_toe.L.002_end","DEF_hips.R.001","DEF_thigh.R.001","DEF_thigh.R.002","DEF_shin.R.001","DEF_feet.R.001","DEF_feet.R.003","DEF_feet.R.003_end","DEF_Toe_master.R.001","DEF_index_toe.R.002","DEF_index_toe.R.003","DEF_index_toe.R.003_end","DEF_middle_toe.R.002","DEF_middle_toe.R.003","DEF_middle_toe.R.003_end","DEF_pinky_toe.R.002","DEF_pinky_toe.R.002_end","DEF_ring_toe.R.002","DEF_ring_toe.R.003","DEF_ring_toe.R.003_end","DEF_thumb_toe.R.002","DEF_thumb_toe.R.002_end","DEF_penis.001","DEF_penis.002","DEF_penis.003","DEF_penis.003_end","DEF_spine.001","DEF_spine.002","DEF_spine.003","DEF_breast.L.001","DEF_breast.L.002","DEF_breast.L.003","DEF_breast.L.003_end","DEF_breast.R.001","DEF_breast.R.002","DEF_breast.R.003","DEF_breast.R.003_end","DEF_neck.001","DEF_head.001","DEF_Ear_L.001","DEF_Ear_L.002","DEF_Ear_L.003","DEF_Ear_L.004","DEF_Ear_L.005","DEF_Ear_L.005_end","DEF_Ear_R.001","DEF_Ear_R.002","DEF_Ear_R.003","DEF_Ear_R.004","DEF_Ear_R.005","DEF_Ear_R.005_end","DEF_eye_L.001","DEF_eye_L.001_end","DEF_eye_R.001","DEF_eye_R.001_end","DEF_Jaw.001","DEF_Jaw_L.002","DEF_Jaw_L.003","DEF_Jaw_L.004","DEF_Jaw_L.004_end","DEF_Jaw_R.002","DEF_Jaw_R.003","DEF_Jaw_R.004","DEF_Jaw_R.004_end","DEF_LipMouthBottom.001","DEF_LipMouthBottom.001_end","DEF_tongue.001","DEF_tongue.002","DEF_tongue.003","DEF_tongue.003_end","DEF_Nose.001","DEF_Nose.001_end","DEF_Scalp_L.001","DEF_Scalp_L.002","DEF_Scalp_L.003","DEF_Scalp_L.004","DEF_Scalp_L.004_end","DEF_Scalp_R.001","DEF_Scalp_R.002","DEF_Scalp_R.003","DEF_Scalp_R.004","DEF_Scalp_R.004_end","DEF_UpperCheek_L.001","DEF_UpperCheek_L.002","DEF_UpperCheek_L.003","DEF_UpperCheek_L.003_end","DEF_UpperCheek_R.001","DEF_UpperCheek_R.002","DEF_UpperCheek_R.003","DEF_UpperCheek_R.003_end","DEF_LipMouthUpper.001","DEF_LipMouthUpper.001_end","DEF_shoulder.L.001","DEF_biceps.L.001","DEF_biceps.L.002","DEF_forearm.L.001","DEF_hand.L.001","DEF_index.L.002","DEF_index.L.003","DEF_index.L.004","DEF_index.L.004_end","DEF_middle.L.002","DEF_middle.L.003","DEF_middle.L.004","DEF_middle.L.004_end","DEF_pinky.L.002","DEF_pinky.L.003","DEF_pinky.L.004","DEF_pinky.L.004_end","DEF_ring.L.002","DEF_ring.L.003","DEF_ring.L.004","DEF_ring.L.004_end","DEF_thumb.L.001","DEF_thumb.L.002","DEF_thumb.L.003","DEF_thumb.L.003_end","DEF_shoulder.R.001","DEF_biceps.R.001","DEF_biceps.R.002","DEF_forearm.R.001","DEF_hand.R.001","DEF_index.R.002","DEF_index.R.003","DEF_index.R.004","DEF_index.R.004_end","DEF_middle.R.002","DEF_middle.R.003","DEF_middle.R.004","DEF_middle.R.004_end","DEF_pinky.R.002","DEF_pinky.R.003","DEF_pinky.R.004","DEF_pinky.R.004_end","DEF_ring.R.002","DEF_ring.R.003","DEF_ring.R.004","DEF_ring.R.004_end","DEF_thumb.R.001","DEF_thumb.R.002","DEF_thumb.R.003","DEF_thumb.R.003_end"
			// rappy
			"Armature","Hips","Penis.000","Penis.001","Penis.002","Skirt1_1.001","Skirt1_1","Skirt1_2","Skirt1_3","Skirt1_4","Skirt2_1_L.001","Skirt2_1_L","Skirt2_2_L","Skirt2_3_L","Skirt2_4_L","Skirt2_1_R.001","Skirt2_1_R","Skirt2_2_R","Skirt2_3_R","Skirt2_4_R","Skirt3_1_L.001","Skirt3_1_L","Skirt3_2_L","Skirt3_3_L","Skirt3_4_L","Skirt3_1_R.001","Skirt3_1_R","Skirt3_2_R","Skirt3_3_R","Skirt3_4_R","Skirt4_1_L.001","Skirt4_1_L","Skirt4_2_L","Skirt4_3_L","Skirt4_4_L","Skirt4_1_R.001","Skirt4_1_R","Skirt4_2_R","Skirt4_3_R","Skirt4_4_R","Skirt5_1_L.001","Skirt5_1_L","Skirt5_2_L","Skirt5_3_L","Skirt5_4_L","Skirt5_1_R.001","Skirt5_1_R","Skirt5_2_R","Skirt5_3_R","Skirt5_4_R","Skirt6_1_L.001","Skirt6_1_L","Skirt6_2_L","Skirt6_3_L","Skirt6_4_L","Skirt6_1_R.001","Skirt6_1_R","Skirt6_2_R","Skirt6_3_R","Skirt6_4_R","Skirt7_1.001","Skirt7_1","Skirt7_2","Skirt7_3","Skirt7_4","Spine","Chest","UpperChest","Breast.L","Breast.R","Neck","Head","Cheak.L","Cheak_shape.L","Cheak.R","Cheak_shape.R","Chin.L","Chin.R","Ctrl.lip.corner.L","Ctrl.lip.corner.R","Ctrl.lip.lower","lip.lower.L","lip.lower.R","Ctrl.lip.lower.L","lip.lower.L.001","Ctrl.lip.lower.R","lip.lower.R.001","Ctrl.lip.upper","lip.upper.L","lip.upper.R","Ctrl.lip.upper.L","lip.upper.L.001","Ctrl.lip.upper.R","lip.upper.R.001","ctrl.tongue.000","ctrl.tongue.001","ctrl.tongue.002","ear rings.L","ear rings.R","Eye.L","Eye.R","hair.center","hair.center.001","hair.center.002","hair.center.003","hair.center.L","hair.center.L.001","hair.center.L.002","hair.center.R","hair.center.R.001","hair.center.R.002","hair.hane","hair.hane.001","hair.hane.002","hair.side.L","hair.side.L.001","hair.side.L.002","hair.side.R","hair.side.R.001","hair.side.R.002","hair.twintail.L.001","hair.twintail.L.002","hair.twintail.L.003","hair.twintail.R.001","hair.twintail.R.002","hair.twintail.R.003","Jaw_shape.L","Jaw.L","Jaw_shape.R","Jaw.R","teeth.lower","teeth.upper","tongue.000","tongue.001","tongue.002","Shoulder.L","UpperArm.L","LowerArm.L","Hand.L","palm.01.L","f_index.01.L","f_index.02.L","f_index.03.L","thumb.01.L","thumb.02.L","thumb.03.L","palm.02.L","f_middle.01.L","f_middle.02.L","f_middle.03.L","palm.03.L","f_ring.01.L","f_ring.02.L","f_ring.03.L","palm.04.L","f_pinky.01.L","f_pinky.02.L","f_pinky.03.L","Shoulder.R","UpperArm.R","LowerArm.R","Hand.R","palm.01.R","f_index.01.R","f_index.02.R","f_index.03.R","thumb.01.R","thumb.02.R","thumb.03.R","palm.02.R","f_middle.01.R","f_middle.02.R","f_middle.03.R","palm.03.R","f_ring.01.R","f_ring.02.R","f_ring.03.R","palm.04.R","f_pinky.01.R","f_pinky.02.R","f_pinky.03.R","UpperLeg.L","LowerLeg.L","Foot.L","Toes.L","Toes.Index.L.001","Toes.Index.L.002","Toes.Middle.L.001","Toes.Middle.L.002","Toes.Pinky.L.001","Toes.Ring.L.001","Toes.Ring.L.002","Toes.Thumb.L.001","Toes.Thumb.L.002","UpperLeg.R","LowerLeg.R","Foot.R","Toes.R","Toes.Index.R.001","Toes.Index.R.002","Toes.Middle.R.001","Toes.Middle.R.002","Toes.Pinky.R.001","Toes.Ring.R.001","Toes.Ring.R.002","Toes.Thumb.R.001","Toes.Thumb.R.002","Waist.L","Waist.R","GameObject"
		};

		[MenuItem("Mod Engine/Mod Creator")]
		public static void Initialize()
		{
			var window = GetWindow(typeof(ModCreator), true, "Mod Creator");
			window.minSize = new UnityEngine.Vector2(700, 500);
			window.Show();

			LocalizationManager.Instance.LoadAllLanguages();

			var modCreator = (ModCreator)window;
			modCreator.AvailableLanguages = LocalizationManager.Instance.GetAvailableLanguages().ToArray();
			modCreator.SetLanguage("English");
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
					
					presetsScrollPosition = GUILayout.BeginScrollView(presetsScrollPosition, GUILayout.Height(150));

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
					
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_LANGUAGES"), EditorStyles.boldLabel);

					var prevLanguage = Language;
					Language = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_CURRENTLANG")}*", Language, AvailableLanguages);

					if (prevLanguage != Language)
						SetLanguage(AvailableLanguages[Language]);
					
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

								break;
							case ECharacterObjectType.Accessory:
								template.AccessoryType = (EAccessoryType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_ACCSLOT")}*", template.AccessoryType, "MODCREATOR_BASIC_ACCSLOT_");
								template.DefaultAccessoryParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultAccessoryParentIdx, defaultParents);
								template.DefaultAccessoryParent = defaultParents[template.DefaultAccessoryParentIdx];
								break;
							case ECharacterObjectType.Texture:
								template.TextureType = (ETextureType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_TEXTYPE")}*", template.TextureType, "MODCREATOR_BASIC_TEXTYPE_");

								if (template.TextureType is ETextureType.BodyOverlay or ETextureType.Nipples)
								{
									template.OverlayTarget = EOverlayTarget.Body;
									template.OverlayMode = EOverlayMode.FullTexture;
									template.IsOverlay = true;
								}
								else if (template.TextureType is ETextureType.FaceOverlay)
								{
									template.OverlayTarget = EOverlayTarget.Face;
									template.OverlayMode = EOverlayMode.FullTexture;
									template.IsOverlay = true;
								}
								else
								{
									template.IsOverlay = false;
								}
								
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
									template.DefaultAccessoryParent = acc.DefaultParent;

									for (var i = 0; i < defaultParents.Length; i++)
									{
										if (defaultParents[i] != acc.DefaultParent) 
											continue;
										
										template.DefaultAccessoryParentIdx = i;
										break;
									}
									
									break;
								case ITexture tex:
									template.TextureType = tex.TextureType;
									template.Texture = tex.Texture;
									template.IsOverlay = tex.IsOverlay;
									template.OverlayTarget = tex.OverlayTarget;
									template.OverlayMode = tex.OverlayMode;
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
							acc.DefaultParent = template.DefaultAccessoryParent;
							break;
						case ITexture tex:
							tex.TextureType = template.TextureType;
							tex.Texture = template.Texture;
							tex.IsOverlay = template.IsOverlay;
							tex.OverlayTarget = template.OverlayTarget;
							tex.OverlayMode = template.OverlayMode;
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

				switch (template.TemplateType)
				{
					case ETemplateType.CharacterObject:
					{
						var characterObject = ((GameObject)Prefabs[i]).GetComponent<ICharacterObject>();
						characterObject.Renderers = characterObject.GetGameObject().GetComponentsInChildren<Renderer>();
						break;
					}
					case ETemplateType.StudioObject:
					{
						var studioObject = ((GameObject)Prefabs[i]).GetComponent<IStudioObject>();
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
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Events;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.Character.CharacterObjects;

using Code.Frameworks.Studio;
using Code.Frameworks.Studio.Enums;
using Code.Frameworks.Studio.Events;
using Code.Frameworks.Studio.Interfaces;
using Code.Frameworks.Studio.StudioObjects;

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

		#region presets

		public void SavePreset(string presetName)
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

			foreach (var go in preset.Prefabs.Cast<GameObject>())
				go.transform.SetParent(root.transform);
			
			var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(ModCreatorPath, $"Code/Editor/ModEngine/Presets/{presetName}.prefab"));
			
			PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
			
			foreach (var go in preset.Prefabs.Cast<GameObject>())
				go.transform.SetParent(null);
			 
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

			foreach (var go in preset.Prefabs.Cast<GameObject>())
				go.transform.SetParent(null);

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
							if (clothingObject.GetStateObject(EClothingState.Full) == null)
							{
								pass = false;
								Debug.LogWarning($"Full state missing for {template.Name}");
							}
					
							if (clothingObject.GetStateObject(EClothingState.Half) == null)
							{
								Debug.LogWarning($"Optional half state missing for {template.Name}");
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
					}
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

		private void verticalList(List<string> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle);
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
			GUILayout.Label(labelTitle);
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
			{
				list.Add(new Object());
				Templates.Add(new Template { TemplateType = type });
				
				CurrentTemplate = -1;
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

		private string[] verticalList(string[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle, EditorStyles.boldLabel);
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
					if (defaultParents[i] == template.DefaultAccessoryParent)
					{
						template.DefaultAccessoryParentIdx = i;
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