using UnityEngine;

namespace Code.Attributes
{
	public class ReadOnlyAttribute : PropertyAttribute
	{
		
	}
	
#if UNITY_EDITOR
	[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyAttributeDrawer : UnityEditor.PropertyDrawer
	{
		public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false; 
			UnityEditor.EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
#endif
}