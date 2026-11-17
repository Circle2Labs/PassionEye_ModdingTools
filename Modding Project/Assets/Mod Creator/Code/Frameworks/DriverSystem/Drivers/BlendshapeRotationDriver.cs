using System;
using UnityEngine;

namespace Code.Frameworks.DriverSystem.Drivers
{
	[Serializable]
	public class BlendshapeRotationDriver : BaseDriver
	{
		public override bool UseTarget => false;

		[Header("Blendshape Rotation")]
		[SerializeField]
		public SkinnedMeshRenderer Renderer;
		
		[SerializeField]
		public string Blendshape;
		
		[Space(5)]
		[SerializeField]
		public bool ObjectLocalSpace;

		[SerializeField]
		public Vector3 NewRotation;
		
		private Quaternion originalRotation;
		private Quaternion originalLocalRotation;
		
		private int blendshapeIndex = -1;

		public override bool OnEnable()
		{
			if (!base.OnEnable())
				return false;
			
			originalRotation = Object.rotation;
			originalLocalRotation = Object.localRotation;

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
				Debug.LogWarning($"[BlendshapeRotationDriver] Blendshape with name {Blendshape} not found");
				Enabled = false;
				return false;
			}
			
			return true;
		}

		public override bool OnDisable()
		{
			if (!base.OnDisable())
				return false;

			Object.localEulerAngles = originalLocalRotation.eulerAngles;
			return true;
		}

		public override void Run()
		{
			var objectValue = Quaternion.SlerpUnclamped(ObjectLocalSpace ? originalLocalRotation : originalRotation, Quaternion.Euler(NewRotation), Renderer.GetBlendShapeWeight(blendshapeIndex) / 100f);
			
			if (ObjectLocalSpace)
				Object.localRotation = objectValue;
			else
				Object.rotation = objectValue;
		}
	}
}