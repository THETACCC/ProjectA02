using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GetTheBGText : ScriptableRendererFeature
{
    
    class GrabScreenPass : ScriptableRenderPass
    {
        
        private RenderTargetIdentifier cameraColorTargetIdent;
        private RenderTextureDescriptor cameraTextureDescriptor;
        private RenderTexture grabTexture;

        public GrabScreenPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public void Setup(RenderTargetIdentifier cameraColorTarget, RenderTextureDescriptor descriptor)
        {
            this.cameraColorTargetIdent = cameraColorTarget;
            this.cameraTextureDescriptor = descriptor;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (grabTexture == null || grabTexture.width != cameraTextureDescriptor.width || grabTexture.height != cameraTextureDescriptor.height)
            {
                if (grabTexture != null)
                    grabTexture.Release();

                grabTexture = new RenderTexture(cameraTextureDescriptor);
                grabTexture.Create();
            }

            CommandBuffer cmd = CommandBufferPool.Get("Grab Screen Texture");
            cmd.Blit(cameraColorTargetIdent, grabTexture);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            Shader.SetGlobalTexture("_BackgroundTex", grabTexture);
        }
    }

    private GrabScreenPass grabScreenPass;

    public override void Create()
    {
        grabScreenPass = new GrabScreenPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        grabScreenPass.Setup(renderer.cameraColorTarget, renderingData.cameraData.cameraTargetDescriptor);
        renderer.EnqueuePass(grabScreenPass);
    }
    
}
