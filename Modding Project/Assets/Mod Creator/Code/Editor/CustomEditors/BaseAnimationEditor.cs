using System;
using System.Collections.Generic;
using System.Linq;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation;
using Code.Frameworks.Animation.Structs;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.H.Enums;
using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

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
					
					hAnimation.ExclusiveTags = verticalList(hAnimation.ExclusiveTags, "Exclusive Tags");
					hAnimation.ObjectAnimationTag = EditorGUILayout.TextField("Object Animation Tag", hAnimation.ObjectAnimationTag);
					
					GUILayout.Space(5);
					
					hAnimation.ArouseActive = EditorGUILayout.Toggle("Arouse Active Member", hAnimation.ArouseActive);
					hAnimation.ArousePassive = EditorGUILayout.Toggle("Arouse Passive Member", hAnimation.ArousePassive);
					
					GUILayout.Space(5);
					
					hAnimation.ArousalMultiplier = EditorGUILayout.FloatField("Arousal Multiplier", hAnimation.ArousalMultiplier);
					
					GUILayout.Space(5);

					hAnimation.CameraPositionOffset = EditorGUILayout.Vector3Field("Camera Position Offset", hAnimation.CameraPositionOffset);
					hAnimation.CameraAnglesOffset = EditorGUILayout.Vector3Field("Camera Angles Offset", hAnimation.CameraAnglesOffset);
					hAnimation.CameraDistance = EditorGUILayout.FloatField("Camera Distance", hAnimation.CameraDistance);
					
					GUILayout.Space(5);
					
					if (hAnimation.RaycastDown == null || hAnimation.RaycastDown.Length != hAnimation.ClipContainers.Length)
					{
						var raycastDown = hAnimation.RaycastDown;
						Array.Resize(ref raycastDown, hAnimation.ClipContainers.Length);
						hAnimation.RaycastDown = raycastDown;
					}
					
					hAnimation.RaycastDown = verticalList(hAnimation.RaycastDown, "Raycast Down", true);

					GUILayout.Space(5);
					
					if (hAnimation.LookAtPartner == null || hAnimation.LookAtPartner.Length != hAnimation.ClipContainers.Length)
					{
						var lookAtPartner = hAnimation.LookAtPartner;
						Array.Resize(ref lookAtPartner, hAnimation.ClipContainers.Length);
						hAnimation.LookAtPartner = lookAtPartner;
					}
					
					hAnimation.LookAtPartner = verticalList(hAnimation.LookAtPartner, "Look at Partner", true);

					GUILayout.Space(5);

					if (hAnimation.IdleDefinitions == null || hAnimation.IdleDefinitions.Length != hAnimation.ClipContainers.Length)
					{
						var idleDefinitions = hAnimation.IdleDefinitions;
						Array.Resize(ref idleDefinitions, hAnimation.ClipContainers.Length);
						hAnimation.IdleDefinitions = idleDefinitions;
					}
					
					hAnimation.IdleDefinitions = verticalList(hAnimation.IdleDefinitions, "H Idle Animations", true);

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
				
				element.TransitionDefinition = drawClipDefinition(element.TransitionDefinition, "Transition");
				element.ClimaxDefinition = drawClipDefinition(element.ClimaxDefinition, $"{prefix}Climax{postfix}");
				element.IdleDefinition = drawClipDefinition(element.IdleDefinition, "Idle");

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

		private SClipDefinition drawClipDefinition(SClipDefinition definition, string labelTitle)
		{
			var newDefinition = definition;
			newDefinition.AnimationClip = (AnimationClip)EditorGUILayout.ObjectField(labelTitle, newDefinition.AnimationClip, typeof(AnimationClip), false);
			
			var cockTargetDefinition = newDefinition.CockTargetDefinition;
			cockTargetDefinition.CockTarget = (ECockTarget)EditorGUILayout.EnumPopup("Cock Target", cockTargetDefinition.CockTarget);
			if (cockTargetDefinition.CockTarget != ECockTarget.None)
				cockTargetDefinition.Curve = EditorGUILayout.CurveField("Curve", cockTargetDefinition.Curve, Color.green, new Rect(0, 0, 1f, 1f));
			newDefinition.CockTargetDefinition = cockTargetDefinition;
			
			newDefinition.IKDefinitions = verticalList(newDefinition.IKDefinitions, "IK");
			
			return newDefinition;
		}
		
		private SClipDefinition[] verticalList(SClipDefinition[] array, string labelTitle, bool modifyOnly = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (!modifyOnly)
			{
				if (GUILayout.Button("Add", GUILayout.Width(50)))
					array = array.Append(new SClipDefinition()).ToArray();
				if (GUILayout.Button("Clear", GUILayout.Width(50)))
					array = Array.Empty<SClipDefinition>();
			}
			
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				var definition = array[i];
				
				GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
				
				definition.AnimationClip = (AnimationClip)EditorGUILayout.ObjectField(definition.AnimationClip, typeof(AnimationClip), false);

				if (!modifyOnly)
				{
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						array = array.Where((_, k) => i != k).ToArray();
						break;
					}
				}
				
				GUILayout.EndHorizontal();
				
				var cockTargetDefinition = definition.CockTargetDefinition;
				cockTargetDefinition.CockTarget = (ECockTarget)EditorGUILayout.EnumPopup("Cock Target", cockTargetDefinition.CockTarget);
				if (cockTargetDefinition.CockTarget != ECockTarget.None)
					cockTargetDefinition.Curve = EditorGUILayout.CurveField("Curve", cockTargetDefinition.Curve, Color.green, new Rect(0, 0, 1f, 1f));
				definition.CockTargetDefinition = cockTargetDefinition;
				
				definition.IKDefinitions = verticalList(definition.IKDefinitions, "IK");
				
				GUILayout.EndVertical();
				
				array[i] = definition;

				if (i < array.Length)
					GUILayout.Space(10);
			}

			return array;
		}
		
		private bool[] verticalList(bool[] array, string labelTitle, bool modifyOnly = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (!modifyOnly)
			{
				if (GUILayout.Button("Add", GUILayout.Width(50)))
					array = array.Append(false).ToArray();
				if (GUILayout.Button("Clear", GUILayout.Width(50)))
					array = Array.Empty<bool>();
			}
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginHorizontal();
				array[i] = EditorGUILayout.ToggleLeft($"Container {i}", array[i]);
				
				if (!modifyOnly)
				{
					if (GUILayout.Button("-", GUILayout.Width(25)))
						array = array.Where((_, k) => i != k).ToArray();
				}
				
				GUILayout.EndHorizontal();
			}

			return array;
		}
		
		private string[] verticalList(string[] array, string labelTitle, bool modifyOnly = false)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);

			if (!modifyOnly)
			{
				if (GUILayout.Button("Add", GUILayout.Width(50)))
					array = array.Append("").ToArray();
				if (GUILayout.Button("Clear", GUILayout.Width(50)))
					array = Array.Empty<string>();
			}
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				GUILayout.BeginHorizontal();
				array[i] = EditorGUILayout.TextField(array[i]);
				
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
				
				container.ClipDefinitions ??= Array.Empty<SClipDefinition>();
				container.ClipDefinitions = verticalList(container.ClipDefinitions, "Clips");
				
				if (container.ClipContainerType == EClipContainerType.Single)
				{
					if (container.ClipDefinitions.Length == 0 || container.ClipDefinitions.Length > 1)
						Array.Resize(ref container.ClipDefinitions, 1);
				}
				else
				{
					if (container.ClipDefinitions.Length == 0 || container.ClipDefinitions.Length < 2)
						Array.Resize(ref container.ClipDefinitions, 2);
				}
				
				if (container.ClipDefinitions.Length > 1)
				{
					GUILayout.Space(5);
					
					container.Thresholds ??= Array.Empty<float>();

					if (container.Thresholds.Length != container.ClipDefinitions.Length)
						Array.Resize(ref container.Thresholds, container.ClipDefinitions.Length);

					container.Positions ??= Array.Empty<Vector2>();

					if (container.Positions.Length != container.ClipDefinitions.Length)
						Array.Resize(ref container.Positions, container.ClipDefinitions.Length);

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

		private SStateIKDefinition[] copyIK;
		
		private SStateIKDefinition[] verticalList(SStateIKDefinition[] array, string labelTitle)
		{
			array ??= Array.Empty<SStateIKDefinition>();
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelTitle + itemsCount(array.Length), EditorStyles.boldLabel);
			
			if (GUILayout.Button("Add", GUILayout.Width(50)))
				array = array.Append(new SStateIKDefinition()).ToArray();
			if (GUILayout.Button("Copy", GUILayout.Width(50)))
			{
				copyIK = new SStateIKDefinition[array.Length];
				for (var k = 0; k < array.Length; k++)
					copyIK[k] = array[k].Copy();
			}
			if (GUILayout.Button("Paste", GUILayout.Width(50)))
				array = array.Concat(SStateIKDefinition.CopyArray(copyIK)).ToArray();
			if (GUILayout.Button("Clear", GUILayout.Width(50)))
				array = Array.Empty<SStateIKDefinition>();
			
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < array.Length; i++)
			{
				var definition = array[i];
				
				GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
				
				if (definition.IsChain)
					definition.Chain = (FullBodyBipedChain)EditorGUILayout.EnumPopup("Chain", definition.Chain);
				else
					definition.Effector = (FullBodyBipedEffector)EditorGUILayout.EnumPopup("Effector", definition.Effector);
				
				if (GUILayout.Button("Copy", GUILayout.Width(50)))
					copyIK = new[] { array[i] };
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					array = array.Where((_, k) => i != k).ToArray();
					break;
				}
				
				GUILayout.EndHorizontal();
				
				GUILayout.Space(5);
				
				definition.IsChain = EditorGUILayout.Toggle("Is Chain", definition.IsChain);
				definition.TargetObject = (EIKTargetObject)EditorGUILayout.EnumPopup("Target Object", definition.TargetObject);
				
				GUILayout.Space(5);
				
				definition.TargetType = (EIKTargetType)EditorGUILayout.EnumPopup("Target Type", definition.TargetType);
				
				if (definition.TargetType == EIKTargetType.HumanBodyBones)
					definition.TargetBone = (HumanBodyBones)EditorGUILayout.EnumPopup("Target Bone", definition.TargetBone);
				else if (definition.TargetType == EIKTargetType.AdditionalHumanBodyBones)
					definition.AdditionalTargetBone = (EAdditionalHumanBodyBones)EditorGUILayout.EnumPopup("Additional Target Bone", definition.AdditionalTargetBone);
				
				GUILayout.Space(5);
				
				definition.PositionCurve = EditorGUILayout.CurveField("Position Curve", definition.PositionCurve, Color.green, new Rect(0, 0, 1f, 1f));
				definition.RotationCurve = EditorGUILayout.CurveField("Rotation Curve", definition.RotationCurve, Color.green, new Rect(0, 0, 1f, 1f));
				
				GUILayout.EndVertical();
				
				array[i] = definition;

				if (i < array.Length)
					GUILayout.Space(10);
			}

			return array;
		}
	}
}