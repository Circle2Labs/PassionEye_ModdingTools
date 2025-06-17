using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Structs;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Structs;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator
	{
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

		public Enum LocalizedEnumPopup(string label, Enum selected, string localizationRoot, params GUILayoutOption[] options)
		{
			var names = Enum.GetNames(selected.GetType());
			for (var i = 0; i < names.Length; i++)
				names[i] = GetLocalizedString(localizationRoot + names[i].ToUpper());
			
			var popup = EditorGUILayout.Popup(label, Convert.ToInt32(selected), names, options);
			return (Enum)Enum.ToObject(selected.GetType(), popup);
		}
		
		public GUIStyle GetBackgroundStyle(Color color)
		{
			Texture.SetPixel(0, 0, color);
			Texture.Apply();
			
			GUIStyle.normal.background = Texture;
			return GUIStyle;
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
					Templates.RemoveAt(current);
					list.RemoveAt(current);
					
					CurrentTemplate = -1;
					return;
				}
				GUILayout.EndHorizontal();
			}
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
		
		private void verticalList(List<SBlendShape> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(new SBlendShape());
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginVertical();
				
				var blendshape = list[i];
				blendshape.Blendshapes ??= new string[1];
				blendshape.NSFWRange ??= new [] {-100, 100};
				blendshape.SFWRange ??= new [] {-100, 100};
				
				GUILayout.BeginHorizontal();
				blendshape.Title = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_NAME"), blendshape.Title);
				blendshape.IsNSFW = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_ISNSFW"), blendshape.IsNSFW, GUILayout.Width(100));
				blendshape.FutaExclusive = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_SHOWFUTA"), blendshape.FutaExclusive, GUILayout.Width(200));

				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					list.RemoveAt(i);
					return;
				}
				
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				blendshape.FaceCategory = (EFaceBlendShapeCategory)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_FACE_SHAPECAT"), blendshape.FaceCategory, "MODCREATOR_BASIC_FACE_SHAPECAT_");
				blendshape.BodyCategory = (EBodyBlendShapeCategory)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_BODY_SHAPECAT"), blendshape.BodyCategory, "MODCREATOR_BASIC_BODY_SHAPECAT_");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label(GetLocalizedString("MODCREATOR_BASIC_SFWRANGE"), GUILayout.Width(147));
				blendshape.SFWRange[0] = EditorGUILayout.IntField(blendshape.SFWRange[0]);
				blendshape.SFWRange[1] = EditorGUILayout.IntField(blendshape.SFWRange[1]);
				GUILayout.Label(GetLocalizedString("MODCREATOR_BASIC_NSFWRANGE"), GUILayout.Width(147));
				blendshape.NSFWRange[0] = EditorGUILayout.IntField(blendshape.NSFWRange[0]);
				blendshape.NSFWRange[1] = EditorGUILayout.IntField(blendshape.NSFWRange[1]);
				GUILayout.EndHorizontal();
				
				blendshape.Blendshapes = verticalList(blendshape.Blendshapes, GetLocalizedString("MODCREATOR_BASIC_BLENDSHAPES"), 2); 
				
				if (blendshape.Blendshapes.Length == 0)
					blendshape.Blendshapes = new string[1];
				
				list[i] = blendshape;
				GUILayout.EndVertical();
				
				if (i != list.Count - 1)
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			}
		}
		
		private void verticalList(List<SBlendshapePreset> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(new SBlendshapePreset());
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginVertical();
				
				var preset = list[i];
				preset.Blendshapes ??= new List<Tools.Tuple.SerializableTuple<string, float>>();
				preset.FaceCategories ??= new List<EFaceBlendShapeCategory>();
				preset.BodyCategories ??= new List<EBodyBlendShapeCategory>();
				
				GUILayout.BeginHorizontal();
				preset.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_NAME"), preset.Name);
				preset.IsNSFW = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_ISNSFW"), preset.IsNSFW, GUILayout.Width(100));
				preset.FutaExclusive = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_SHOWFUTA"), preset.FutaExclusive, GUILayout.Width(200));

				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					list.RemoveAt(i);
					return;
				}
				
				GUILayout.EndHorizontal();

				verticalList(preset.FaceCategories, $"{GetLocalizedString("MODCREATOR_BASIC_FACE_SHAPECAT")}*", false);
				verticalList(preset.BodyCategories, $"{GetLocalizedString("MODCREATOR_BASIC_BODY_SHAPECAT")}*", false);
				verticalList(preset.Blendshapes, $"{GetLocalizedString("MODCREATOR_BASIC_BLENDSHAPES")}*");
				
				list[i] = preset;
				GUILayout.EndVertical();
				
				if (i != list.Count - 1)
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			}
		}
		
		private void verticalList(List<Tools.Tuple.SerializableTuple<Transform, string>> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(new Tools.Tuple.SerializableTuple<Transform, string>(null, ""));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				
				var tuple = list[i];
				tuple.Item1 = (Transform)EditorGUILayout.ObjectField("", tuple.Item1, typeof(Transform), true);
				tuple.Item2 = EditorGUILayout.TextField("", tuple.Item2);
				list[i] = tuple;
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<SBodyPart> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(new SBodyPart());
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				
				var bodyPart = list[i];
				bodyPart.Type = (EBodyPartType)LocalizedEnumPopup("", bodyPart.Type, "MODCREATOR_BASIC_BODYPARTS_");
				bodyPart.Bone = (Transform)EditorGUILayout.ObjectField("", bodyPart.Bone, typeof(Transform), true);

				list[i] = bodyPart;
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
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
		
		private void verticalList(List<SkinnedMeshRenderer> list, string labelTitle)
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
				
				list[i] = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("", list[i], typeof(SkinnedMeshRenderer), true);
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<Collider> list, string labelTitle)
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
				
				list[i] = (Collider)EditorGUILayout.ObjectField("", list[i], typeof(Collider), true);
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<EFaceBlendShapeCategory> list, string labelTitle, bool individualTitles = true)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(EFaceBlendShapeCategory.None);
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				
				list[i] = (EFaceBlendShapeCategory)LocalizedEnumPopup(individualTitles ? GetLocalizedString("MODCREATOR_BASIC_FACE_SHAPECAT") : "", list[i], "MODCREATOR_BASIC_FACE_SHAPECAT_");
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<EBodyBlendShapeCategory> list, string labelTitle, bool individualTitles = true)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(EBodyBlendShapeCategory.None);
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				
				list[i] = (EBodyBlendShapeCategory)LocalizedEnumPopup(individualTitles ? GetLocalizedString("MODCREATOR_BASIC_BODY_SHAPECAT") : "", list[i], "MODCREATOR_BASIC_BODY_SHAPECAT_");
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<Tools.Tuple.SerializableTuple<string, float>> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add(new Tools.Tuple.SerializableTuple<string, float>("", 0f));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				
				var tuple = list[i];
				tuple.Item1 = EditorGUILayout.TextField("", tuple.Item1);
				tuple.Item2 = EditorGUILayout.FloatField("", tuple.Item2);
				list[i] = tuple;
				
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
		
		private SClipContainer[] verticalList(SClipContainer[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				array = array.Append(new SClipContainer()).ToArray();
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				array = Array.Empty<SClipContainer>();
			
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginVertical();
				
				GUILayout.BeginHorizontal();

				var container = array[i];
				container.ClipUsageFlags = (EClipUsageFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_USAGE")}*", container.ClipUsageFlags);

				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					array = array.Where((_, k) => i != k).ToArray();
					break;
				}
				
				GUILayout.EndHorizontal();
				
				container.ClipContainerType = (EClipContainerType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_CONTAINERTYPE")}*", container.ClipContainerType, "MODCREATOR_BASIC_CONTAINERTYPE_");
				
				container.Clips ??= Array.Empty<AnimationClip>();
				container.Clips = verticalList(container.Clips, $"{GetLocalizedString("MODCREATOR_BASIC_CLIPS")}*");

				if (container.ClipContainerType == EClipContainerType.Single)
				{
					if (container.Clips.Length == 0 || container.Clips.Length > 1)
						Array.Resize(ref container.Clips, 1);
				}
				else
				{
					if (container.Clips.Length == 0 || container.Clips.Length < 2)
						Array.Resize(ref container.Clips, 2);
				}
				
				if (container.Clips.Length > 1)
				{
					GUILayout.Space(5);
					
					container.Thresholds ??= Array.Empty<float>();

					if (container.Thresholds.Length != container.Clips.Length)
						Array.Resize(ref container.Thresholds, container.Clips.Length);

					container.Positions ??= Array.Empty<Vector2>();

					if (container.Positions.Length != container.Clips.Length)
						Array.Resize(ref container.Positions, container.Clips.Length);

					if (container.ClipContainerType == EClipContainerType.Linear)
					{
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CONTAINERTYPE_LINEAR"), EditorStyles.boldLabel);
						GUILayout.BeginHorizontal();
						container.ParameterName = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_PARAMNAME")}*", container.ParameterName);
						container.ParameterInitialValue = EditorGUILayout.FloatField($"{GetLocalizedString("MODCREATOR_BASIC_PARAMINITVALUE")}*", container.ParameterInitialValue);
						GUILayout.EndHorizontal();
					
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_THRESHOLDS"), EditorStyles.boldLabel);
						for (var k = 0; k < container.Thresholds.Length; k++)
							container.Thresholds[k] = EditorGUILayout.FloatField("", container.Thresholds[k]);
					}
					else if (container.ClipContainerType == EClipContainerType.TwoDimensional)
					{
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CONTAINERTYPE_TWODIMENSIONAL"), EditorStyles.boldLabel);
						GUILayout.BeginHorizontal();
						container.ParameterXName = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_PARAMXNAME")}*", container.ParameterXName);
						container.ParameterYName = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_PARAMYNAME")}*", container.ParameterYName);
						GUILayout.EndHorizontal();
						container.ParameterXYInitialValue = EditorGUILayout.Vector2Field($"{GetLocalizedString("MODCREATOR_BASIC_PARAMINITVALUE")}*", container.ParameterXYInitialValue);
					
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_POSITIONS"), EditorStyles.boldLabel);
						for (var k = 0; k < container.Positions.Length; k++)
							container.Positions[k] = EditorGUILayout.Vector2Field("", container.Positions[k]);
					}
				}
				
				GUILayout.EndVertical();
				
				array[i] = container;
			}

			return array;
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
		
		private string[] verticalList(string[] array, string labelTitle, int maxLength = -1)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			GUI.enabled = maxLength == -1 || array.Length < maxLength;
			var add = GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50));
			GUI.enabled = true;
			if (add)
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
		
		private SCompatibleBaseMesh[] verticalList(SCompatibleBaseMesh[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				array = array.Append(new SCompatibleBaseMesh()).ToArray();
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				array = Array.Empty<SCompatibleBaseMesh>();
			GUILayout.EndHorizontal();

			var availableBaseMeshes = GetBaseMeshes();
			var baseMeshNames = new List<string>();
			
			foreach (var availableBaseMesh in availableBaseMeshes)
				baseMeshNames.Add($"{availableBaseMesh.Item1}.{availableBaseMesh.Item2}");
			
			for (var i = 0; i < array.Length; i++)
			{
				var compatibleBaseMesh = array[i];

				var selected = -1;
				for (var k = 0; k < availableBaseMeshes.Count; k++)
				{
					var availableBaseMesh = availableBaseMeshes[k];
					
					if (compatibleBaseMesh.GUID != availableBaseMesh.Item1 || compatibleBaseMesh.ID != availableBaseMesh.Item2)
						continue;

					selected = k;
					break;
				}

				selected = EditorGUILayout.Popup("", selected, baseMeshNames.ToArray());
				if (selected != -1)
				{
					compatibleBaseMesh.GUID = availableBaseMeshes[selected].Item1;
					compatibleBaseMesh.ID = availableBaseMeshes[selected].Item2;
				}
				
				GUILayout.BeginHorizontal();
				
				compatibleBaseMesh.GUID = EditorGUILayout.TextField("GUID", compatibleBaseMesh.GUID);
				EditorGUIUtility.labelWidth = 50;
				compatibleBaseMesh.ID = (byte)EditorGUILayout.IntField("ID", compatibleBaseMesh.ID, GUILayout.Width(100));
				EditorGUIUtility.labelWidth = 0;
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					var list = array.ToList();
					list.RemoveAt(i);
					
					array = list.ToArray();
					break;
				}
				GUILayout.EndHorizontal();
				
				array[i] = compatibleBaseMesh;
				
				if (i != array.Length - 1)
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			}

			return array;
		}
		
		private SClimaxAnimation[] verticalList(SClimaxAnimation[] array, string labelTitle, bool nonClimax = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			GUILayout.EndHorizontal();
			
			var prefix = nonClimax ? "NON" : "";
			var postfix = nonClimax ? "" : "*";

			for (var i = 0; i < array.Length; i++)
			{
				var element = array[i];
				
				element.Transition = (AnimationClip)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_CLIPS_HTRANSITION"), element.Transition, typeof(AnimationClip), false);
				element.Climax = (AnimationClip)EditorGUILayout.ObjectField($"{GetLocalizedString($"MODCREATOR_BASIC_CLIPS_H{prefix}CLIMAX")}{postfix}", element.Climax, typeof(AnimationClip), false);
				element.Idle = (AnimationClip)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_CLIPS_HIDLE"), element.Idle, typeof(AnimationClip), false);
				
				array[i] = element;
				
				GUILayout.Space(5);
			}

			return array;
		}
		
		private bool[] verticalList(bool[] array, string labelTitle, bool modifyOnly = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (!modifyOnly)
			{
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
					array = array.Append(false).ToArray();
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
					array = Array.Empty<bool>();
			}
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginHorizontal();
				array[i] = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_CONTAINER")} {i}", array[i]);

				if (!modifyOnly)
				{
					if (GUILayout.Button("-", GUILayout.Width(25)))
						array = array.Where((_, k) => i != k).ToArray();
				}
				
				GUILayout.EndHorizontal();
			}

			return array;
		}
		
		private AnimationClip[] verticalList(AnimationClip[] array, string labelTitle, bool modifyOnly = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (!modifyOnly)
			{
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
					array = array.Append(null).ToArray();
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
					array = Array.Empty<AnimationClip>();
			}
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginHorizontal();
				array[i] = (AnimationClip)EditorGUILayout.ObjectField("", array[i], typeof(AnimationClip), false);

				if (!modifyOnly)
				{
					if (GUILayout.Button("-", GUILayout.Width(25)))
						array = array.Where((_, k) => i != k).ToArray();
				}
				
				GUILayout.EndHorizontal();
			}

			return array;
		}
	}
}