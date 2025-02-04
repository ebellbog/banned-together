using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

#if UNITY_2023_3_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace OccaSoftware.Outlines.Runtime
{
    class OutlineRenderPass : ScriptableRenderPass
    {
        //RTHandle source;
        RTHandle compute;
        RTHandle draw;

        ComputeShader shader;

        private readonly string targetId = "_OutlineTarget";
        private readonly string outlinesId = "_OutlineDrawTarget";
        private readonly string computeKernelName = "ComputeOutlines";
        private readonly string drawKernelName = "DrawOutlines";
        private readonly string shaderFilename = "OutlinesCompute";
        private readonly string profilerTag = "Outlines Render Pass";
        private readonly string bufferPoolName = "_OutlinesPass";

        public OutlineRenderPass()
        {
            compute = RTHandles.Alloc(targetId, name: targetId);
            draw = RTHandles.Alloc(outlinesId, name: outlinesId);
        }

        int computeKernel;
        int drawKernel;
        int groupsX,
          groupsY;

        private int GetGroupCount(int textureDimension, uint groupSize)
        {
            return Mathf.CeilToInt((textureDimension + groupSize - 1) / groupSize);
        }

        public bool LoadComputeShader()
        {
            if (shader != null)
                return true;

            shader = (ComputeShader)Resources.Load(shaderFilename);
            if (shader == null)
                return false;

            return true;
        }

        OutlineVolumeSettings outlineSettings;

#if UNITY_2023_3_OR_NEWER
        private class PassData
        {
            internal TextureHandle source;
            internal RenderTextureDescriptor descriptor;
            internal Camera camera;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Setting up the render pass in RenderGraph
            using (var builder = renderGraph.AddUnsafePass<PassData>(profilerTag, out var passData))
            {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                RenderTextureDescriptor descriptor = ConfigurePass(cameraData.cameraTargetDescriptor);

                passData.source = resourceData.cameraColor;
                passData.descriptor = descriptor;
                passData.camera = cameraData.camera;

                builder.UseTexture(passData.source, AccessFlags.ReadWrite);

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        private void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            ExecutePass(cmd, data.source, data.camera, data.descriptor);
        }
#endif

#if UNITY_2023_3_OR_NEWER
        [Obsolete]
#endif
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigurePass(renderingData.cameraData.cameraTargetDescriptor);
        }

        public RenderTextureDescriptor ConfigurePass(RenderTextureDescriptor cameraTextureDescriptor)
        {
            LoadComputeShader();

            RenderTextureDescriptor descriptor = cameraTextureDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.enableRandomWrite = true;
            descriptor.msaaSamples = 1;
            descriptor.sRGB = false;

            descriptor.width = Mathf.Max(1, descriptor.width);
            descriptor.height = Mathf.Max(1, descriptor.height);
            RenderTextureDescriptor targetDescriptor = descriptor;
            targetDescriptor.colorFormat = RenderTextureFormat.R16;

            RenderingUtilsHelper.ReAllocateIfNeeded(
              ref compute,
              targetDescriptor,
              FilterMode.Point,
              TextureWrapMode.Clamp,
              name: targetId
            );

            RenderingUtilsHelper.ReAllocateIfNeeded(
              ref draw,
              descriptor,
              FilterMode.Point,
              TextureWrapMode.Clamp,
              name: outlinesId
            );

            computeKernel = shader.FindKernel(computeKernelName);
            drawKernel = shader.FindKernel(drawKernelName);
            shader.GetKernelThreadGroupSizes(
              computeKernel,
              out uint threadGroupSizeX,
              out uint threadGroupSizeY,
              out _
            );
            groupsX = GetGroupCount(descriptor.width, threadGroupSizeX);
            groupsY = GetGroupCount(descriptor.height, threadGroupSizeY);

            return descriptor;
        }

#if UNITY_2023_3_OR_NEWER
        [Obsolete]
#endif
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Profiler.BeginSample(profilerTag);

            CommandBuffer cmd = CommandBufferPool.Get(bufferPoolName);

            ExecutePass(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, renderingData.cameraData.camera, renderingData.cameraData.cameraTargetDescriptor);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            Profiler.EndSample();
        }


        public void ExecutePass(CommandBuffer cmd, RTHandle source, Camera camera, RenderTextureDescriptor descriptor)
        {
            if (outlineSettings == null)
            {
                VolumeStack stack = VolumeManager.instance.stack;
                outlineSettings = stack.GetComponent<OutlineVolumeSettings>();
            }
            if (outlineSettings == null)
                return;

            if (!outlineSettings.IsActive())
                return;

            float halfTanFov = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
            float aspectRatio = camera.pixelHeight / (float)camera.pixelWidth;

            float invAspectRatio = 1.0f / aspectRatio;

            cmd.SetGlobalFloat("_FrameId", Time.frameCount);

            Vector4 DepthToViewParams = new Vector4(
              2.0f * halfTanFov * invAspectRatio,
              2 * halfTanFov,
              halfTanFov * invAspectRatio,
              halfTanFov
            );
            cmd.SetGlobalVector(ShaderParams.DepthToViewParams, DepthToViewParams);

            cmd.SetComputeTextureParam(shader, computeKernel, ShaderParams.ScreenTexture, source);
            cmd.SetComputeTextureParam(shader, computeKernel, ShaderParams.OutlineTarget, compute);

            Vector4 screenSizePx = new Vector4(
              descriptor.width,
              descriptor.height,
              1.0f / descriptor.width,
              1.0f / descriptor.height
            );
            cmd.SetComputeVectorParam(shader, ShaderParams.ScreenDimensions, screenSizePx);

            float ratio = 1;
            if (outlineSettings.resolutionScaling.value != (int)ReferenceResolution.Disabled)
            {
                int s = (int)outlineSettings.resolutionScaling.value;
                int c = descriptor.height;
                ratio = (float)c / (float)s;
            }

            cmd.SetGlobalFloat(
              ShaderParams.MaximumOutlineThickness,
              outlineSettings.maximumOutlineThickness.value * ratio
            );
            cmd.SetGlobalFloat(ShaderParams.ExcludeSkybox, outlineSettings.excludeSkybox.value ? 1 : 0);

            cmd.SetGlobalFloat(ShaderParams.DepthThreshold, outlineSettings.depthThreshold.value);
            cmd.SetGlobalFloat(ShaderParams.DepthSoftness, outlineSettings.depthSoftness.value);
            cmd.SetGlobalFloat(ShaderParams.DepthThickness, outlineSettings.depthThickness01.value);
            if (outlineSettings.depthThickness01.value > 0)
            {
                Shader.EnableKeyword(ShaderParams.DepthEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.DepthEnabled);
            }

            cmd.SetGlobalFloat(ShaderParams.NormalThreshold, outlineSettings.normalThreshold.value);
            cmd.SetGlobalFloat(ShaderParams.NormalSoftness, outlineSettings.normalSoftness.value);
            cmd.SetGlobalFloat(ShaderParams.NormalThickness, outlineSettings.normalThickness01.value);
            if (outlineSettings.normalThickness01.value > 0)
            {
                Shader.EnableKeyword(ShaderParams.NormalEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.NormalEnabled);
            }

            cmd.SetGlobalFloat(ShaderParams.LumaThreshold, outlineSettings.lumaThreshold.value);
            cmd.SetGlobalFloat(ShaderParams.LumaSoftness, outlineSettings.lumaSoftness.value);
            cmd.SetGlobalFloat(ShaderParams.LumaThickness, outlineSettings.lumaThickness01.value);

            if (outlineSettings.lumaThickness01.value > 0)
            {
                Shader.EnableKeyword(ShaderParams.LumaEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.LumaEnabled);
            }

            cmd.SetGlobalFloat(ShaderParams.ColorThreshold, outlineSettings.colorThreshold.value);
            cmd.SetGlobalFloat(ShaderParams.ColorSoftness, outlineSettings.colorSoftness.value);
            cmd.SetGlobalFloat(ShaderParams.ColorThickness, outlineSettings.colorThickness01.value);
            if (outlineSettings.colorThickness01.value > 0)
            {
                Shader.EnableKeyword(ShaderParams.ColorEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.ColorEnabled);
            }

            cmd.SetGlobalColor(ShaderParams.OutlineColor, outlineSettings.outlineColor.value);
            cmd.SetGlobalFloat(ShaderParams.NearThickness, outlineSettings.nearThickness01.value);
            cmd.SetGlobalFloat(ShaderParams.FarThickness, outlineSettings.farThickness01.value);
            cmd.SetGlobalFloat(ShaderParams.NearAlpha, outlineSettings.nearAlpha.value);
            cmd.SetGlobalFloat(ShaderParams.FarAlpha, outlineSettings.farAlpha.value);

            cmd.SetGlobalFloat(ShaderParams.DistanceStart, outlineSettings.distanceStart.value);
            cmd.SetGlobalFloat(ShaderParams.DistanceEnd, outlineSettings.distanceEnd.value);

            cmd.SetGlobalFloat(
              ShaderParams.GrazingAngleThreshold,
              outlineSettings.grazingAngleThreshold.value
            );
            cmd.SetGlobalFloat(ShaderParams.GrazingAngleOffset, outlineSettings.grazingAngleOffset.value);
            cmd.SetGlobalFloat(ShaderParams.EdgeThreshold, outlineSettings.edgeThreshold.value);

            cmd.SetGlobalFloat(
              ShaderParams.PreviewOutlines,
              outlineSettings.previewOutlines.value ? 1 : 0
            );

            cmd.DispatchCompute(shader, computeKernel, groupsX, groupsY, 1);

            cmd.SetComputeTextureParam(shader, drawKernel, ShaderParams.ScreenTexture, source);
            cmd.SetComputeTextureParam(shader, drawKernel, ShaderParams.OutlineTargetResource, compute);
            cmd.SetComputeTextureParam(shader, drawKernel, ShaderParams.OutlineDrawTarget, draw);
            cmd.SetGlobalFloat("os_Time", Time.time);
            cmd.SetGlobalFloat(ShaderParams.FrameRate, outlineSettings.frameRate.value);
            cmd.SetGlobalFloat(ShaderParams.InverseFrameRate, 1f / outlineSettings.frameRate.value);

            cmd.SetComputeTextureParam(
              shader,
              drawKernel,
              ShaderParams.OffsetTexture,
              outlineSettings.offsetTexture.value
                ? outlineSettings.offsetTexture.value
                : Texture2D.blackTexture
            );

            cmd.SetGlobalFloat(ShaderParams.TextureSpeed, outlineSettings.offsetTextureSpeed.value);
            cmd.SetGlobalFloat(
              ShaderParams.TextureScale,
              1.0f / outlineSettings.offsetTextureScale.value
            );
            cmd.SetGlobalFloat(ShaderParams.TextureOffset, outlineSettings.offsetTextureStrength.value);
            if (
              outlineSettings.offsetTexture.value != null
              && outlineSettings.offsetTextureStrength.value != 0
            )
            {
                Shader.EnableKeyword(ShaderParams.OffsetEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.OffsetEnabled);
            }

            cmd.SetGlobalFloat(
              ShaderParams.OffsetExcludeSkybox,
              outlineSettings.offsetExcludeSkybox.value ? 1 : 0
            );

            cmd.SetComputeTextureParam(
              shader,
              drawKernel,
              ShaderParams.PaperTexture,
              outlineSettings.paperTexture.value
                ? outlineSettings.paperTexture.value
                : Texture2D.whiteTexture
            );

            cmd.SetGlobalFloat(ShaderParams.PaperTextureSpeed, outlineSettings.paperTextureSpeed.value);
            cmd.SetGlobalFloat(
              ShaderParams.PaperTextureScale,
              1.0f / outlineSettings.paperTextureScale.value
            );
            cmd.SetGlobalFloat(
              ShaderParams.PaperTextureStrength,
              outlineSettings.paperTextureStrength.value
            );
            cmd.SetGlobalFloat(
              ShaderParams.PaperExcludeSkybox,
              outlineSettings.paperExcludeSkybox.value ? 1 : 0
            );

            if (
              outlineSettings.paperTexture.value != null
              && outlineSettings.paperTextureStrength.value != 0
            )
            {
                Shader.EnableKeyword(ShaderParams.PaperEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.PaperEnabled);
            }

            cmd.SetGlobalFloat(ShaderParams.BlurAmount, outlineSettings.blurAmount.value);
            if (outlineSettings.blurAmount.value > 0)
            {
                Shader.EnableKeyword(ShaderParams.BlurEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.BlurEnabled);
            }

            if (outlineSettings.blurNoiseEnabled.value)
            {
                Shader.EnableKeyword(ShaderParams.BlurNoiseEnabled);
            }
            else
            {
                Shader.DisableKeyword(ShaderParams.BlurNoiseEnabled);
            }

            cmd.DispatchCompute(shader, drawKernel, groupsX, groupsY, 1);

            Blitter.BlitCameraTexture(cmd, draw, source);

        }
        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            compute?.Release();
            compute = null;

            shader = null;
        }
    }
}
