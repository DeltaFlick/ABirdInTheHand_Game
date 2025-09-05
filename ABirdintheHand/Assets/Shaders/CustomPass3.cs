using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public class CustomPass3 : CustomPass
{
    [Header("Shader Settings")]
    public Material shaderMaterial;
    public int playerID = 1;
    
    [Header("Render Settings")]
    public RenderQueueRange renderQueueRange = RenderQueueRange.all;
    public bool useDepthTest = true;
    
    private MaterialPropertyBlock propertyBlock;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (shaderMaterial == null || ctx.cmd == null || ctx.hdCamera?.camera == null)
        {
            return;
        }

        int currentCameraID = ctx.hdCamera.camera.GetInstanceID();
        
        shaderMaterial.SetInt("_PlayerCameraID", currentCameraID);
        shaderMaterial.SetInt("_PlayerID", playerID); 
        shaderMaterial.SetInt("_CurrentCameraID", currentCameraID);
        shaderMaterial.SetFloat("_IsVisible", 1f);
        shaderMaterial.SetFloat("_TimeStamp", Time.time);

        var rendererListDesc = new RendererListDesc(
            new ShaderTagId("Forward"),
            ctx.cullingResults,
            ctx.hdCamera.camera)
        {
            sortingCriteria = SortingCriteria.BackToFront,
            renderQueueRange = renderQueueRange,
            overrideMaterial = shaderMaterial,
            overrideMaterialPassIndex = 0,
            excludeObjectMotionVectors = false
        };

        var rendererList = ctx.renderContext.CreateRendererList(rendererListDesc);
        
        if (rendererList.isValid)
        {
            if (useDepthTest)
            {
                ctx.cmd.SetRenderTarget(ctx.cameraColorBuffer, ctx.cameraDepthBuffer);
            }
            else
            {
                ctx.cmd.SetRenderTarget(ctx.cameraColorBuffer);
            }

            ctx.cmd.DrawRendererList(rendererList);
        }
    }

    protected override void Cleanup()
    {
    }

    public void SetPlayerID(int newPlayerID)
    {
        playerID = newPlayerID;
    }

    public void SetShaderMaterial(Material newMaterial)
    {
        shaderMaterial = newMaterial;
    }
}