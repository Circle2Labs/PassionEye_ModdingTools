using System;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class BlendshapeTransformDriver : BaseDriver
	{
		public override bool UseTarget => false;

		[Header("Blendshape Transform")]
		[SerializeField]
		public SkinnedMeshRenderer Renderer;
		
		[SerializeField]
		public string Blendshape;
		
		[Space(5)]
		[SerializeField]
		public bool ObjectPositionLocalSpace;
		
		[SerializeField]
		public bool ObjectRotationLocalSpace;
		
		[SerializeField]
		public bool ObjectScaleLocalSpace;

		[Space(5)]
		[SerializeField]
		public Vector3 NewPosition;
		
		[SerializeField]
		public Vector3 NewRotation;
		
		[SerializeField]
		public Vector3 NewScale = Vector3.one;

		private Vector3 originalPosition;
		private Vector3 originalLocalPosition;
		private Quaternion originalRotation;
		private Quaternion originalLocalRotation;
		private Vector3 originalScale;
		private Vector3 originalLocalScale;
		
		private int blendshapeIndex = -1;

		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalPosition = Object.position;
			originalLocalPosition = Object.localPosition;
			originalRotation = Object.rotation;
			originalLocalRotation = Object.localRotation;
			originalScale = Object.lossyScale;
			originalLocalScale = Object.localScale;

			var sharedMesh = Renderer.sharedMesh;
			for (var i = 0; i < sharedMesh.blendShapeCount; i++)
			{
				if (sharedMesh.GetBlendShapeName(i) != Blendshape)
					continue;

				blendshapeIndex = i;
				break;
			}
			
			if (blendshapeIndex == -1)
			{
				Debug.LogWarning($"[BlendshapeTransformDriver] Blendshape with name {Blendshape} not found");
				Enabled = false;
				return false;
			}
			
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localPosition = originalLocalPosition;
			Object.localEulerAngles = originalLocalRotation.eulerAngles;
			Object.localScale = originalLocalScale;
			return true;
		}

		public override void Run()
		{
			var blendshapeValue = Renderer.GetBlendShapeWeight(blendshapeIndex) / 100f;
			
			var objectPosition = Vector3.LerpUnclamped(ObjectPositionLocalSpace ? originalLocalPosition : originalPosition, NewPosition, blendshapeValue);
			
			if (ObjectPositionLocalSpace)
				Object.localPosition = objectPosition;
			else
				Object.position = objectPosition;
			
			var objectRotation = Quaternion.SlerpUnclamped(ObjectRotationLocalSpace ? originalLocalRotation : originalRotation, Quaternion.Euler(NewRotation), blendshapeValue);
			
			if (ObjectRotationLocalSpace)
				Object.localRotation = objectRotation;
			else
				Object.rotation = objectRotation;
			
			var objectScale = Vector3.LerpUnclamped(ObjectScaleLocalSpace ? originalLocalScale : originalScale, NewScale, blendshapeValue);
			
			if (ObjectScaleLocalSpace)
				Object.localScale = objectScale;
			else
				Debug.LogWarning("[BlendshapeTransformDriver] Applying worldspace scale is not supported");
		}
	}
}