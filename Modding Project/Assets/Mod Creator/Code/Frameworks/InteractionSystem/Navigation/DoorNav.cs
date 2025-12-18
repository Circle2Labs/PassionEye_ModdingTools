using System.Collections.Generic;
using Code.Components;
using Code.Components.Enums;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Frameworks.InteractionSystem.Navigation
{
	public class DoorNav : MonoBehaviour
	{
		[SerializeField]
		public Vector3 StartPoint;
		
		[SerializeField]
		public Vector3 EndPoint;

		[SerializeField]
		public float Width;
		
		[SerializeField]
		public bool Multidirectional;

		[SerializeField]
		public DoorController Door;
		
#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			var previousGizmosColor = Gizmos.color;
			Gizmos.matrix = transform.localToWorldMatrix;
			
			Gizmos.color = Color.white;
			Gizmos.DrawLine(StartPoint + new Vector3(Width / 2f, 0, 0), EndPoint + new Vector3(Width / 2f, 0, 0));
			Gizmos.DrawLine(StartPoint - new Vector3(Width / 2f, 0, 0), EndPoint - new Vector3(Width / 2f, 0, 0));
			
			Gizmos.DrawLine(StartPoint + new Vector3(Width / 2f, 0, 0), StartPoint - new Vector3(Width / 2f, 0, 0));
			Gizmos.DrawLine(EndPoint + new Vector3(Width / 2f, 0, 0), EndPoint - new Vector3(Width / 2f, 0, 0));
			
			Gizmos.DrawLine(EndPoint, EndPoint + new Vector3(0.1f, 0, -0.1f));
			Gizmos.DrawLine(EndPoint, EndPoint + new Vector3(-0.1f, 0, -0.1f));

			if (Multidirectional)
			{
				Gizmos.DrawLine(StartPoint, StartPoint + new Vector3(0.1f, 0, 0.1f));
				Gizmos.DrawLine(StartPoint, StartPoint + new Vector3(-0.1f, 0, 0.1f));
			}

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(Width, 0f, 0.1f));

			Gizmos.color = previousGizmosColor;
		}
#endif
	}
}