using UnityEngine;  
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

public class OutlineEffect : CustomPass
{
    public LayerMask outlineLayer;       
    public Color outlineColor = Color.cyan;
    public float thickness = 1.5f;
    private Material outlineMaterial;
    private GameObject targetObject;     
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {   
        outlineMaterial = new Material(Shader.Find("Renderers/OutlineEffect1"));
    }

    protected override void Execute(CustomPassContext ctx)
    {
        var camera = ctx.hdCamera.camera;
        var cmd = ctx.cmd;

        if (targetObject == null)
            return;

        if ((outlineLayer & (1 << targetObject.layer)) == 0)
        {
            Debug.LogWarning("OutlineEffect: Target object is not on the specified outline layer.");
            return;
        }
        var viewport = camera.pixelRect;
        int viewportWidth = Mathf.RoundToInt(viewport.width);
        int viewportHeight = Mathf.RoundToInt(viewport.height);

        RTHandle tempRT = RTHandles.Alloc(
            viewportWidth, viewportHeight, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.R8G8B8A8_UNorm,
            useDynamicScale: true, name: "OutlineMask");

        CoreUtils.SetRenderTarget(cmd, tempRT, ClearFlag.Color, Color.black);
        
        cmd.SetViewport(new Rect(0, 0, viewportWidth, viewportHeight));
        cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        
        Material maskMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        if (maskMaterial.shader.name == "Hidden/InternalErrorShader")
        {
            maskMaterial = new Material(Shader.Find("Unlit/Color"));
            maskMaterial.color = Color.white;
        }
        else
        {
            maskMaterial.color = Color.white;
        }
        
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.activeInHierarchy && ((outlineLayer & (1 << renderer.gameObject.layer)) != 0))
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    cmd.DrawRenderer(renderer, maskMaterial, i);
                }
            }
        }

        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_Thickness", thickness);
        outlineMaterial.SetTexture("_ObjectMask", tempRT);
        
        outlineMaterial.SetVector("_ViewportParams", new Vector4(viewportWidth, viewportHeight, 1.0f / viewportWidth, 1.0f / viewportHeight));

        CoreUtils.SetRenderTarget(cmd, ctx.cameraColorBuffer);
        cmd.SetViewport(viewport);
        HDUtils.DrawFullScreen(cmd, outlineMaterial, ctx.cameraColorBuffer);

        RTHandles.Release(tempRT);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(outlineMaterial);
    }

    public void SetTargetObject(GameObject obj)
    {
        targetObject = obj;
    }

    public void ClearTarget()
    {
        targetObject = null;
    }
}
