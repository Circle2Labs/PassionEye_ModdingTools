using System;
using Code.Components.Enums;
using UnityEngine;

namespace Code.Components
{
	/// <summary>
	/// Stores data to create a Magica collider on runtime since paid assets can't be bundled in modding tools
	/// Drawn with the same shape as it would be in Magica
	/// </summary>
	public class PhysicsColliderHolder : MonoBehaviour
	{
		[SerializeField]
		public EPhysicsColliderType Type;
		
		#region Sphere

		[SerializeField]
		[Range(0.001f, 0.5f)]
		public float Radius = 0.001f;
		
		#endregion

		#region Capsule

		[SerializeField]
		public EPhysicsColliderDirection Direction;
		
		[SerializeField]
		public bool AlignedOnCenter = true;

		[SerializeField]
		[Range(0.001f, 2f)]
		public float Length = 0.001f;

		[SerializeField]
		[Range(0.001f, 0.5f)]
		public float StartRadius = 0.001f;
		
		[SerializeField]
		[Range(0.001f, 0.5f)]
		public float EndRadius = 0.001f;
		
		#endregion
		
		[SerializeField]
		public Vector3 Center;

#if UNITY_EDITOR
		public void OnDrawGizmosSelected()
		{
			var camTr = UnityEditor.SceneView.currentDrawingSceneView.camera.transform;
			
			var pos = transform.TransformPoint(Center);
			var rot = transform.rotation;
			var scale = Vector3.one * transform.lossyScale.x;

			var camRot = Quaternion.Inverse(rot) * camTr.rotation;
			
			UnityEditor.Handles.matrix = Matrix4x4.TRS(pos, rot, scale);
			UnityEditor.Handles.color = Color.cyan;
			
			switch (Type)
			{
				case EPhysicsColliderType.Sphere:
					// Draw sphere
					UnityEditor.Handles.CircleHandleCap(0, Center, camRot, Radius, EventType.Repaint);
					UnityEditor.Handles.DrawWireDisc(Center, Vector3.up, Radius);
					UnityEditor.Handles.DrawWireDisc(Center, Vector3.right, Radius);
					UnityEditor.Handles.DrawWireDisc(Center, Vector3.forward, Radius);
					break;
				case EPhysicsColliderType.Capsule:
					var startOffsetAmount = 0f;
					var endOffsetAmount = 0f;

					if (Length > StartRadius * 2f)
						startOffsetAmount = (Length - StartRadius * 2f) / 2f;
							
					if (Length > EndRadius * 2f)
						endOffsetAmount = -((Length - EndRadius * 2f) / 2f);

					var startOffsetVector = new Vector3();
					var endOffsetVector = new Vector3();
					
					switch (Direction)
					{
						case EPhysicsColliderDirection.X:
							startOffsetVector.x = startOffsetAmount;
							endOffsetVector.x = endOffsetAmount;
							break;
						case EPhysicsColliderDirection.Y:
							startOffsetVector.y = startOffsetAmount;
							endOffsetVector.y = endOffsetAmount;
							break;
						case EPhysicsColliderDirection.Z:
							startOffsetVector.z = startOffsetAmount;
							endOffsetVector.z = endOffsetAmount;
							break;
						default:
							throw new NotImplementedException();
					}

					var alignVector = Vector3.zero;
					
					if (!AlignedOnCenter)
						alignVector = -startOffsetVector;

					var startCenter = Center + startOffsetVector + alignVector;
					var endCenter = Center + endOffsetVector + alignVector;
					
					// Draw start sphere
					UnityEditor.Handles.CircleHandleCap(0, startCenter, camRot, StartRadius, EventType.Repaint);
					UnityEditor.Handles.DrawWireDisc(startCenter, Vector3.up, StartRadius);
					UnityEditor.Handles.DrawWireDisc(startCenter, Vector3.right, StartRadius);
					UnityEditor.Handles.DrawWireDisc(startCenter, Vector3.forward, StartRadius);

					// Draw end sphere
					UnityEditor.Handles.CircleHandleCap(0, endCenter, camRot, EndRadius, EventType.Repaint);
					UnityEditor.Handles.DrawWireDisc(endCenter, Vector3.up, EndRadius);
					UnityEditor.Handles.DrawWireDisc(endCenter, Vector3.right, EndRadius);
					UnityEditor.Handles.DrawWireDisc(endCenter, Vector3.forward, EndRadius);
					
					Vector3 rotateVector;
					
					// Draw lines connecting spheres
					switch (Direction)
					{
						case EPhysicsColliderDirection.X:
							rotateVector = new Vector3(0f, 1f, 0f);
							for (var i = 0; i < 8; i++)
							{
								UnityEditor.Handles.DrawLine(startCenter + rotateVector * StartRadius, endCenter + rotateVector * EndRadius);
								rotateVector = Quaternion.AngleAxis(-45, Vector3.right) * rotateVector;
							}
							break;
						case EPhysicsColliderDirection.Y:
							rotateVector = new Vector3(0f, 0f, 1f);
							for (var i = 0; i < 8; i++)
							{
								UnityEditor.Handles.DrawLine(startCenter + rotateVector * StartRadius, endCenter + rotateVector * EndRadius);
								rotateVector = Quaternion.AngleAxis(-45, Vector3.up) * rotateVector;
							}
							break;
						case EPhysicsColliderDirection.Z:
							rotateVector = new Vector3(1f, 0f, 0f);
							for (var i = 0; i < 8; i++)
							{
								UnityEditor.Handles.DrawLine(startCenter + rotateVector * StartRadius, endCenter + rotateVector * EndRadius);
								rotateVector = Quaternion.AngleAxis(-45, Vector3.forward) * rotateVector;
							}
							break;
						default:
							throw new NotImplementedException();
					}
					break;
				default:
					throw new NotImplementedException();
			}
		}
#endif
	}
}