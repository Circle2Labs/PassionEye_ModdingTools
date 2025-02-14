using System;
using System.Linq;
using Assets.Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Structs;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.CustomEditors
{
    [CustomEditor(typeof(BaseAnimation))]
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
				EditorGUI.BeginChangeCheck();
			
				EditorGUILayout.LabelField("Component Configuration", EditorStyles.boldLabel);
				BaseAnimation.UsageFlags = (EAnimationUsageFlags)EditorGUILayout.EnumFlagsField("Usage", BaseAnimation.UsageFlags);
				BaseAnimation.FadeDuration = EditorGUILayout.FloatField("Fade Duration", BaseAnimation.FadeDuration);
			
				GUILayout.Space(5);
			
				BaseAnimation.ClipContainers ??= Array.Empty<SClipContainer>();
				BaseAnimation.ClipContainers = verticalList(BaseAnimation.ClipContainers, "Containers");

				if (EditorGUI.EndChangeCheck())
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
		
		private AnimationClip[] verticalList(AnimationClip[] array, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (GUILayout.Button("Add", GUILayout.Width(50)))
				array = array.Append(null).ToArray();
			if (GUILayout.Button("Clear", GUILayout.Width(50)))
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
					if (container.Clips.Length == 0)
						container.Clips = new AnimationClip[1];
					else if (container.Clips.Length > 1)
						container.Clips = new[] { container.Clips[0] };
				}
				else
				{
					if (container.Clips.Length == 0)
						container.Clips = new AnimationClip[2];
					else if (container.Clips.Length < 2)
						container.Clips = new[] { container.Clips[0], null };
				}
				
				if (container.Clips.Length > 1)
				{
					GUILayout.Space(5);
					
					container.Thresholds ??= Array.Empty<float>();

					if (container.Thresholds.Length != container.Clips.Length)
						container.Thresholds = new float[container.Clips.Length];

					container.Positions ??= Array.Empty<Vector2>();

					if (container.Positions.Length != container.Clips.Length)
						container.Positions = new Vector2[container.Clips.Length];

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