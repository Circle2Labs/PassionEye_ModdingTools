using System;
using System.Collections.Generic;
using System.Threading;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.ClippingFix.Enums;
using Code.Frameworks.RayTracing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Ray = Code.Frameworks.RayTracing.Ray;
using Timer = System.Timers.Timer;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator
	{
		private bool hidePreviewRenderers;
		public bool HidePreviewRenderers
		{
			get => hidePreviewRenderers;
			set
			{
				var previousValue = hidePreviewRenderers;
				hidePreviewRenderers = value;
				
				if (previewingTemplate == null || previousValue == hidePreviewRenderers)
					return;
				
				previewingClothingClone.Item1.gameObject.SetActive(!value);
			}
		}

		private ERaysResolution bodyRaysResolution = (ERaysResolution)3;
		public ERaysResolution BodyRaysResolution
		{
			get
			{
				switch ((int)bodyRaysResolution)
				{
					case 0:
						return ERaysResolution.Low;
					case 1:
						return ERaysResolution.Medium;
					case 2:
						return ERaysResolution.High;
					case 3:
						return ERaysResolution.VeryHigh;
				}

				return ERaysResolution.Low;
			}
		}

		// actual properties for baking
		private BVHNodeGPU[] bvhNodes;
		private Triangle[] triangles;
		
		// compute shaders
		private ComputeShader clippingFixShader;
		private int clippingFixKernel;

		private ComputeShader dilaterShader;
		private int dilaterKernel;
		
		private ComputeBuffer raysBuffer;
		private CustomRenderTexture bakerTempRT;

		// preview base mesh data
		private GameObject previewingBody;
		private Material previewingBodyMaterial;
		private CustomRenderTexture previewingClippingFixTexture;
		private Dictionary<ERaysResolution, Ray[]> bodyRays;
		
		// others
		private Tuple<Transform, SkinnedMeshRenderer[]> previewingClothingClone;
		private Template previewingTemplate;
		private EClothingState previewingState;
		private Dictionary<string, Transform> previewingBodyBones;
		private Timer previewTimer;

		private readonly SynchronizationContext context = SynchronizationContext.Current;

		// PreviewingBodyMaterial - material of the test body
		// PreviewingBody - gameobject of the test body
		// previewingState - clothing state being previewed
		// previewingClothingRenderers - all renderers that belong to the clothing state being previewed
		// BodyRaysResolution - selected resolution of body rays (make sure to use the property, not the field)
		// BodyRays - dictionary of resolution->ray[] (make sure to use the property, not the field)

		private void startPreview(Template template, EClothingState state, Transform clothingTransform)
		{
			Tuple<string, byte> guid;
			IBaseMesh baseMesh;
			
			var compatibleBaseMeshes = template.CompatibleBaseMeshes;
			if (compatibleBaseMeshes != null && compatibleBaseMeshes.Length > 0)
			{
				var compatibleBaseMesh = compatibleBaseMeshes[0];
				
				guid = new Tuple<string, byte>(compatibleBaseMesh.GUID, compatibleBaseMesh.ID);
				baseMesh = GetBaseMeshInstance(guid);
			}
			else
			{
				guid = null;
				baseMesh = null;
			}

			if (baseMesh == null)
			{
				Debug.LogError("Failed to grab compatible base mesh. Make sure there is one specified");
				stopPreview();
				return;
			}
			
			Debug.Log("Entering clothing preview mode");

			#region Base Mesh

			var bodyRenderer = baseMesh.GetBodyRenderer();
			
			previewingBody = baseMesh.GetGameObject();
			previewingBodyMaterial = bodyRenderer!.Value.Item1.sharedMaterials[bodyRenderer.Value.Item2];
			
			previewingBodyBones = new Dictionary<string,Transform>();
			var bones = bodyRenderer.Value.Item1.bones;
			foreach (var bone in bones)
				previewingBodyBones.Add(bone.name, bone);
			
			previewingClippingFixTexture = new CustomRenderTexture((int)BodyRaysResolution, (int)BodyRaysResolution, RenderTextureFormat.R8);
			previewingClippingFixTexture.enableRandomWrite = true;
			previewingClippingFixTexture.doubleBuffered = true;
			previewingClippingFixTexture.material = CoreUtils.CreateEngineMaterial(Shader.Find("White"));
			previewingClippingFixTexture.Create();
			previewingClippingFixTexture.Initialize();
			previewingClippingFixTexture.Update();

			previewingBodyMaterial.SetTexture("_AlphaMap", previewingClippingFixTexture);

			bodyRays = GetBaseMeshRays(guid);
			
			#endregion

			// hide all states except the one we need so we can grab the appropriate renderers when cloning
			for (var i = 0; i < template.ClothingStates.Length; i++)
				if (template.ClothingStates[i] != null)
					template.ClothingStates[i].gameObject.SetActive(state == (EClothingState)i);
			
			// get a copy of the clothing so it can be used as a preview
			var clone = Instantiate(clothingTransform);
			
			// get only renderers on active objects (the correct state)
			var renderers = clone.GetComponentsInChildren<SkinnedMeshRenderer>(false);

			// re-enable all states again
			foreach (var stateObject in template.ClothingStates)
				if (stateObject != null)
					stateObject.gameObject.SetActive(true);
			
			previewingClothingClone = new Tuple<Transform, SkinnedMeshRenderer[]>(clone, renderers);
			previewingTemplate = template;
			previewingState = state;
			
			// hide all original objects
			foreach (var prefab in Prefabs)
			{
				if (prefab == null || prefab is not GameObject gameObject)
					continue;
				
				gameObject.SetActive(false);
			}
			
			previewingBodyMaterial.SetTexture("_AlphaMap", null);
			previewingBody.SetActive(true);
			
			/*// remap preview clothing bones to body bones
			foreach (var skinnedMeshRenderer in previewingClothingClone.Item2)
			{
				var currentBones = skinnedMeshRenderer.bones;
				var newBones = new Transform[currentBones.Length];
				for (var i = 0; i < currentBones.Length; i++)
				{
					var bone = currentBones[i].gameObject;
					previewingBodyBones.TryGetValue(bone.name, out newBones[i]);

					if (newBones[i] == null)
						newBones[i] = bone.transform;
				}
				skinnedMeshRenderer.bones = newBones;
			}*/
			
			// TODO: this doesn't work correctly. bake preview clothing meshes to adjust to new bones
			/*foreach (var skinnedMeshRenderer in previewingClothingClone.Item2)
			{
				var transform = skinnedMeshRenderer.transform;
				
				var previousPosition = transform.localPosition;
				var previousAngles = transform.localEulerAngles;
				
				transform.localPosition = Vector3.zero;
				transform.localEulerAngles = Vector3.zero;

				var newMesh = new Mesh();
				skinnedMeshRenderer.BakeMesh(newMesh, true);
				skinnedMeshRenderer.sharedMesh = newMesh;

				transform.localPosition = previousPosition;
				transform.localEulerAngles = previousAngles;
			}*/
			
			// select and focus previewing state
			Selection.activeGameObject = clone.gameObject;
			focusSelectedObject();
			
			// make sure things are shown when needed
			HidePreviewRenderers = !HidePreviewRenderers;
			HidePreviewRenderers = !HidePreviewRenderers;
			
			// initialize compute
			clippingFixShader = Resources.Load<ComputeShader>("Compute/TransparencyBaker");
			clippingFixKernel = clippingFixShader.FindKernel("CastRaysBVH");

			dilaterShader = Resources.Load<ComputeShader>("Compute/DilateTexture");
			dilaterKernel = dilaterShader.FindKernel("DilateTexture");
			
			bvhNodes = null;
			triangles = null;
			
			// reset textures
			if (previewingClippingFixTexture != null)
			{
				previewingClippingFixTexture.Release();
				previewingClippingFixTexture = null;
			}
			
			if (bakerTempRT != null)
			{
				bakerTempRT.Release();
				bakerTempRT = null;
			}
			
			refreshPreview(-1, BodyRaysResolution);
		}
		
		private void refreshPreview(float previousClippingDistance, ERaysResolution previousBodyRaysResolution)
		{
			if (previewingTemplate == null)
				return;

			// Hide previewing item for half a second and then show it again, used to see the resulting clipping texture
			if (!HidePreviewRenderers)
			{
				previewingClothingClone.Item1.gameObject.SetActive(false);

				if (previewTimer == null)
				{
					previewTimer = new Timer();
					previewTimer.Interval = 500;
					previewTimer.AutoReset = false;
					previewTimer.Enabled = true;
					previewTimer.Elapsed += delegate { context.Send(delegate
					{
						if (HidePreviewRenderers)
							return;

						previewingClothingClone.Item1.gameObject.SetActive(true);
					}, null); };
				}

				previewTimer.Stop();
				previewTimer.Start();
			}
			
			// if it's the first time we're baking, we need to build the BVH
			if (bvhNodes == null || triangles == null)
			{
				Raycaster raycaster = new();

				foreach (var renderer in previewingClothingClone.Item2)
					raycaster.AddMesh(renderer.sharedMesh, renderer.transform);

				raycaster.BuildBVH();

				(bvhNodes, triangles) = raycaster.BvhRoot.ToGPU();
			}
			
			// if resolution is changed, we need to update the rays buffer and the textures
			if (BodyRaysResolution != previousBodyRaysResolution || raysBuffer == null)
			{
				if (previewingClippingFixTexture != null)
				{
					previewingClippingFixTexture.Release();
					previewingClippingFixTexture = null;
				}

				if (bakerTempRT != null)
				{
					bakerTempRT.Release();
					bakerTempRT = null;
				}
				
				var rays = bodyRays[BodyRaysResolution];
				
				raysBuffer = new ComputeBuffer(rays.Length, Ray.Size);
				raysBuffer.SetData(rays);
				
				clippingFixShader.SetBuffer(clippingFixKernel, "Rays", raysBuffer);
			}
			
			var bvhNodesBuffer = new ComputeBuffer(bvhNodes.Length, BVHNodeGPU.Size);
			bvhNodesBuffer.SetData(bvhNodes);
			
			var trianglesBuffer = new ComputeBuffer(triangles.Length, Triangle.Size);
			trianglesBuffer.SetData(triangles);
			
			// we only have one bvh root, so the only offset is 0
			var bvhNodesOffsets = new ComputeBuffer(1, sizeof(int));
			bvhNodesOffsets.SetData(new[] { 0 });
			
			// same as above, we only have one clothing piece so one distance
			var raycastDistancesBuffer = new ComputeBuffer(1, sizeof(float));
			raycastDistancesBuffer.SetData(new[] { previewingTemplate.ClippingDistance });

			if (previewingClippingFixTexture == null)
			{
				previewingClippingFixTexture = new CustomRenderTexture((int)BodyRaysResolution, (int)BodyRaysResolution, RenderTextureFormat.R8);
				previewingClippingFixTexture.enableRandomWrite = true;
				previewingClippingFixTexture.doubleBuffered = true;
				previewingClippingFixTexture.material = CoreUtils.CreateEngineMaterial(Shader.Find("White"));
				previewingClippingFixTexture.Create();
				previewingClippingFixTexture.Initialize();
				previewingClippingFixTexture.Update();

				previewingBodyMaterial.SetTexture("_AlphaMap", previewingClippingFixTexture);
			}
			
			if (bakerTempRT == null)
			{
				bakerTempRT = new CustomRenderTexture((int)BodyRaysResolution, (int)BodyRaysResolution, RenderTextureFormat.R8);
				bakerTempRT.doubleBuffered = true;
				bakerTempRT.enableRandomWrite = true;
				bakerTempRT.material = new Material(Shader.Find("White"));
				bakerTempRT.Create();
				bakerTempRT.Initialize();
				bakerTempRT.Update();
			}

			clippingFixShader.SetTexture(clippingFixKernel, "Hitmap", bakerTempRT);
			clippingFixShader.SetBuffer(clippingFixKernel, "Rays", raysBuffer);
			clippingFixShader.SetBuffer(clippingFixKernel, "BVHNodes", bvhNodesBuffer);
			clippingFixShader.SetBuffer(clippingFixKernel, "Triangles", trianglesBuffer);
			clippingFixShader.SetBuffer(clippingFixKernel, "NodesOffsets", bvhNodesOffsets);
			clippingFixShader.SetBuffer(clippingFixKernel, "RaycastDistances", raycastDistancesBuffer);
            
			var fence = Graphics.CreateAsyncGraphicsFence();
			clippingFixShader.Dispatch(clippingFixKernel, bakerTempRT.width / 32, bakerTempRT.height / 32, 1);
			Graphics.WaitOnAsyncGraphicsFence(fence);

			// then dilate the texture
			// TODO: set the dilation radius as a parameter
			dilaterShader.SetInt("kernelSize", 6);
			dilaterShader.SetTexture(dilaterKernel, "original", bakerTempRT);
			dilaterShader.SetTexture(dilaterKernel, "result", previewingClippingFixTexture);
            
			fence = Graphics.CreateAsyncGraphicsFence();
			dilaterShader.Dispatch(dilaterKernel, previewingClippingFixTexture.width / 32, previewingClippingFixTexture.height / 32, 1);
			Graphics.WaitOnAsyncGraphicsFence(fence);
			
			// cleanup
			bvhNodesBuffer.Release();
			trianglesBuffer.Release();
			bvhNodesOffsets.Release();
			raycastDistancesBuffer.Release();
			
			// see the changes instantly
			SceneView.RepaintAll();
		}
		
		private void stopPreview()
		{
			// clean up data
			previewTimer?.Stop();
			
			bvhNodes = null;
			triangles = null;
			
			raysBuffer?.Release();
			raysBuffer = null;

			if (bakerTempRT != null)
			{
				bakerTempRT.Release();
				bakerTempRT = null;
			}

			if (previewingClippingFixTexture != null)
			{
				previewingClippingFixTexture.Release();
				previewingClippingFixTexture = null;
			}
			
			// remove the copied preview clothing
			if (previewingClothingClone != null && previewingClothingClone.Item1 != null)
				DestroyImmediate(previewingClothingClone.Item1.gameObject);
			
			// disable the preview body
			if (previewingBody != null)
				previewingBody.SetActive(false);

			previewingBodyMaterial = null;
			previewingBodyBones = null;
			
			if (previewingTemplate == null)
				return;

			Debug.Log("Exiting clothing preview mode");

			foreach (var prefab in Prefabs)
			{
				if (prefab == null || prefab is not GameObject gameObject)
					continue;
				
				gameObject.SetActive(true);
			}
			
			Selection.activeGameObject = null;
			focusSelectedObject();

			previewingTemplate = null;
		}
	}
}