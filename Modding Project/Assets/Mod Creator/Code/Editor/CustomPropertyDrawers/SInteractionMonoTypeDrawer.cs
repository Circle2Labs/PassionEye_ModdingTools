using System.Collections.Generic;
using System.Reflection;
using Code.Frameworks.InteractionSystem.Database.Structs;
using Code.Frameworks.InteractionSystem.Interactions;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.CustomPropertyDrawers
{
	[CustomPropertyDrawer(typeof(SInteractionMonoType))]
	public class SInteractionMonoTypeDrawer : PropertyDrawer
	{
		private string[] interactionTypes;
		public string[] InteractionTypes
		{
			get
			{
				if (interactionTypes != null)
					return interactionTypes;

				interactionTypes = getInteractionTypes();
				return interactionTypes;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0f;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var newIndex = EditorGUILayout.Popup(label, getInteractionType(property), getInteractionTypes());
			var monoType = getInteractionType(newIndex);

			EditorGUI.BeginProperty(position, label, property);

			var assemblyProperty = property.FindPropertyRelative("Assembly");
			assemblyProperty.stringValue = monoType.Item1;
			
			var classProperty = property.FindPropertyRelative("Class");
			classProperty.stringValue = monoType.Item2;
			
			EditorGUI.EndProperty();

			if (GUILayout.Button("Refresh Types"))
				interactionTypes = getInteractionTypes();
		}

		private string[] getInteractionTypes()
		{
			var list = new List<string>();

			var assembly = Assembly.GetAssembly(typeof(IBaseInteraction));
			var types = assembly.GetTypes();
				
			foreach (var type in types)
			{
				if (!typeof(IBaseInteraction).IsAssignableFrom(type) || type.IsInterface)
					continue;

				list.Add(type.FullName);
			}

			return list.ToArray();
		}

		private int getInteractionType(SerializedProperty property)
		{
			var monoType = new SInteractionMonoType
			{
				Class = property.FindPropertyRelative("Class").stringValue,
				Assembly = property.FindPropertyRelative("Assembly").stringValue
			};

			for (var i = 0; i < InteractionTypes.Length; i++)
			{
				var interactionType = InteractionTypes[i];
				if (interactionType != monoType.Class || monoType.Assembly != "Assembly-CSharp")
					continue;

				return i;
			}

			return -1;
		}

		private (string, string) getInteractionType(int type)
		{
			if (type >= InteractionTypes.Length || type < 0)
				return ("", "");

			return ("Assembly-CSharp", InteractionTypes[type]);
		}
	}
}