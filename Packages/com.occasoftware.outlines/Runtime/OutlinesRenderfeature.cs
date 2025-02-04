using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Outlines.Runtime
{
    public class OutlinesRenderfeature : ScriptableRendererFeature
    {

        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        public bool overridePostProcessingBoolean = false;

        public int renderPassEventOffset;
        public bool renderInSceneView = true;

        private OutlineRenderPass outlineRenderPass;

        public override void Create()
        {
            outlineRenderPass = new OutlineRenderPass();
            outlineRenderPass.renderPassEvent = renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType == CameraType.Reflection)
                return;

            if (renderingData.cameraData.camera.cameraType == CameraType.Preview)
                return;

            if (!renderingData.cameraData.postProcessEnabled && !overridePostProcessingBoolean)
                return;

            renderer.EnqueuePass(outlineRenderPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            outlineRenderPass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Depth);
        }
    }
}
