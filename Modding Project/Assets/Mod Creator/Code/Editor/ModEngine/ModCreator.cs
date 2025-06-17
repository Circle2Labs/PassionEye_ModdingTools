//#define MODCREATOR_VERBOSE
#define MODCREATOR_STANDALONE

using System;
using System.Collections.Generic;
using System.Linq;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Structs;
using Railgun.AssetPipeline.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Editor.ModEngine
{
    public partial class ModCreator : EditorWindow
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
						SetupPresets();
					
					stopPreview();
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
		public const bool IsStandalone = true;
		public const string ModCreatorPath = "Assets/Mod Creator/";
#else
		public const bool IsStandalone = false;
		public const string ModCreatorPath = "Assets/";
#endif
		
#if MODCREATOR_VERBOSE
		public const bool IsVerbose = true;
#else
		public const bool IsVerbose = false;
#endif

		public const string Version = "v0.1.10.0";
		public const string EditorVersion = "2022.3.53f1";
		
		[SerializeField]
		public Manifest Manifest = new () {Name = "", Author = "", Version = "1.0.0"};
		
		[SerializeField]
		public List<Object> Prefabs = new ();
		
		[SerializeField]
		public List<Template> Templates = new ();
		
		private const string nameTypeFilter = @"(^[0-9])|([^a-zA-Z0-9_])";

		[MenuItem("Mod Engine/Mod Creator")]
		public static void Initialize()
		{
            var window = (ModCreator)GetWindow(typeof(ModCreator), true, $"Mod Creator {Version}");
			window.minSize = new UnityEngine.Vector2(700, 500);
			window.Show();
			window.InitializeLanguages();
			window.InitializeBaseMeshes();
		}
		
		public void OnGUI()
		{
			if (Application.unityVersion != EditorVersion)
			{
				EditorGUILayout.LabelField("Unsupported Unity Editor Version", EditorStyles.boldLabel);

				GUILayout.Space(8);
				
				EditorGUILayout.LabelField($"This version of Mod Creator expects Unity {EditorVersion}");
				EditorGUILayout.LabelField($"However the current version is Unity {Application.unityVersion}");

				GUILayout.Space(8);
				
				EditorGUILayout.LabelField("Please upgrade your project to the correct Unity version");
				return;
			}
			
			if (CurrentTemplate != -1 && Templates.Count > CurrentTemplate)
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
			else
			{
				CurrentTab = GUILayout.Toolbar(CurrentTab, new[]
				{
					GetLocalizedString("MODCREATOR_TABS_PRESETLANGUAGE"),
					GetLocalizedString("MODCREATOR_TABS_BUILDER")
				});
			}

			switch (CurrentTab)
			{
				case 0:
				{
					DrawPresetsLanguages();
					break;
				}
				case 1:
				{
					DrawBuilder();
					break;
				}
				case 2:
				{
					DrawBasic();
					break;
				}
				case 3:
				{
					DrawAdvanced();
					break;
				}
			}
		}
		
		public void OnDestroy()
		{
			stopPreview();
			SavePreset("_previous_state_", true);
			UnloadBaseMeshes();
		}
	}
}