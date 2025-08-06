using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Code.Frameworks.Outline
{
    sealed partial class ToonOutlinePass : ScriptableRenderPass
    {
        new string passName = "ToonOutline";

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            var cameraColor = resourceData.cameraColor;
            if (!cameraColor.IsValid())
            {
                Debug.LogError("Camera color target is not valid. IdMap rendering will not work.");
                return;
            }
            
            var activeColorTexture = resourceData.activeColorTexture;
            if (!activeColorTexture.IsValid())
            {
                Debug.LogError("Active color texture is not valid. IdMap rendering will not work.");
                return;
            }

            VolumeStack volumes = VolumeManager.instance.stack;
            ToonOutlinePostProcess toonOutlinePostProcess = volumes.GetComponent<ToonOutlinePostProcess>();
            if (toonOutlinePostProcess.IsActive())
            {
                using (var builder =
                       renderGraph.AddUnsafePass<ToonOutlinePassData>(passName, out var passData, profilingSampler))
                {
                    builder.UseTexture(cameraColor, AccessFlags.Read);
                    builder.UseTexture(activeColorTexture, AccessFlags.ReadWrite);

                    // fill pass data
                    passData.toonOutlineMat = toonOutlineMat;
                    passData.blurBlendMat = blurBlendMat;
                    
                    passData.cameraColorHandle = cameraColor;
                    passData.activeColorTextureHandle = activeColorTexture;
                    
                    passData.outlineRT = outlineRT;

                    builder.SetRenderFunc((ToonOutlinePassData data, UnsafeGraphContext context) =>
                    {
                        CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                        cmd.SetRenderTarget(data.outlineRT, RenderBufferLoadAction.DontCare,
                            RenderBufferStoreAction.Store);
                        cmd.ClearRenderTarget(true, true, Color.clear);

                        //properties
                        data.toonOutlineMat.SetFloat("_DepthThreshold",
                            toonOutlinePostProcess.DepthThreshold.value);
                        data.toonOutlineMat.SetFloat("_NormalsThreshold",
                            toonOutlinePostProcess.NormalsThreshold.value);
                        data.toonOutlineMat.SetColor("_Color", toonOutlinePostProcess.OutlineColor.value);
                        data.toonOutlineMat.SetInt("_Radius", toonOutlinePostProcess.Radius.value);
                        data.toonOutlineMat.SetFloat("_AngleFixScale", toonOutlinePostProcess.AngleFixScale.value);
                        data.toonOutlineMat.SetFloat("_AngleFixPower", toonOutlinePostProcess.AngleFixPower.value);
                        data.toonOutlineMat.SetFloat("_FadeStart", toonOutlinePostProcess.FadeStart.value);
                        data.toonOutlineMat.SetFloat("_FadeEnd", toonOutlinePostProcess.FadeEnd.value);
                        data.toonOutlineMat.SetFloat("_DEBUGOUTLINE",
                            toonOutlinePostProcess.DebugOutline.value ? 1 : 0);
                        data.toonOutlineMat.SetFloat("_DebugTransparency",
                            toonOutlinePostProcess.DebugTransparency.value);

                        data.toonOutlineMat.SetTexture("_BlitTexture", data.activeColorTextureHandle);

                        cmd.DrawProcedural(Matrix4x4.identity, data.toonOutlineMat, 0, MeshTopology.Triangles, 
                            3, 1);

                        cmd.SetRenderTarget(data.activeColorTextureHandle, RenderBufferLoadAction.DontCare,
                            RenderBufferStoreAction.Store);
                        cmd.ClearRenderTarget(true, true, Color.clear);

                        //properties
                        data.blurBlendMat.SetColor("_OutlineColor", toonOutlinePostProcess.OutlineColor.value);
                        data.blurBlendMat.SetFloat("radius", toonOutlinePostProcess.BlurRadius.value);
                        data.blurBlendMat.SetFloat("use5x5", toonOutlinePostProcess.Use5x5.value ? 1 : 0);
                        data.blurBlendMat.SetTexture("_BlitTexture", data.cameraColorHandle);
                        data.blurBlendMat.SetTexture("_OutlineTexture", data.outlineRT);

                        cmd.DrawProcedural(Matrix4x4.identity, data.blurBlendMat, 0, MeshTopology.Triangles, 3, 1);
                        //Blitter.BlitCameraTexture(cmd, data.blurredOutlineRT, data.activeColorTextureHandle);
                    });
                }
            }
        }
    }

    internal class ToonOutlinePassData
    {
        internal Material toonOutlineMat;
        internal Material blurBlendMat;

        internal TextureHandle cameraColorHandle;
        internal TextureHandle activeColorTextureHandle;

        internal RTHandle outlineRT;
        // internal RTHandle blurredOutlineRT; not used right now, but might be needed if problems arise
    }
}