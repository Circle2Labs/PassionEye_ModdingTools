using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Studio.Interfaces;
using Code.Managers;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator 
	{
		[SerializeField]
		public List<string> Presets = new ();
		
		[SerializeField]
		public string SavePresetName = "Preset Name";

		[SerializeField] 
		public string[] AvailableLanguages = {"English"};
		
		[SerializeField]
		public int Language;

		private Vector2 presetsLanguagesScrollPosition;

		public void DrawPresetsLanguages()
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
					
			presetsLanguagesScrollPosition = GUILayout.BeginScrollView(presetsLanguagesScrollPosition);

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
			
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_BUILDEVERYTHING")))
			{
				foreach (var preset in Presets)
				{
					if (preset == "_previous_state_")
						continue;
					
					ResetState();
					LoadPreset(preset);
					AssignComponents();
					Build();
				}
			}
		}

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
			RemoveBrokenComponents();
			RemoveGameComponents();
			
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
		
		public void InitializeLanguages()
		{
			LocalizationManager.Instance.LoadAllLanguages();

			AvailableLanguages = LocalizationManager.Instance.GetAvailableLanguages().ToArray();
			SetLanguage("English");
		}
		
		public void SetLanguage(string language)
		{
			for (var i = 0; i < AvailableLanguages.Length; i++)
			{
				if (AvailableLanguages[i] != language) 
					continue;
				
				Language = i;
			}
		}

		public void RemoveGameComponents()
		{
			foreach (var prefab in Prefabs)
			{
				if (prefab == null || prefab is not GameObject gameObject)
					return;

				var charaObject = gameObject.GetComponent<ICharacterObject>();
				if (charaObject != null)
				{
					Debug.LogWarning($"Removing game component {charaObject}");
					DestroyImmediate((Component)charaObject);
				}
				
				var studioObject = gameObject.GetComponent<IStudioObject>();
				if (studioObject != null)
				{
					Debug.LogWarning($"Removing game component {studioObject}");
					DestroyImmediate((Component)studioObject);
				}
			}
		}
		
		public void FixNonexistentStates()
		{
			foreach (var template in Templates)
				template.SetupStates();
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
	}
}