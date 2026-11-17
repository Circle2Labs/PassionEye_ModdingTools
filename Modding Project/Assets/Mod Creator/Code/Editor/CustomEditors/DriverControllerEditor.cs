using System;
using System.Collections.Generic;
using Code.Frameworks.DriverSystem;
using UnityEditor;
using UnityEngine;
 
namespace Code.Editor.CustomEditors
{
    [CustomEditor(typeof(DriverController))]
	public class DriverControllerEditor : UnityEditor.Editor
	{
		public DriverController DriverController;
		
		private List<Type> driverTypes;
		private List<string> driverTypeNames;
		
		private int createDriverType;

		public void OnEnable()
		{
			DriverController = (DriverController)target;

			if (driverTypes != null)
				return;

			driverTypes = new List<Type>();
			driverTypeNames = new List<string>();
				
			var baseType = typeof(BaseDriver);
				
			var types = baseType.Assembly.GetTypes();
			for (var i = 0; i < types.Length; i++)
			{
				var type = types[i];
				if (!type.IsSubclassOf(baseType))
					continue;

				driverTypes.Add(type);
				driverTypeNames.Add(type.Name);
			}
		}

		public override void OnInspectorGUI()
		{
			GUILayout.BeginHorizontal();
			
			createDriverType = EditorGUILayout.Popup(createDriverType, driverTypeNames.ToArray());
			
			if (GUILayout.Button("Add", GUILayout.Width(65)))
			{
				DriverController.Drivers.Add((BaseDriver)Activator.CreateInstance(driverTypes[createDriverType]));
				
				EditorUtility.SetDirty(DriverController);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(DriverController.gameObject.scene);
			}

			if (GUILayout.Button("Clear"))
			{
				DriverController.Drivers.Clear();
				
				EditorUtility.SetDirty(DriverController);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(DriverController.gameObject.scene);
			}
			
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Enable"))
			{
				DriverController.enabled = true;
				
				EditorUtility.SetDirty(DriverController);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(DriverController.gameObject.scene);
			}

			if (GUILayout.Button("Disable"))
			{
				DriverController.enabled = false;
				
				EditorUtility.SetDirty(DriverController);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(DriverController.gameObject.scene);
			}

			if (GUILayout.Button("Restart"))
			{
				DriverController.enabled = false;
				DriverController.enabled = true;
				
				EditorUtility.SetDirty(DriverController);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(DriverController.gameObject.scene);
			}
			
			GUILayout.EndHorizontal();
			
			for (var i = DriverController.Drivers.Count - 1; i >= 0; i--)
			{
				var driver = DriverController.Drivers[i];
				if (driver != null)
					continue;

				DriverController.Drivers.RemoveAt(i);
				
				EditorUtility.SetDirty(DriverController);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(DriverController.gameObject.scene);
			}
			
			base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
		}
	}
}