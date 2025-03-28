using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Animation.Interfaces;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.Studio.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator 
	{
		private const string uniqueIDFormat = "_#";
		
		private readonly Regex lodRegex = new ("^LOD\\d$");

		private Template copyBuffer;
		private SimulationData? copySimulationData;

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
				
				var animation = gameObject.GetComponent<IAnimation>();
				if (animation != null)
				{
					Debug.LogWarning($"Removing game component {animation}");
					DestroyImmediate((Component)animation);
				}
			}
		}
		
		public void FixNonexistentStates()
		{
			foreach (var template in Templates)
				template.SetupStates();
		}
		
		public void FixAccessoryParents()
		{
			foreach (var template in Templates.Where(template => template.TemplateType == ETemplateType.CharacterObject).Where(template => template.CharacterObjectType == ECharacterObjectType.Accessory))
			{
				var accessoryParents = new List<string>();
			
				var compatibleBaseMeshes = template.CompatibleBaseMeshes;
				if (compatibleBaseMeshes != null && compatibleBaseMeshes.Length > 0)
				{
					var baseMesh = compatibleBaseMeshes[0];
					accessoryParents = GetAccessoryParents(new Tuple<string, byte>(baseMesh.GUID, baseMesh.ID));
				}
				
				for (var i = 0; i < accessoryParents.Count; i++)
				{
					if (accessoryParents[i] != template.DefaultParent)
						continue;

					template.DefaultParentIdx = i;
				}
			}
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

		private string itemsCount(int count)
		{
			return $" ({count} item{(count == 1 ? "" : "s")})";
		}

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
		
		private void focusSelectedObject()
		{
			var sceneView = SceneView.lastActiveSceneView;
				
			var focusedField = sceneView.GetType().GetField("m_WasFocused", BindingFlags.NonPublic | BindingFlags.Instance);
			focusedField.SetValue(sceneView, false);
			
			sceneView.FrameSelected(false, true);
		}
		
		private bool containsBlendshape(Template template, string shape)
		{
			if (template.Blendshapes == null)
				return false;

			foreach (var sBlendShape in template.Blendshapes)
			{
				foreach (var blendShape in sBlendShape.Blendshapes)
				{
					if (blendShape != shape)
						continue;

					return true;
				}
			}

			return false;
		}
	}
}