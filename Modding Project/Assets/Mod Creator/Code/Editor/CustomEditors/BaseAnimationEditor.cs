using System;
using System.Collections.Generic;
using System.Linq;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation;
using Code.Frameworks.Animation.Structs;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Enums;
using UnityEditor;
using UnityEngine;
using Tuple = Code.Tools.Tuple;

namespace Code.Editor.CustomEditors
{
    [CustomEditor(typeof(BaseAnimation), true)]
	public class BaseAnimationEditor : UnityEditor.Editor
	{
		public BaseAnimation BaseAnimation;

		[SerializeField]
		public bool DeveloperMode;
		
		public void OnEnable()
		{
			BaseAnimation = (BaseAnimation)target;
		}

		public override void OnInspectorGUI()
		{
			if (DeveloperMode)
			{
				base.OnInspectorGUI();
			}
			else
			{
				EditorGUILayout.LabelField("Component Configuration", EditorStyles.boldLabel);
				BaseAnimation.UsageFlags = (EAnimationUsageFlags)EditorGUILayout.EnumFlagsField("Usage", BaseAnimation.UsageFlags);
				BaseAnimation.FadeDuration = EditorGUILayout.FloatField("Fade Duration", BaseAnimation.FadeDuration);
			
				GUILayout.Space(5);
			
				BaseAnimation.ClipContainers ??= Array.Empty<SClipContainer>();
				BaseAnimation.ClipContainers = verticalList(BaseAnimation.ClipContainers, "Containers");

				if (BaseAnimation is HAnimation hAnimation)
				{
					GUILayout.Space(5);
					
					hAnimation.Type = (EHAnimationType)EditorGUILayout.EnumPopup("Type", hAnimation.Type);
					hAnimation.SupportedClimaxTypes = (ESupportedClimaxTypesFlags)EditorGUILayout.EnumFlagsField("Supported Climax Types", hAnimation.SupportedClimaxTypes);
					
					GUILayout.Space(5);
					
					hAnimation.ArouseActive = EditorGUILayout.Toggle("Arouse Active Member", hAnimation.ArouseActive);
					hAnimation.ArousePassive = EditorGUILayout.Toggle("Arouse Passive Member", hAnimation.ArousePassive);
					
					GUILayout.Space(5);
					
					hAnimation.ArousalMultiplier = EditorGUILayout.FloatField("Arousal Multiplier", hAnimation.ArousalMultiplier);
					hAnimation.VerticalCameraOffset = EditorGUILayout.FloatField("Vertical Camera Offset", hAnimation.VerticalCameraOffset);
					
					GUILayout.Space(5);

					if (hAnimation.IdleClips == null || hAnimation.IdleClips.Length != hAnimation.ClipContainers.Length)
					{
						var idleClips = hAnimation.IdleClips;
						Array.Resize(ref idleClips, hAnimation.ClipContainers.Length);
						hAnimation.IdleClips = idleClips;
					}
					
					hAnimation.IdleClips = verticalList(hAnimation.IdleClips, "H Idle Animations", true);

					GUILayout.Space(5);
					
					if (hAnimation.NonClimaxClips == null || hAnimation.NonClimaxClips.Length != hAnimation.ClipContainers.Length)
					{
						var nonClimaxClips = hAnimation.NonClimaxClips;
						Array.Resize(ref nonClimaxClips, hAnimation.ClipContainers.Length);
						hAnimation.NonClimaxClips = nonClimaxClips;
					}
					
					hAnimation.NonClimaxClips = verticalList(hAnimation.NonClimaxClips, "H Non-Climax Animations", true);

					GUILayout.Space(5);

					var climaxTypes = new List<EClimaxType>();
					
					foreach (Enum supportedClimaxType in Enum.GetValues(typeof(ESupportedClimaxTypesFlags)))
					{
						if (!hAnimation.SupportedClimaxTypes.HasFlag(supportedClimaxType))
							continue;

						var typeInt = Convert.ToInt32(supportedClimaxType);
						
						// None is always set, skip it
						if (typeInt == 0)
							continue;
					
						climaxTypes.Add((EClimaxType)typeInt);
					}

					if (hAnimation.ClimaxClips == null || hAnimation.ClimaxClips.Length != climaxTypes.Count)
					{
						var climaxClips = hAnimation.ClimaxClips;
						Array.Resize(ref climaxClips, climaxTypes.Count);
						hAnimation.ClimaxClips = climaxClips;
					}

					for (var i = 0; i < climaxTypes.Count; i++)
					{
						var tuple = hAnimation.ClimaxClips[i];
						
						if (tuple.Item2 == null || tuple.Item2.Length != hAnimation.ClipContainers.Length)
							Array.Resize(ref tuple.Item2, hAnimation.ClipContainers.Length);
						
						var climaxType = climaxTypes[i];
						
						tuple.Item1 = climaxType;
						tuple.Item2 = verticalList(tuple.Item2, $"H Climax {climaxType} Animations");
						hAnimation.ClimaxClips[i] = tuple;
					}
				}
				
				EditorUtility.SetDirty(BaseAnimation);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(BaseAnimation.gameObject.scene);
			}
			
			GUILayout.Space(10);
			
			DeveloperMode = EditorGUILayout.Toggle("Developer Mode", DeveloperMode);
			
			serializedObject.ApplyModifiedProperties();
		}
		
		private string itemsCount(int count)
		{
			return $" ({count} item{(count == 1 ? "" : "s")})";
		}
		
		private SClimaxAnimation[] verticalList(SClimaxAnimation[] array, string labelTitle, bool nonClimax = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			GUILayout.EndHorizontal();

			var prefix = nonClimax ? "Non-" : "";
			var postfix = nonClimax ? "" : "*";
			
			for (var i = 0; i < array.Length; i++)
			{
				var element = array[i];
				
				element.Transition = (AnimationClip)EditorGUILayout.ObjectField("Transition", element.Transition, typeof(AnimationClip), false);
				element.Climax = (AnimationClip)EditorGUILayout.ObjectField($"{prefix}Climax{postfix}", element.Climax, typeof(AnimationClip), false);
				element.Idle = (AnimationClip)EditorGUILayout.ObjectField("Idle", element.Idle, typeof(AnimationClip), false);
				
				array[i] = element;
				
				GUILayout.Space(5);
			}

			return array;
		}
		
		private AnimationClip[] verticalList(AnimationClip[] array, string labelTitle, bool modifyOnly = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (!modifyOnly)
			{
				if (GUILayout.Button("Add", GUILayout.Width(50)))
					array = array.Append(null).ToArray();
				if (GUILayout.Button("Clear", GUILayout.Width(50)))
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
		
		private SClipContainer[] verticalList(SClipContainer[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			
			if (GUILayout.Button("Add", GUILayout.Width(50)))
				array = array.Append(new SClipContainer()).ToArray();
			if (GUILayout.Button("Clear", GUILayout.Width(50)))
				array = Array.Empty<SClipContainer>();
			
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginVertical();
				
				GUILayout.BeginHorizontal();

				var container = array[i];
				container.ClipUsageFlags = (EClipUsageFlags)EditorGUILayout.EnumFlagsField("Usage", container.ClipUsageFlags);

				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					array = array.Where((_, k) => i != k).ToArray();
					break;
				}
				
				GUILayout.EndHorizontal();
				
				container.ClipContainerType = (EClipContainerType)EditorGUILayout.EnumPopup("Container Type", container.ClipContainerType);
				
				container.Clips ??= Array.Empty<AnimationClip>();
				container.Clips = verticalList(container.Clips, "Clips");

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
						EditorGUILayout.LabelField("Linear", EditorStyles.boldLabel);
						container.ParameterName = EditorGUILayout.TextField("Parameter Name", container.ParameterName);
						container.ParameterInitialValue = EditorGUILayout.FloatField("Parameter Initial Value", container.ParameterInitialValue);
					
						EditorGUILayout.LabelField("Thresholds", EditorStyles.boldLabel);
						for (var k = 0; k < container.Thresholds.Length; k++)
							container.Thresholds[k] = EditorGUILayout.FloatField("", container.Thresholds[k]);
					}
					else if (container.ClipContainerType == EClipContainerType.TwoDimensional)
					{
						EditorGUILayout.LabelField("2D", EditorStyles.boldLabel);
						container.ParameterXName = EditorGUILayout.TextField("Parameter X Name", container.ParameterXName);
						container.ParameterYName = EditorGUILayout.TextField("Parameter Y Name", container.ParameterYName);
						container.ParameterXYInitialValue = EditorGUILayout.Vector2Field("Parameter Initial Value", container.ParameterXYInitialValue);
					
						EditorGUILayout.LabelField("Positions", EditorStyles.boldLabel);
						for (var k = 0; k < container.Positions.Length; k++)
							container.Positions[k] = EditorGUILayout.Vector2Field("", container.Positions[k]);
					}
				}
				
				GUILayout.EndVertical();
				
				array[i] = container;
			}

			return array;
		}
	}
}