using System;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class BlendshapePositionDriver : BaseDriver
	{
		public override bool UseTarget => false;

		[Header("Blendshape Position")]
		[SerializeField]
		public SkinnedMeshRenderer Renderer;
		
		[SerializeField]
		public string Blendshape;
		
		[Space(5)]
		[SerializeField]
		public bool ObjectLocalSpace;

		[SerializeField]
		public Vector3 NewPosition;
		
		private Vector3 originalPosition;
		private Vector3 originalLocalPosition;
		
		private int blendshapeIndex = -1;

		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalPosition = Object.position;
			originalLocalPosition = Object.localPosition;

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
				Debug.LogWarning($"[BlendshapePositionDriver] Blendshape with name {Blendshape} not found");
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
			return true;
		}

		public override void Run()
		{
			var objectValue = Vector3.LerpUnclamped(ObjectLocalSpace ? originalLocalPosition : originalPosition, NewPosition, Renderer.GetBlendShapeWeight(blendshapeIndex) / 100f);
			
			if (ObjectLocalSpace)
				Object.localPosition = objectValue;
			else
				Object.position = objectValue;
		}
	}
}