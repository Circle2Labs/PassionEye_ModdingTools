using System;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class BlendshapeScaleDriver : BaseDriver
	{
		public override bool UseTarget => false;

		[Header("Blendshape Scale")]
		[SerializeField]
		public SkinnedMeshRenderer Renderer;
		
		[SerializeField]
		public string Blendshape;
		
		[Space(5)]
		[SerializeField]
		public bool ObjectLocalSpace;

		[SerializeField]
		public Vector3 NewScale = Vector3.one;
		
		private Vector3 originalScale;
		private Vector3 originalLocalScale;
		
		private int blendshapeIndex = -1;

		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
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
				Debug.LogWarning($"[BlendshapeScaleDriver] Blendshape with name {Blendshape} not found");
				Enabled = false;
				return false;
			}
			
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localScale = originalLocalScale;
			return true;
		}

		public override void Run()
		{
			var objectValue = Vector3.LerpUnclamped(ObjectLocalSpace ? originalLocalScale : originalScale, NewScale, Renderer.GetBlendShapeWeight(blendshapeIndex) / 100f);
			
			if (ObjectLocalSpace)
				Object.localScale = objectValue;
			else
				Debug.LogWarning("[BlendshapeScaleDriver] Applying worldspace scale is not supported");
		}
	}
}