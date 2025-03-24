//#define MODCREATOR_VERBOSE
#define MODCREATOR_STANDALONE

using System;
using System.Collections.Generic;
using System.Linq;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.Enums;
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

		public const string Version = "v0.1.8.1";
		
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
		}
		
		public void OnGUI()
		{
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
		}
		
		public void FixAccessoryParents()
		{
			foreach (var template in Templates.Where(template => template.TemplateType == ETemplateType.CharacterObject).Where(template => template.CharacterObjectType == ECharacterObjectType.Accessory))
				for (var i = 0; i < defaultParents.Length; i++)
					if (defaultParents[i] == template.DefaultParent)
						template.DefaultParentIdx = i;
		}
		
		public void RemoveBrokenComponents()
        {
        	foreach (var prefab in Prefabs)
        	{
        		if (prefab == null || prefab is not GameObject gameObject)
        			return;

        		var transforms = gameObject.GetComponentsInChildren<Transform>(true);
        		foreach (var transform in transforms)
        		{
        			var removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
        			if (removed > 0)
        				Debug.LogWarning($"Removed {removed} broken scripts from {prefab}");
        		}
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

		private AnimationClip[] verticalList(AnimationClip[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				array = array.Append(null).ToArray();
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				array = Array.Empty<AnimationClip>();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginHorizontal();
				array[i] = (AnimationClip)EditorGUILayout.ObjectField("", array[i], typeof(AnimationClip), false);
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					array = array.Where((_, k) => i != k).ToArray();
				
				GUILayout.EndHorizontal();
			}

			return array;
		}

		private string itemsCount(int count)
		{
			return $" ({count} item{(count == 1 ? "" : "s")})";
		}
	}
}