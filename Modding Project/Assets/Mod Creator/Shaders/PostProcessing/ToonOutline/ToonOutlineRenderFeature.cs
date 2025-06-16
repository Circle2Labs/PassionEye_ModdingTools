using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

namespace Code.Frameworks.Outline
{
    public class ToonOutlineRenderFeature : ScriptableRendererFeature
    {
        private ToonOutlinePass toonOutlinePass;

        [SerializeField]
        private Shader toonOutlineShader;
        [SerializeField]
        private Shader blurBlendShader;
        private Material toonOutlineMat;
        private Material blurBlendMat;
        private GraphicsFormat cameraColorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        
        //rthandles
        RTHandleSystem rtHandleSystem;
        RTHandle outlineRT, blurredOutlineRT;
        
        int width, height;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(renderingData.cameraData.cameraType == CameraType.Game)
            {
                if(width != renderingData.cameraData.camera.pixelWidth || height != renderingData.cameraData.camera.pixelHeight)
                {
                    width = renderingData.cameraData.camera.pixelWidth;
                    height = renderingData.cameraData.camera.pixelHeight;
                    rtHandleSystem.ResetReferenceSize(width, height);
                }
                    
                toonOutlinePass.ConfigureInput(ScriptableRenderPassInput.Color |
                                               ScriptableRenderPassInput.Depth |
                                               ScriptableRenderPassInput.Normal);
                
                renderer.EnqueuePass(toonOutlinePass);
                
            }
        }
        
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
            if(renderingData.cameraData.cameraType == CameraType.Game)
                cameraColorFormat = renderingData.cameraData.renderer.cameraColorTargetHandle.rt.graphicsFormat;
            base.SetupRenderPasses(renderer, in renderingData);
        }

        public override void Create()
        {
            
            //create materials only if they aren't already istantiated
            if (toonOutlineMat == null)
            {
                toonOutlineMat = CoreUtils.CreateEngineMaterial(toonOutlineShader);
                blurBlendMat = CoreUtils.CreateEngineMaterial(blurBlendShader);
            }
            
            //initialize rthandlesystem and rthandles
            if (rtHandleSystem == null)
            {
                rtHandleSystem = new();
                width = Screen.width;
                height = Screen.height;
                rtHandleSystem.Initialize(width, height);

                //instantiate rthandles
                outlineRT = rtHandleSystem.Alloc(
                    scaleFactor: Vector2.one,
                    colorFormat: cameraColorFormat,
                    enableRandomWrite: true,
                    useMipMap: true,
                    msaaSamples: MSAASamples.None,
                    name: "ToonOutlineTex"
                );

                blurredOutlineRT = rtHandleSystem.Alloc(
                    scaleFactor: Vector2.one,
                    colorFormat: cameraColorFormat,
                    enableRandomWrite: true,
                    useMipMap: false,
                    msaaSamples: MSAASamples.None,
                    name: "TooOutlineBlurredTex"
                );
            }

            //send everything to the pass
            toonOutlinePass = new ToonOutlinePass(toonOutlineMat, blurBlendMat, outlineRT, blurredOutlineRT);
        }

        protected override void Dispose(bool disposing)
        {
            //cleanup unneeded materials 
            //CoreUtils.Destroy(outlineMat);
        }
    }
    
    class ToonOutlinePass : ScriptableRenderPass
    {
        Material toonOutlineMat, blurBlendMat;
        RTHandle outlineRT, blurredOutlineRT;
        RTHandle cameraHandleRT, cameraDepthRT, cameraNormalRT;
        
        public ToonOutlinePass(Material toonOutlineMat, Material blurBlendMat, RTHandle outlineRT, RTHandle blurredOutlineRT)
        {
            this.toonOutlineMat = toonOutlineMat;
            this.blurBlendMat = blurBlendMat;
            this.outlineRT = outlineRT;
            this.blurredOutlineRT = blurredOutlineRT;
            
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing + 1;
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            cameraHandleRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            VolumeStack volumes = VolumeManager.instance.stack;
            
            ToonOutlinePostProcess toonOutlinePostProcess = volumes.GetComponent<ToonOutlinePostProcess>();
            
            if(toonOutlinePostProcess.IsActive()){
                cameraHandleRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
                CommandBuffer cmd = CommandBufferPool.Get("Toon Outline Pass");
                cmd.Clear();
                
                cmd.SetRenderTarget(outlineRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.ClearRenderTarget(true, true, Color.clear);
                
                //properties
                toonOutlineMat.SetFloat("_DepthThreshold", toonOutlinePostProcess.DepthThreshold.value);
                toonOutlineMat.SetFloat("_NormalsThreshold", toonOutlinePostProcess.NormalsThreshold.value);
                toonOutlineMat.SetColor("_Color", toonOutlinePostProcess.OutlineColor.value);
                toonOutlineMat.SetInt("_Radius", toonOutlinePostProcess.Radius.value);
                toonOutlineMat.SetFloat("_AngleFixScale", toonOutlinePostProcess.AngleFixScale.value);
                toonOutlineMat.SetFloat("_AngleFixPower", toonOutlinePostProcess.AngleFixPower.value);
                toonOutlineMat.SetFloat("_FadeStart", toonOutlinePostProcess.FadeStart.value);
                toonOutlineMat.SetFloat("_FadeEnd", toonOutlinePostProcess.FadeEnd.value);
                toonOutlineMat.SetFloat("_DEBUGOUTLINE", toonOutlinePostProcess.DebugOutline.value ? 1 : 0);
                toonOutlineMat.SetFloat("_DebugTransparency", toonOutlinePostProcess.DebugTransparency.value);

                toonOutlineMat.SetTexture("_BlitTexture", cameraHandleRT);
                
                cmd.DrawProcedural(Matrix4x4.identity, toonOutlineMat, 0, MeshTopology.Triangles, 3, 1);

                context.ExecuteCommandBuffer(cmd);
                context.Submit();

                CommandBufferPool.Release(cmd);
                
                CommandBuffer cmd2 = CommandBufferPool.Get("Toon Outline Pass/Blur Blend Pass");
                cmd2.Clear();
                
                cmd2.SetRenderTarget(blurredOutlineRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd2.ClearRenderTarget(true, true, Color.clear);
                
                //properties
                blurBlendMat.SetColor("_OutlineColor", toonOutlinePostProcess.OutlineColor.value);
                blurBlendMat.SetFloat("radius", toonOutlinePostProcess.BlurRadius.value);
                blurBlendMat.SetFloat("use5x5", toonOutlinePostProcess.Use5x5.value ? 1 : 0);
                blurBlendMat.SetTexture("_BlitTexture", cameraHandleRT);
                blurBlendMat.SetTexture("_OutlineTexture", outlineRT);
                
                cmd2.DrawProcedural(Matrix4x4.identity, blurBlendMat, 0, MeshTopology.Triangles, 3, 1);
                cmd2.Blit(blurredOutlineRT, cameraHandleRT);
                
                context.ExecuteCommandBuffer(cmd2);
                context.Submit();

                CommandBufferPool.Release(cmd2);
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }
    }
}
