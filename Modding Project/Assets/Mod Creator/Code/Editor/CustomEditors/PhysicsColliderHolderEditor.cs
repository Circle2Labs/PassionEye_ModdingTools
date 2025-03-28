using System;
using Code.Components;
using Code.Components.Enums;
using UnityEditor;

namespace Code.Editor.CustomEditors
{
	[CustomEditor(typeof(PhysicsColliderHolder)), CanEditMultipleObjects]
	public class PhysicsColliderHolderEditor : UnityEditor.Editor
	{
		private readonly string[] sphereProperties =
		{
			nameof(PhysicsColliderHolder.Radius),
			"m_Script"
		};
		
		private readonly string[] capsuleProperties =
		{
			nameof(PhysicsColliderHolder.Direction),
			nameof(PhysicsColliderHolder.AlignedOnCenter),
			nameof(PhysicsColliderHolder.Length),
			nameof(PhysicsColliderHolder.StartRadius),
			nameof(PhysicsColliderHolder.EndRadius),
			"m_Script"
		};
		
		public override void OnInspectorGUI()
		{
			var holder = (PhysicsColliderHolder)target;
			switch (holder.Type)
			{
				case EPhysicsColliderType.Sphere:
					DrawPropertiesExcluding(serializedObject, capsuleProperties);
					break;
				case EPhysicsColliderType.Capsule:
					DrawPropertiesExcluding(serializedObject, sphereProperties);
					break;
				default:
					throw new NotImplementedException();
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}